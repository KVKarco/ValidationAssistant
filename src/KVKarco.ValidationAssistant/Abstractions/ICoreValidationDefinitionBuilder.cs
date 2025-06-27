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
public interface ICoreValidationDefinitionBuilder<T, TExternalResources> : IValidationDefinitionBuilder<T, TExternalResources>
{
    // The UseFor method is now inherited from IValidatorDefinitionBuilder.

    // TODO: Add any ExpressValidator-specific methods or overloads for defining validation rules,
    // if they cannot be generically placed on IValidatorDefinitionBuilder.
}
