using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;

namespace KVKarco.ValidationAssistant.Internal.PropertyValidation.GenericValidationRules;

internal sealed class ContextAwarePredicateValidationRule<T, TExternalResources, TProperty> :
    ValidationComponent<T, TExternalResources, TProperty>
{
    private readonly ContextAwarePropertyPredicate<T, TExternalResources, TProperty>? _predicate;
    private readonly AsyncContextAwarePropertyPredicate<T, TExternalResources, TProperty>? _asyncPredicate;

    public ContextAwarePredicateValidationRule(ContextAwarePropertyPredicate<T, TExternalResources, TProperty> predicate)
        : base(true) => _predicate = predicate;

    public ContextAwarePredicateValidationRule(AsyncContextAwarePropertyPredicate<T, TExternalResources, TProperty> asyncPredicate)
       : base(false) => _asyncPredicate = asyncPredicate;

    public sealed override ReadOnlySpan<char> RuleName
        => CanRunSynchronously ? DefaultCodes.ContextAwarePropertyValRule : DefaultCodes.ContextAwarePredicateAsyncValRule;

    public sealed override string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value)
       => ValidatorsConfig.GlobalDefaults.Messages.ValidationComponentError(context, value);

    internal sealed override void Validate(
            ValidatorRunCtx<T, TExternalResources> context,
            Undefined<TProperty> property,
            ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo)
    {
        if (!CanRunSynchronously)
        {
            context.ForceStopAsyncValidationRuleCalledSynchronously(property, failureInfo);
        }
        else if (!property.HasValue)
        {
            context.ForceStopPropertyValueIsMissing(property);
        }
        else if (!_predicate!(context, property.Value))
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
        if (!property.HasValue)
        {
            context.ForceStopPropertyValueIsMissing(property);
        }
        else if (!(CanRunSynchronously ? _predicate!(context, property.Value) : await _asyncPredicate!(context, property.Value, ct).ConfigureAwait(false)))
        {
            context.AddValidationRuleFailure(property, failureInfo);
        }
    }
}
