namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// Defines the fluent API for constructing and composing validation logic specific to the
/// <see cref="ExpressValidator{T, TExternalResources}"/> implementation within the ValidationAssistant framework.
/// This interface extends <see cref="IValidationDefinitionBuilder{T, TExternalResources}"/> and
/// provides methods to set default failure strategies, initiate property-specific rules,
/// or define conditional rule blocks for an ExpressValidator.
/// </summary>
/// <typeparam name="T">The type of the instance being validated by this builder.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies that
/// might be required for rule evaluation or condition checking.</typeparam>
/// <remarks>
/// <para>
/// This interface serves as the core entry point for fluently defining the validation schema
/// within a concrete <see cref="ExpressValidator{T, TExternalResources}"/> implementation.
/// </para>
/// <para>
/// It inherits all foundational rule-definition capabilities from <see cref="IValidationDefinitionBuilder{T, TExternalResources}"/>,
/// such as the <c>UseFor</c> method for initiating property-specific validation chains,
/// and methods for defining conditional rule blocks.
/// </para>
/// <para>
/// While currently empty beyond its inheritance, <c>ICoreValidationDefinitionBuilder</c> is designed
/// to be the dedicated location for future methods that apply configuration or behavior
/// directly at the overall validator level, rather than being tied to individual properties or components.
/// </para>
/// <para>
/// Examples of methods that might be added to this interface include those for setting
/// global default failure strategies for all rules, defining validator-level pre-validation or
/// post-validation hooks, or configuring cross-property validation conditions that affect the entire validation run.
/// </para>
/// </remarks>
public interface ICoreValidationDefinitionBuilder<T, TExternalResources> :
    IValidationDefinitionBuilder<T, TExternalResources>
{
    // The UseFor method is now inherited from IValidationDefinitionBuilder.

    // TODO: Add any ExpressValidator-specific methods or overloads for defining validation rules,
    // if they cannot be generically placed on IValidationDefinitionBuilder.
}