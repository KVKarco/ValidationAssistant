using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;

namespace KVKarco.ValidationAssistant.Internal.PropertyValidation.GenericValidationRules;

internal sealed class ConditionalFlowValidationRule<T, TExternalResources, TProperty> :
    ValidationComponent<T, TExternalResources, TProperty>
{
    private readonly ValidationCondition<T, TExternalResources>? _condition;
    private readonly AsyncValidationCondition<T, TExternalResources>? _asyncCondition;

    public ConditionalFlowValidationRule(ValidationCondition<T, TExternalResources> condition)
        : base(true) => _condition = condition;

    public ConditionalFlowValidationRule(AsyncValidationCondition<T, TExternalResources> asyncCondition)
        : base(false) => _asyncCondition = asyncCondition;

    public sealed override ReadOnlySpan<char> RuleName
        => CanRunSynchronously ? DefaultCodes.ConditionalValRule : DefaultCodes.ConditionalAsyncValRule;

    public sealed override string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value)
        => ValidatorsConfig.GlobalDefaults.Messages.LogicalComponentStopExplanation(context, value);

    internal sealed override void Validate(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo)
    {
        if (!CanRunSynchronously)
        {
            context.ForceStopAsyncValidationRuleCalledSynchronously(property, failureInfo);
        }
        else if (!_condition!(context))
        {
            context.AddValidationRuleFailure(property, failureInfo);
        }
    }

    internal sealed override async ValueTask ValidateAsync(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo,
        CancellationToken ct)
    {
        bool isValid = CanRunSynchronously
            ? _condition!(context)
            : await _asyncCondition!(context, ct).ConfigureAwait(false);

        if (!isValid)
        {
            context.AddValidationRuleFailure(property, failureInfo);
        }
    }
}
