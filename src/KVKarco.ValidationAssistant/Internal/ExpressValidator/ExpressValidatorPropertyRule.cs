using KVKarco.ValidationAssistant.Internal.PropertyValidation;

namespace KVKarco.ValidationAssistant.Internal.ExpressValidator;


/// <summary>
/// Represents a sealed, concrete implementation of a <see cref="PropertyRule{T, TExternalResources, TProperty, TContext}"/>
/// specifically designed for <see cref="ExpressValidator{T, TExternalResources}"/> contexts.
/// This rule is responsible for extracting a property's value from the main instance,
/// updating the validation context with property-specific information, and
/// then executing a sequence of <see cref="PropertyRuleComponent{T, TExternalResources, TProperty}"/>s
/// against that property. It properly handles cases where the property value itself is missing
/// or cannot be resolved, stopping further component validation for that property if it's not present.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property this rule applies to.</typeparam>
internal sealed class ExpressValidatorPropertyRule<T, TExternalResources, TProperty> :
    PropertyRule<T, TExternalResources, TProperty, ExpressValidatorRunCtx<T, TExternalResources>>
{
    /// <summary>
    /// Stores the <see cref="PropertyCtx{T, TProperty}"/> which contains both the <see cref="PropertyKey"/>
    /// (metadata about the property) and the <see cref="PropertyValueResolver{T, TProperty}"/> (delegate to extract the value).
    /// This context is used to efficiently retrieve property information and its value during validation.
    /// </summary>
    private readonly PropertyCtx<T, TProperty> _propertyContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressValidatorPropertyRule{T, TExternalResources, TProperty}"/> class.
    /// </summary>
    /// <param name="propertyContext">
    /// The <see cref="PropertyCtx{T, TProperty}"/> containing the property's metadata and its value resolver.
    /// This is typically created by a builder that compiles the property selection expression.
    /// </param>
    /// <param name="validatorName">The name of the validator (e.g., "UserValidator"). This is inherited by the base <see cref="PropertyRule"/>.</param>
    /// <param name="strategy">The <see cref="RuleFailureStrategy"/> to apply if this property rule fails. This is inherited by the base <see cref="PropertyRule"/>.</param>
    /// <param name="declaredOnLine">The line number where this property rule was declared in the validator definition. Used for debugging/reporting. This is inherited by the base <see cref="PropertyRule"/>.</param>
    /// <param name="ruleComponents">
    /// A list of <see cref="PropertyRuleComponent{T, TExternalResources, TProperty}"/> instances
    /// that define the individual validation checks for this property. These components are executed sequentially.
    /// </param>
    public ExpressValidatorPropertyRule(
        PropertyCtx<T, TProperty> propertyContext,
        ReadOnlySpan<char> validatorName,
        RuleFailureStrategy strategy,
        int declaredOnLine,
        List<PropertyRuleComponent<T, TExternalResources, TProperty>> ruleComponents)
        : base(validatorName, strategy, declaredOnLine, ruleComponents)
    {
        _propertyContext = propertyContext;
    }

    /// <summary>
    /// Extracts the property's value from the <paramref name="context"/>'s validation instance
    /// using the encapsulated <see cref="PropertyValueResolver{T, TProperty}"/>.
    /// This method also updates the <paramref name="context"/> to indicate which property is currently being validated
    /// by *this specific PropertyRule instance*.
    /// <para>
    /// If the property value cannot be resolved (e.g., due to a null intermediate in a property chain,
    /// resulting in an <see cref="Undefined{TProperty}"/> instance with <see cref="Undefined{TProperty}.HasValue"/> being <see langword="false"/>),
    /// a <c>PropertyRuleMissingValueFailure</c> is added to the context, and this method returns <see langword="false"/>
    /// to prevent further execution of <see cref="PropertyRuleComponent{T, TExternalResources, TProperty}"/> instances.
    /// </para>
    /// </summary>
    /// <param name="context">The <see cref="ExpressValidatorRunCtx{T, TExternalResources}"/> for the current validation run.
    /// This context provides the main instance being validated and accumulates results.</param>
    /// <param name="property">
    /// When this method returns, contains the extracted property value wrapped in an <see cref="Undefined{TProperty}"/> instance.
    /// This parameter will never be <see langword="null"/>; it will always contain a valid <see cref="Undefined{TProperty}"/>
    /// instance representing either a defined value or an undefined state.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the property value could be successfully extracted (even if its value is <see langword="null"/>)
    /// and is available for validation by its components; otherwise, <see langword="false"/> if the property
    /// is considered "missing" or "not applicable" in a way that should prevent component execution.
    /// </returns>
    protected override bool HasValue(ExpressValidatorRunCtx<T, TExternalResources> context, out Undefined<TProperty> property)
    {
        // Set the context's current property information to reflect THIS rule's property.
        // This makes `context.PropertyName` and `context.CorrectPropertyPath` accurate for messages
        // generated by subsequent components within this property rule.
        context.ForProperty(_propertyContext.Key, Info);

        // Attempt to get the value using the encapsulated property value resolver.
        property = _propertyContext.ExtractValue(context.ValidationInstance);

        // If the property is defined (even if its value is null, which can be validated by components like NotNull),
        // return true to proceed with component validation.
        if (property.HasValue)
        {
            return true;
        }

        // If the property is not defined (e.g., a null in a property chain was encountered),
        // add a specific failure indicating a missing value for the property rule itself,
        // and return false to stop further component execution for this property.
        context.AddPropertyRuleMissingValueFailure(property, Info);

        return false;
    }
}
