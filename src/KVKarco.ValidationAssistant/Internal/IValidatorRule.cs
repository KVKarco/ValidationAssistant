using KVKarco.ValidationAssistant.Internal.FailureAssets;

namespace KVKarco.ValidationAssistant.Internal;

/// <summary>
/// Defines the contract for an internal validator rule, specifying the core methods
/// and properties required for any rule to be executed within the validation framework.
/// This interface ensures a common API for both property-specific and other types of rules.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
/// <typeparam name="TContext">The specific type of <see cref="ValidatorRunCtx{T, TExternalResources}"/>
/// used for the validation run, ensuring context-specific operations.</typeparam>
internal interface IValidatorRule<T, TExternalResources, TContext>
    where TContext : ValidatorRunCtx<T, TExternalResources>
{
    /// <summary>
    /// Gets the failure information associated with this rule.
    /// This property provides access to details like explanation factories and failure strategies
    /// that are applicable if this rule determines a failure.
    /// </summary>
    RuleFailureInfo Info { get; }

    /// <summary>
    /// Gets the unique or descriptive name of this validator rule.
    /// This name can be used for logging, identification, or debugging purposes.
    /// </summary>
    ReadOnlySpan<char> RuleName { get; }

    /// <summary>
    /// Gets a value indicating whether this validator rule can be executed synchronously.
    /// If <see langword="false"/>, it implies that the rule relies on asynchronous operations
    /// and should only be invoked via <see cref="ValidateAsync(TContext, CancellationToken)"/>.
    /// </summary>
    bool CanRunSynchronously { get; }

    /// <summary>
    /// Synchronously executes this validator rule.
    /// Implementations should validate their internal state or conditions using the provided <paramref name="context"/>.
    /// If a failure occurs, the necessary information should be added to the <paramref name="context"/>.
    /// This method should only be called if <see cref="CanRunSynchronously"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="context">The validation run context for the current operation.</param>
    void Validate(TContext context);

    /// <summary>
    /// Asynchronously executes this validator rule.
    /// Implementations should validate their internal state or conditions using the provided <paramref name="context"/>.
    /// If a failure occurs, the necessary information should be added to the <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The validation run context for the current operation.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask ValidateAsync(TContext context, CancellationToken ct);
}
