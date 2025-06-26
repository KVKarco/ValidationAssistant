using System.Collections.Immutable;

namespace KVKarco.ValidationAssistant.Internal;

/// <summary>
/// Represents the abstract base class for a compiled validator core.
/// This class holds the immutable set of <see cref="IValidatorRule{T, TExternalResources, TContex}"/> instances
/// that define the validation logic for a specific validator type.
/// It provides the internal mechanisms for executing these rules synchronously and asynchronously.
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated by this core.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources that can be accessed by the validation rules.</typeparam>
/// <typeparam name="TContex">The specific type of <see cref="ValidatorRunCtx{T, TExternalResources}"/>
/// used for the validation run, ensuring context-specific operations and access to resources.</typeparam>
internal abstract class ValidatorCore<T, TExternalResources, TContex>
    where TContex : ValidatorRunCtx<T, TExternalResources>
{
    /// <summary>
    /// An immutable array containing all the compiled <see cref="IValidatorRule{T, TExternalResources, TContex}"/> instances
    /// that constitute the validation logic for this core.
    /// </summary>
    private readonly ImmutableArray<IValidatorRule<T, TExternalResources, TContex>> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorCore{T, TExternalResources, TContex}"/> class.
    /// </summary>
    /// <param name="validatorName">The descriptive name of the validator core.</param>
    /// <param name="snapShots">An optional list of string snapshots, potentially used for debugging or capturing rule definitions state.</param>
    /// <param name="rules">A list of compiled <see cref="IValidatorRule{T, TExternalResources, TContex}"/> instances
    /// that this core will execute. This list is converted to an immutable array internally.</param>
    protected ValidatorCore(
        string validatorName,
        List<string>? snapShots,
        List<IValidatorRule<T, TExternalResources, TContex>> rules)
    {
        _rules = [.. rules]; // Convert list to immutable array for thread-safety and performance.
        ValidatorName = validatorName;
        // Determines if this core can run synchronously by checking if any of its contained rules require asynchronous execution.
        CanRunSynchronously = !rules.Exists(x => !x.CanRunSynchronously);
        SnapShots = snapShots is null ? null : [.. snapShots]; // Convert list to immutable array if provided.
    }

    /// <summary>
    /// Gets the descriptive name of this validator core.
    /// </summary>
    public string ValidatorName { get; }

    /// <summary>
    /// Gets a value indicating whether this validator core can be executed synchronously.
    /// This is <see langword="true"/> if and only if all contained rules are synchronous.
    /// </summary>
    public bool CanRunSynchronously { get; }

    /// <summary>
    /// Gets an optional immutable array of string snapshots, which can be used for debugging,
    /// rule definition visualization, or other meta-information purposes.
    /// </summary>
    public ImmutableArray<string>? SnapShots { get; }

    /// <summary>
    /// Synchronously executes all compiled validation rules within this core.
    /// The execution flow can be controlled by the <paramref name="context"/>, allowing for
    /// short-circuiting or jumping between rules.
    /// </summary>
    /// <param name="context">The validation run context, containing the instance to validate,
    /// external resources, and state for tracking validation progress and failures.</param>
    public void InternalValidate(TContex context)
    {
        int index = 0;

        // Iterate through each rule in the immutable array.
        while (index < _rules.Length)
        {
            // Execute the current rule's synchronous validation logic.
            _rules[index].Validate(context);

            // Check if the validation context indicates that the entire validation process should stop.
            if (context.ToStopValidation())
            {
                break; // Exit the loop and stop further validation.
            }

            // Calculate the next index based on the context's logic (e.g., for conditional jumps).
            index = context.CalculateNextIndex(index);
        }
    }

    /// <summary>
    /// Asynchronously executes all compiled validation rules within this core.
    /// The execution flow can be controlled by the <paramref name="context"/>, allowing for
    /// short-circuiting or jumping between rules. Supports cancellation.
    /// </summary>
    /// <param name="context">The validation run context, containing the instance to validate,
    /// external resources, and state for tracking validation progress and failures.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous validation operation.</returns>
    public async ValueTask InternalValidateAsync(TContex context, CancellationToken ct)
    {
        int index = 0;

        // Iterate through each rule in the immutable array.
        while (index < _rules.Length)
        {
            // Throws an OperationCanceledException if cancellation is requested.
            ct.ThrowIfCancellationRequested();

            // Execute the current rule's asynchronous validation logic.
            await _rules[index].ValidateAsync(context, ct).ConfigureAwait(false);

            // Check if the validation context indicates that the entire validation process should stop.
            if (context.ToStopValidation())
            {
                break; // Exit the loop and stop further validation.
            }

            // Calculate the next index based on the context's logic (e.g., for conditional jumps).
            index = context.CalculateNextIndex(index);
        }
    }
}
