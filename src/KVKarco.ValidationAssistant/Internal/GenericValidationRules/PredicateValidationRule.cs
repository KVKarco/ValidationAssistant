using KVKarco.ValidationAssistant.ValidationRules;

namespace KVKarco.ValidationAssistant.Internal.GenericValidationRules;

/// <summary>
/// Represents a synchronous validation rule that uses a simple predicate function
/// to determine the validity of a property. This rule directly applies the provided
/// <see cref="PropertyPredicate{TProperty}"/> to the property's value.
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources or dependencies available to the rule.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated by this rule.</typeparam>
internal sealed class PredicateValidationRule<T, TExternalResources, TProperty> :
    IValidationRule<T, TExternalResources, TProperty>
{
    /// <summary>
    /// The predicate function used to evaluate the validity of the property.
    /// </summary>
    private readonly PropertyPredicate<TProperty> _predicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateValidationRule{T, TExternalResources, TProperty}"/> class.
    /// </summary>
    /// <param name="predicate">The synchronous predicate function that defines the validation logic for the property.</param>
    public PredicateValidationRule(PropertyPredicate<TProperty> predicate)
        => _predicate = predicate;

    /// <summary>
    /// Gets the unique name of this validation rule. This is typically a predefined constant
    /// from <see cref="DefaultNaming"/>.
    /// </summary>
    public ReadOnlySpan<char> RuleName => DefaultNaming.PredicateValRule;

    /// <summary>
    /// Provides the default failure message for this validation rule when the predicate evaluates to <see langword="false"/>.
    /// It retrieves the message from the global default messages configuration.
    /// </summary>
    /// <param name="context">The message context, providing access to the validated object, external resources, and culture.</param>
    /// <param name="value">The actual value of the property that was being validated.</param>
    /// <returns>A string representing the default failure message for this rule, typically
    /// "The specified condition was not met for [PropertyName]".</returns>
    public string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value)
        => ValidatorsConfig.GlobalDefaults.Messages.ValidationComponentError(context, value);

    /// <summary>
    /// Determines whether the property value is valid by executing the encapsulated predicate.
    /// </summary>
    /// <param name="context">The validation run context, providing access to the main object and external dependencies.</param>
    /// <param name="value">The value of the property to validate.</param>
    /// <returns><see langword="true"/> if the predicate returns <see langword="true"/> (indicating validity); otherwise, <see langword="false"/>.</returns>
    public bool IsValid(ValidatorRunCtx<T, TExternalResources> context, TProperty value)
        => _predicate(value);
}