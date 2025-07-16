using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant.ValidationRules;

public abstract class CustomAsyncValidationRule<T, TExternalResources, TProperty> :
    ValidationRule<T, TExternalResources, TProperty>,
    IAsyncValidationRule<T, TExternalResources, TProperty>
{
    protected CustomAsyncValidationRule(bool canRunSynchronously)
       : base(false)
    {
    }

    public abstract Task<bool> IsValidAsync(IValidationCtx<T, TExternalResources> context, TProperty value, CancellationToken ct);

    internal sealed override void Validate(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo)
    {
        throw new ValidationRunException(
            $"Cannot run validation synchronously for rule '{RuleName}' because it is an asynchronous rule. " +
            "Use the ValidateAsync method instead.");
    }

    internal sealed override async ValueTask ValidateAsync(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo,
        CancellationToken ct)
    {
        RuleCreationException.ThrowIfPropertyValueIsMissing(context, property, failureInfo);

        if (!await IsValidAsync(context, property.Value, ct).ConfigureAwait(false))
        {
            context.AddValidationRuleFailure(property, failureInfo);
        }
    }
}