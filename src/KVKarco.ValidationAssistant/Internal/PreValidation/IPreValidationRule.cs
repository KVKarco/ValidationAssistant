namespace KVKarco.ValidationAssistant.Internal.PreValidation;

/// <summary>
/// Defines the contract for an internal pre-validation rule.
/// These rules are designed to perform fundamental checks on the main instance or external resources
/// before the main validation logic is executed. They are distinct from standard validation rules
/// by having a dedicated execution phase at the beginning of the validation process.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
internal interface IPreValidationRule<T, TExternalResources>
{
    /// <summary>
    /// Gets a value indicating whether this pre-validation rule can be executed synchronously.
    /// If <see langword="false"/>, it implies that the rule relies on asynchronous operations
    /// and should only be invoked via <see cref="PreValidateAsync(ValidatorRunCtx{T, TExternalResources}, CancellationToken)"/>.
    /// </summary>
    bool CanRunSynchronously { get; }

    /// <summary>
    /// Gets the unique or descriptive name of this pre-validation rule.
    /// This name can be used for logging, identification, or debugging purposes specific to pre-conditions.
    /// </summary>
    ReadOnlySpan<char> RuleName { get; }

    /// <summary>
    /// Gets an user predefined explanation message for this pre-validation rule failure or use default message.
    /// </summary>
    string FailureExplanation { get; }

    /// <summary>
    /// Retrieves the default error message for this pre-validation rule when it fails.
    /// This method allows for dynamic message generation based on the validation context.
    /// </summary>
    /// <param name="context">The validation run context, providing access to the instance, external resources, and other relevant information.</param>
    /// <returns>A string representing the default error message.</returns>
    string GetDefaultErrorMessage(ValidatorRunCtx<T, TExternalResources> context);

    /// <summary>
    /// Synchronously executes the pre-validation logic.
    /// Implementations should perform their checks and, if a failure occurs,
    /// add the necessary information to the provided <paramref name="context"/>.
    /// This method should only be called if <see cref="CanRunSynchronously"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="context">The validation run context for the current operation.</param>
    void PreValidate(ValidatorRunCtx<T, TExternalResources> context);

    /// <summary>
    /// Asynchronously executes the pre-validation logic.
    /// Implementations should perform their asynchronous checks and, if a failure occurs,
    /// add the necessary information to the provided <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The validation run context for the current operation.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask PreValidateAsync(ValidatorRunCtx<T, TExternalResources> context, CancellationToken ct);
}

