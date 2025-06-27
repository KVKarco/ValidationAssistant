using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// Defines a foundational, generic contract for building and configuring *pre-validation* logic
/// for any type of validator within the ValidationAssistant framework. This interface provides
/// methods to define overall conditions, global checks, or setup steps that occur before the
/// main validation rules are processed. It acts as the primary fluent API for users to set
/// up initial checks on the main instance or its external resources.
/// </summary>
/// <typeparam name="T">The type of the instance being validated by this builder.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies that
/// might be required for pre-validation construction or evaluation.</typeparam>
public interface IPreValidationDefinitionBuilder<T, TExternalResources>
{
    /// <summary>
    /// Adds a synchronous pre-validation check that applies a predicate to the main instance being validated.
    /// This check is executed before any main validation rules. If the predicate returns <see langword="false"/>,
    /// the pre-validation fails.
    /// </summary>
    /// <param name="predicate">A synchronous function that takes the main instance and returns <see langword="true"/> if valid, <see langword="false"/> otherwise. Must not be <see langword="null"/>.</param>
    /// <param name="explanationMessage">An optional custom explanation message for the failure. If <see langword="null"/>, a default message is used.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called. Used for debugging/reporting.</param>
    /// <exception cref="RuleCreationException">Thrown if <paramref name="predicate"/> is <see langword="null"/>.</exception>
    void Ensure(
        PreValidationPredicate<T> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Adds an asynchronous pre-validation check that applies a predicate to the main instance being validated.
    /// This check is executed before any main validation rules. If the predicate returns <see langword="false"/>,
    /// the pre-validation fails.
    /// </summary>
    /// <param name="predicate">An asynchronous function that takes the main instance and returns a <see cref="ValueTask{Boolean}"/> indicating validity. Must not be <see langword="null"/>.</param>
    /// <param name="explanationMessage">An optional custom explanation message for the failure. If <see langword="null"/>, a default message is used.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called. Used for debugging/reporting.</param>
    /// <exception cref="RuleCreationException">Thrown if <paramref name="predicate"/> is <see langword="null"/>.</exception>
    void EnsureAsync(
        AsyncPreValidationPredicate<T> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Adds a synchronous pre-validation check that applies a predicate to the external resources (<typeparamref name="TExternalResources"/>) available during validation.
    /// This check is executed before any main validation rules and is typically used for validating dependencies or global state.
    /// If the predicate returns <see langword="false"/>, the pre-validation fails.
    /// </summary>
    /// <param name="predicate">A synchronous function that takes the external resources and returns <see langword="true"/> if valid, <see langword="false"/> otherwise. Must not be <see langword="null"/>.</param>
    /// <param name="explanationMessage">An optional custom explanation message for the failure. If <see langword="null"/>, a default message is used.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called. Used for debugging/reporting.</param>
    /// <exception cref="RuleCreationException">Thrown if <paramref name="predicate"/> is <see langword="null"/>.</exception>
    void EnsureResources(
        PreValidationPredicate<TExternalResources> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Adds an asynchronous pre-validation check that applies a predicate to the external resources (<typeparamref name="TExternalResources"/>) available during validation.
    /// This check is executed before any main validation rules. If the predicate returns <see langword="false"/>,
    /// the pre-validation fails.
    /// </summary>
    /// <param name="predicate">An asynchronous function that takes the external resources and returns a <see cref="ValueTask{Boolean}"/> indicating validity. Must not be <see langword="null"/>.</param>
    /// <param name="explanationMessage">An optional custom explanation message for the failure. If <see langword="null"/>, a default message is used.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called. Used for debugging/reporting.</param>
    /// <exception cref="RuleCreationException">Thrown if <paramref name="predicate"/> is <see langword="null"/>.</exception>
    void EnsureResourcesAsync(
        AsyncPreValidationPredicate<TExternalResources> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);
}

