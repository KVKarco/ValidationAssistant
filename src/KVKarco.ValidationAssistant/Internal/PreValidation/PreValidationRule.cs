namespace KVKarco.ValidationAssistant.Internal.PreValidation;

/// <summary>
/// Provides an abstract base class for defining pre-validation rules.
/// Derived classes will implement the specific logic for checking fundamental conditions
/// on the main instance or its external resources before the main validation process begins.
/// This class handles common properties like rule synchronicity and a potential default failure message.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
internal abstract class PreValidationRule<T, TExternalResources> :
    IPreValidationRule<T, TExternalResources>
{
    protected readonly PreValidationRuleFailureInfo<T, TExternalResources> _info;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreValidationRule{T, TExternalResources}"/> class.
    /// </summary>
    /// <param name="canRunSynchronously">A boolean indicating whether this rule can be executed synchronously.</param>
    /// <param name="info">The failure information associated with this pre-validation rule.</param>
    /// <param name="failureExplanation">An optional predefined failure message to use if this rule fails.</param>
    protected PreValidationRule(
        PreValidationRuleFailureInfo<T, TExternalResources> info,
        bool canRunSynchronously,
        string failureExplanation)
    {
        _info = info;
        CanRunSynchronously = canRunSynchronously;
        FailureExplanation = failureExplanation;
    }

    /// <summary>
    /// Gets a value indicating whether this pre-validation rule can be executed synchronously.
    /// </summary>
    public bool CanRunSynchronously { get; }

    /// <summary>
    /// When overridden in a derived class, gets the unique or descriptive name of this pre-validation rule.
    /// </summary>
    public abstract ReadOnlySpan<char> RuleName { get; }

    /// <summary>
    /// Gets an user predefined explanation message for this pre-validation rule failure or use default message.
    /// </summary>
    public string FailureExplanation { get; }

    /// <summary>
    /// When overridden in a derived class, retrieves the default error message for this
    /// pre-validation rule when it fails, based on the provided context.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <returns>A string representing the default error message.</returns>
    public abstract string GetDefaultErrorMessage(ValidatorRunCtx<T, TExternalResources> context);

    /// <summary>
    /// When overridden in a derived class, synchronously executes the pre-validation logic.
    /// Implementations should add failures to the context if conditions are not met.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    public abstract void PreValidate(ValidatorRunCtx<T, TExternalResources> context);

    /// <summary>
    /// When overridden in a derived class, asynchronously executes the pre-validation logic.
    /// Implementations should add failures to the context if conditions are not met.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public abstract ValueTask PreValidateAsync(ValidatorRunCtx<T, TExternalResources> context, CancellationToken ct);
}
