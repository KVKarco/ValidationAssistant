using KVKarco.ValidationAssistant.Internal.PropertyValidation;
using KVKarco.ValidationAssistant.ValidationRules;

namespace KVKarco.ValidationAssistant.Internal.ExpressValidatorComponents;

/// <summary>
/// Represents a concrete implementation of a property validation rule specifically designed
/// for validators created from ExpressValidator. It binds the property validation to the
/// <see cref="ExpressValidatorRunCtx{T, TExternalResources}"/> context, extracting the
/// property value in an ExpressValidator-specific manner.
/// </summary>
/// <typeparam name="T">The type of the root entity being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources used during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property this rule is validating.</typeparam>
internal sealed class ExpressValidatorPropertyRule<T, TExternalResources, TProperty> :
    PropertyRule<T, TExternalResources, TProperty, ExpressValidatorRunCtx<T, TExternalResources>>
{
    private readonly PropertyCtx<T, TProperty> _propertyContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressValidatorPropertyRule{T, TExternalResources, TProperty}"/> class.
    /// </summary>
    /// <param name="propertyContext">The context providing details about the property, including its key and value extraction logic.</param>
    /// <param name="validatorName">The name of the validator declaring this property rule.</param>
    /// <param name="strategy">The <see cref="RuleFailureStrategy"/> to apply for this property rule.</param>
    /// <param name="declaredOnLine">The line number in the source code where this property rule was declared.</param>
    /// <param name="validationRules">A list of specific <see cref="ValidationRule{T, TExternalResources, TProperty}"/> instances for the property.</param>
    /// <param name="validationRulesFailureInfo">A list of corresponding <see cref="ValidationRuleFailureInfo{T, TExternalResources, TProperty}"/> instances.</param>
    public ExpressValidatorPropertyRule(
        PropertyCtx<T, TProperty> propertyContext,
        ReadOnlySpan<char> validatorName,
        RuleFailureStrategy strategy,
        int declaredOnLine,
        List<ValidationRule<T, TExternalResources, TProperty>> validationRules,
        List<ValidationRuleFailureInfo<T, TExternalResources, TProperty>> validationRulesFailureInfo)
        : base(validatorName, strategy, declaredOnLine, validationRules, validationRulesFailureInfo)
    {
        _propertyContext = propertyContext;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Overrides the base method to initiate the validation process specifically for ExpressValidator.
    /// It prepares the context for property-level validation using <see cref="PropertyCtx{T, TProperty}.Key"/>
    /// and extracts the property's value from the validation instance.
    /// </summary>
    protected override void StartValidation(ExpressValidatorRunCtx<T, TExternalResources> context, out Undefined<TProperty> property)
    {
        context.ForProperty(_propertyContext.Key, Info); // Set the context for the specific property
        property = _propertyContext.ExtractValue(context.ValidationInstance); // Extract the property value
    }
}