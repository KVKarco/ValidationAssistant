using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant.ValidationRules;

/// <summary>
/// Represents the contract for an asynchronous custom validation rule.
/// <para>
/// Implementations of this interface define a specific asynchronous validation check for a property.
/// </para>
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of the object containing external resources or dependencies available to the rule.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated by this rule.</typeparam>
public interface IAsyncValidationRule<T, TExternalResources, in TProperty>
{
    /// <summary>
    /// Gets the unique name of the asynchronous validation rule. This can be used for identification or logging.
    /// </summary>
    ReadOnlySpan<char> RuleName { get; }

    /// <summary>
    /// Provides a default failure message for this asynchronous validation rule when it fails.
    /// This message can be dynamic, using the provided validation context and property value.
    /// </summary>
    /// <param name="context">The message context, providing access to the validated object, external resources, and other validation details.</param>
    /// <param name="value">The actual value of the property that was being validated.</param>
    /// <returns>A string representing the default failure message for this rule.</returns>
    string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value);

    /// <summary>
    /// When overridden in a derived class, defines the asynchronous validation logic for the rule.
    /// </summary>
    /// <param name="context">The validation context, providing access to the object being validated and external dependencies.</param>
    /// <param name="value">The value of the property to validate.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, containing <c>true</c> if the property value is valid; otherwise, <c>false</c>.</returns>
    Task<bool> IsValidAsync(ValidatorRunCtx<T, TExternalResources> context, TProperty value, CancellationToken ct);
}
