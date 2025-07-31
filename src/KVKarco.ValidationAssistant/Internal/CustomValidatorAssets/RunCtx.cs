using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;
using System.Collections.Immutable;
using System.Globalization;

namespace KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;

internal sealed class CustomValidatorRunCtx<T, TExternalResources> :
    ValidatorRunCtx<T, TExternalResources>
{
    private PropertyKey _propertyKey;

    public CustomValidatorRunCtx(
        string fromValidator,
        T value,
        TExternalResources resources,
        ImmutableArray<string>? availableSnapShots,
        CultureInfo culture,
        ValidatorRunResult? result,
        ValidatorRunCtx? parentContext)
        : base(
            availableSnapShots,
            value,
            resources,
            fromValidator,
            culture,
            parentContext,
            result ?? new(fromValidator))
    {
        _propertyKey = PropertyKey.Empty;
    }

    public override ReadOnlySpan<char> PropertyName => _propertyKey.Info is null ? [] : _propertyKey.PropertyName;

    public override string CorrectPropertyPath => ParentContext is not null
        ? $"{ParentContext.CorrectPropertyPath}.{_propertyKey.PropertyPath}"
        : _propertyKey.PropertyPath;

    public void ForProperty(PropertyKey key, int declarationLine, FlowEffect effect)
    {
        _propertyKey = key;
        _componentFailures = 0;
        _componentsToSkip = 0;
        _onPropertyValidatorFlowImpact = FlowEffect.Proceed;
        _onValidatorFlowImpact = effect;
        _executionLog = ValidatorComponentLog.ForProperty(_validatorName, CorrectPropertyPath, declarationLine);
        Result.AddComponentLog(_executionLog);
    }

    public void AddRuleSuccess(int declarationLine)
    {
        if (_executionLog is null)
        {
            throw new ValidationAssistantInternalException("Debug how AddRuleSuccess is call with null componentLog");
        }
        _executionLog.RuleSucceeded(declarationLine);
        _onPropertyValidatorFlowImpact = null;
    }

    public void AddRuleFailure(int declarationLine, string code, Severity severity, string failureMessage, FlowEffect effect)
    {
        if (_executionLog is null)
        {
            throw new ValidationAssistantInternalException("Debug how AddRuleFailure is call with null componentLog");
        }
        _executionLog.Status = ExecutionStatus.Failed;
        _executionLog.RuleFailed(declarationLine, code, severity, failureMessage);
        _executionLog.Explanation = "One or more rules failed.";
        _onPropertyValidatorFlowImpact = effect;
        _componentFailures++;
    }

    public void AddRuleInfo(int declarationLine, string explanation, FlowEffect effect)
    {
        if (_executionLog is null)
        {
            throw new ValidationAssistantInternalException("Debug how AddRuleInfo is call with null componentLog");
        }
        _executionLog.RuleInformational(declarationLine, explanation);
        _onPropertyValidatorFlowImpact = effect;
    }
}
