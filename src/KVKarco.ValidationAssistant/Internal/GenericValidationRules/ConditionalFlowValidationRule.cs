using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.ValidationRules;

namespace KVKarco.ValidationAssistant.Internal.GenericValidationRules;

internal sealed class ConditionalFlowValidationRule<T, TExternalResources, TProperty> :
    ValidationRule<T, TExternalResources, TProperty>
{
    private readonly ValidationCondition<T, TExternalResources>? _condition;
    private readonly AsyncValidationCondition<T, TExternalResources>? _asyncCondition;

    public ConditionalFlowValidationRule(ValidationCondition<T, TExternalResources> condition)
        : base(true) => _condition = condition;

    public ConditionalFlowValidationRule(AsyncValidationCondition<T, TExternalResources> asyncCondition)
        : base(false) => _asyncCondition = asyncCondition;

    public sealed override ReadOnlySpan<char> RuleName => CanRunSynchronously ? DefaultNaming.ConditionalValRule : DefaultNaming.ConditionalAsyncValRule;

    public sealed override string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value)
        => ValidatorsConfig.GlobalDefaults.Messages.LogicalComponentStopExplanation(context, value);

    internal sealed override void Validate(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo)
    {
        throw new NotImplementedException();
    }

    internal sealed override ValueTask ValidateAsync(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
