using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// Defines the contract for a validation rule that can be executed against a specific context.
/// This interface is a core component in the validation process, supporting both synchronous
/// and asynchronous validation flows for all types of validator rules.
/// </summary>
/// <typeparam name="T">The type of the entity or object being validated by this rule.</typeparam>
/// <typeparam name="TExternalResources">
/// The type representing any external resources required during the validation process (e.g., database contexts, API clients).
/// </typeparam>
/// <typeparam name="TContext">
/// The type of the validation run context, which encapsulates the entity being validated,
/// external resources, and other validation-specific state. This context must derive from
/// <see cref="ValidatorRunCtx{T, TExternalResources}"/>.
/// </typeparam>
internal interface IValidatorRule<T, TExternalResources, TContext>
    where TContext : ValidatorRunCtx<T, TExternalResources>
{
    /// <summary>
    /// Gets information about the rule's potential failure, such as its error code or default message.
    /// This metadata is used to provide consistent failure details if the rule's validation logic fails.
    /// </summary>
    RuleFailureInfo Info { get; }

    /// <summary>
    /// Gets the name of the validation rule.
    /// This name is typically used for identification, logging, or reporting purposes but is not guaranteed to be unique.
    /// Using <see cref="ReadOnlySpan{T}"/> for efficient string-like rule name handling.
    /// </summary>
    ReadOnlySpan<char> RuleName { get; }

    /// <summary>
    /// Gets a value indicating whether this validation rule can be executed synchronously.
    /// If <c>true</c>, the <see cref="Validate(TContext)"/> method can be safely called.
    /// If <c>false</c>, only <see cref="ValidateAsync(TContext, CancellationToken)"/> should be used,
    /// indicating that the rule might perform asynchronous operations (e.g., I/O).
    /// </summary>
    bool CanRunSynchronously { get; }

    /// <summary>
    /// Executes the validation logic synchronously against the provided context.
    /// This method should only be invoked if <see cref="CanRunSynchronously"/> is <c>true</c>.
    /// If <see cref="CanRunSynchronously"/> is <c>false</c>, calling this method will
    /// result in the validation process ending with a single failure and an explanation.
    /// </summary>
    /// <param name="context">The validation run context containing the entity and external resources.</param>
    void Validate(TContext context);

    /// <summary>
    /// Executes the validation logic asynchronously against the provided context.
    /// This method is designed for rules that may involve I/O or other long-running, non-blocking tasks.
    /// </summary>
    /// <param name="context">The validation run context containing the entity and external resources.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous validation operation.</returns>
    ValueTask ValidateAsync(TContext context, CancellationToken ct);
}