using KVKarco.ValidationAssistant.Abstractions;

namespace KVKarco.ValidationAssistant.ValidationRules;

/// <summary>
/// Defines the synchronous contract for a validation rule that operates on a specific property.
/// This interface is intended as an internal contract for the library's custom validation rules.
/// Consumers of the library should typically implement the
/// <see cref="CustomValidationRule{T, TExternalResources, TProperty}"/> abstract class instead of this interface directly.
/// </summary>
/// <typeparam name="T">The type of the root entity being validated.</typeparam>
/// <typeparam name="TExternalResources">The type representing any external resources required during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated. The 'in' keyword indicates that this type parameter is contravariant.</typeparam>
public interface IValidationRule<T, TExternalResources, in TProperty>
{
    /// <summary>
    /// Gets the name of the validation rule. This name is used for identification and reporting, but is not guaranteed to be unique.
    /// </summary>
    ReadOnlySpan<char> RuleName { get; }

    /// <summary>
    /// Provides the default failure message for this validation rule if it fails.
    /// </summary>
    /// <param name="context">The message context providing access to validation instance and external resources.</param>
    /// <param name="value">The value of the property being validated.</param>
    /// <returns>A string representing the default failure message.</returns>
    string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value);

    /// <summary>
    /// Determines whether the property value is valid synchronously according to this rule's logic.
    /// </summary>
    /// <param name="context">The validation context providing access to the validation instance and external resources.</param>
    /// <param name="value">The value of the property being validated.</param>
    /// <returns><c>true</c> if the property value is valid; otherwise, <c>false</c>.</returns>
    bool IsValid(IValidationCtx<T, TExternalResources> context, TProperty value);
}