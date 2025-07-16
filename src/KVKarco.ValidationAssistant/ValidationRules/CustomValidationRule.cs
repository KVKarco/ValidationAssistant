using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant.ValidationRules;

public abstract class CustomValidationRule<T, TExternalResources, TProperty> :
    ValidationRule<T, TExternalResources, TProperty>,
    IValidationRule<T, TExternalResources, TProperty>
{
    protected CustomValidationRule(bool canRunSynchronously)
       : base(true)
    {
    }

    public abstract bool IsValid(IValidationCtx<T, TExternalResources> context, TProperty value);

    internal sealed override void Validate(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo)
    {
        RuleCreationException.ThrowIfPropertyValueIsMissing(context, property, failureInfo);

        if (!IsValid(context, property.Value))
        {
            context.AddValidationRuleFailure(property, failureInfo);
        }
    }


    internal sealed override ValueTask ValidateAsync(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo,
        CancellationToken ct)
    {
        RuleCreationException.ThrowIfPropertyValueIsMissing(context, property, failureInfo);

        if (!IsValid(context, property.Value))
        {
            context.AddValidationRuleFailure(property, failureInfo);
        }

        return ValueTask.CompletedTask;
    }
}
