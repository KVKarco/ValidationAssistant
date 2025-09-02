using KVKarco.ValidationAssistant.Internal.Utilities.TargetAssets;
using System.Collections.Immutable;

namespace KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;

internal sealed class CustomValidatorPropertyRule<T, TExternalResources, TProperty> :
    IValidatorComponent<T, TExternalResources, CustomValidatorRunCtx<T, TExternalResources>>
{
    private readonly TargetCtx<T, TProperty> _propertyContext;
    private readonly ImmutableArray<ValidationRule<T, TExternalResources, TProperty>> _validationRules;
    private readonly ImmutableArray<ValidationRuleMetaData<T, TExternalResources, TProperty>> _validationRulesMetaData;

    public CustomValidatorPropertyRule(
        TargetCtx<T, TProperty> propertyContext,
        List<ValidationRule<T, TExternalResources, TProperty>> validationRules,
        List<ValidationRuleMetaData<T, TExternalResources, TProperty>> validationRulesMetaData)
    {
        _propertyContext = propertyContext;
        _validationRules = [.. validationRules];
        _validationRulesMetaData = [.. validationRulesMetaData];
    }

    public void Execute(CustomValidatorRunCtx<T, TExternalResources> context, ValidatorComponentMetaData metaData)
    {
        context.ForProperty(_propertyContext.Key);
        Undefined<TProperty> property = _propertyContext.ExtractValue(context.Subject);

        if (!property.IsDefined)
        {
            context.AddComponentFailure(metaData, "PropertyValidator is skipped, value cannot be extracted.");
        }

        int index = 0;

        while (index < _validationRules.Length)
        {
            _validationRules[index].Validate(context, property, _validationRulesMetaData[index]);

            // Check if the current validation strategy or a critical failure
            // requires exiting further validation for this property rule.
            if (context.ToStopPropertyValidator())
            {
                break; // Exit the loop, stopping further property rule component validation.
            }

            index++;
        }

        if (context.IsPropertyValidatorValid)
        {
            // add info that the property validator ran successfully
        }
        else
        {
            context.AddComponentFailure(metaData, "PropertyValidator has on or more failed ValidationRules.");

        }


        throw new NotImplementedException();
    }

    public ValueTask ExecuteAsync(CustomValidatorRunCtx<T, TExternalResources> context, ValidatorComponentMetaData metaData, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    protected override void StartValidation(CustomValidatorRunCtx<T, TExternalResources> context, out Undefined<TProperty> property)
    {

        property = _propertyContext.ExtractValue(context.Subject); // Extract the property value
    }
}

