using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal.PropertyValidation;

namespace KVKarco.ValidationAssistant.Internal.ExpressValidatorComponents;

/// <summary>
/// Represents the concrete builder for defining validation rules for a specific property
/// within the validation framework. It extends the abstract <see cref="PropertyRuleBuilder{T, TExternalResources, TProperty}"/>
/// and implements <see cref="IBuildableRule{T, TExternalResources, TContext}"/> to produce a
/// compiled <see cref="ExpressValidatorPropertyRule{T, TExternalResources, TProperty}"/>.
/// This class orchestrates the collection of individual validation components and their
/// configurations for a single property.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the rule is being built.</typeparam>
internal sealed class ExpressValidatorPropertyRuleBuilder<T, TExternalResources, TProperty> :
    PropertyRuleBuilder<T, TExternalResources, TProperty>,
    IBuildableRule<T, TExternalResources, ExpressValidatorRunCtx<T, TExternalResources>>
{
    private readonly string _validatorName;
    private readonly PropertyCtx<T, TProperty> _propertyCtx;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressValidatorPropertyRuleBuilder{T, TExternalResources, TProperty}"/> class.
    /// </summary>
    /// <param name="validatorName">The name of the validator this property rule belongs to.</param>
    /// <param name="propertyCtx">The property context containing information about the property being validated.</param>
    /// <param name="defaultRuleFailureStrategy">The default rule failure strategy inherited from the main validator builder.</param>
    /// <param name="defaultComponentFailureStrategy">The default component failure strategy inherited from the main validator builder.</param>
    /// <param name="declaredOnLine">The line number in the source code where this property rule was declared.</param>
    /// <param name="snapShots">A reference to the list of snapshots managed by the main validator builder.</param>
    public ExpressValidatorPropertyRuleBuilder(
        string validatorName,
        PropertyCtx<T, TProperty> propertyCtx,
        RuleFailureStrategy defaultRuleFailureStrategy,
        ComponentFailureStrategy defaultComponentFailureStrategy,
        int declaredOnLine,
        List<string> snapShots)
        : base(defaultRuleFailureStrategy, defaultComponentFailureStrategy, declaredOnLine, snapShots)
    {
        _validatorName = validatorName;
        _propertyCtx = propertyCtx;
    }

    /// <summary>
    /// Builds and returns a fully configured <see cref="ExpressValidatorPropertyRule{T, TExternalResources, TProperty}"/>
    /// based on the components and configurations added via the fluent API.
    /// This method is typically called by the main validator builder when compiling the complete validator core.
    /// </summary>
    /// <returns>A concrete instance of <see cref="ExpressValidatorPropertyRule{T, TExternalResources, TProperty}"/>.</returns>
    public IValidatorRule<T, TExternalResources, ExpressValidatorRunCtx<T, TExternalResources>> Build()
    {
        // Ensure any last pending component is resolved and added to the _ruleComponents list.
        ResolveLastComponent();

        // Create and return the final ExpressValidatorPropertyRule, encapsulating all its components and settings.
        return new ExpressValidatorPropertyRule<T, TExternalResources, TProperty>(
            _propertyCtx,
            _validatorName,
            _onRuleFailure,
            _ruleDeclaredOnLine,
            _ruleComponents // Pass the collected list of PropertyRuleComponent instances
        );
    }
}
