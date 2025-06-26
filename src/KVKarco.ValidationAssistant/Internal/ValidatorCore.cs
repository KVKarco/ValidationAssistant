using KVKarco.ValidationAssistant.Internal.PreValidation;
using System.Collections.Immutable;

namespace KVKarco.ValidationAssistant.Internal;

internal abstract class ValidatorCore
{
    protected ValidatorCore(string validatorName, bool canRunSynchronously, List<string>? snapShots)
    {
        ValidatorName = validatorName;
        CanRunSynchronously = canRunSynchronously;
        SnapShots = snapShots is null ? null : [.. snapShots];
    }

    /// <summary>
    /// Gets the descriptive name of this validator core.
    /// </summary>
    public string ValidatorName { get; }

    /// <summary>
    /// Gets a value indicating whether this validator core can be executed synchronously.
    /// This is <see langword="true"/> if and only if all contained pre-validation rules and main rules are synchronous.
    /// </summary>
    public bool CanRunSynchronously { get; }

    /// <summary>
    /// Gets an optional immutable array of string snapshots, which can be used for debugging,
    /// rule definition visualization, or other meta-information purposes.
    /// </summary>
    public ImmutableArray<string>? SnapShots { get; }
}

/// <summary>
/// Represents the abstract base class for a compiled validator core.
/// This class holds immutable sets of <see cref="IPreValidationRule{T, TExternalResources}"/> instances
/// for pre-validation logic and <see cref="IValidatorRule{T, TExternalResources, TContex}"/> instances
/// for the main validation logic for a specific validator type.
/// It provides the internal mechanisms for executing these rules synchronously and asynchronously in distinct phases.
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated by this core.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources that can be accessed by the validation rules.</typeparam>
/// <typeparam name="TContex">The specific type of <see cref="ValidatorRunCtx{T, TExternalResources}"/>
/// used for the validation run, ensuring context-specific operations and access to resources.</typeparam>
internal abstract class ValidatorCore<T, TExternalResources, TContex> :
    ValidatorCore
    where TContex : ValidatorRunCtx<T, TExternalResources>
{
    /// <summary>
    /// An immutable array containing the pre-validation <see cref="IPreValidationRule{T, TExternalResources}"/> instances.
    /// These rules are executed first to ensure fundamental conditions are met before main validation.
    /// </summary>
    private readonly ImmutableArray<IPreValidationRule<T, TExternalResources>> _preValidationRules;

    /// <summary>
    /// An immutable array containing all the compiled main <see cref="IValidatorRule{T, TExternalResources, TContex}"/> instances
    /// that constitute the primary validation logic for this core.
    /// </summary>
    private readonly ImmutableArray<IValidatorRule<T, TExternalResources, TContex>> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorCore{T, TExternalResources, TContex}"/> class.
    /// </summary>
    /// <param name="validatorName">The descriptive name of the validator core.</param>
    /// <param name="snapShots">An optional list of string snapshots, potentially used for debugging or capturing rule definitions state.</param>
    /// <param name="preValidationRules">A list of <see cref="IPreValidationRule{T, TExternalResources}"/> instances
    /// representing pre-validation checks. This list is converted to an immutable array internally.</param>
    /// <param name="rules">A list of compiled main <see cref="IValidatorRule{T, TExternalResources, TContex}"/> instances
    /// that this core will execute. This list is converted to an immutable array internally.</param>
    protected ValidatorCore(
        string validatorName,
        List<string>? snapShots,
        List<IPreValidationRule<T, TExternalResources>> preValidationRules,
        List<IValidatorRule<T, TExternalResources, TContex>> rules)
        : base(validatorName, !preValidationRules.Exists(x => !x.CanRunSynchronously) && !rules.Exists(x => !x.CanRunSynchronously), snapShots)
    {
        _preValidationRules = [.. preValidationRules];
        _rules = [.. rules]; // Convert list to immutable array for thread-safety and performance.
    }

    /// <summary>
    /// Synchronously executes all compiled pre-validation rules, and if they all pass
    /// (or do not cause the validation to stop), then proceeds to execute the main validation rules.
    /// The execution flow can be controlled by the <paramref name="context"/>, allowing for
    /// short-circuiting or jumping between rules.
    /// </summary>
    /// <param name="context">The validation run context, containing the instance to validate,
    /// external resources, and state for tracking validation progress and failures.</param>
    public void InternalValidate(TContex context)
    {
        // --- Execute Pre-Validation Rules ---
        int preIndex = 0;
        while (preIndex < _preValidationRules.Length)
        {
            _preValidationRules[preIndex].PreValidate(context);

            // If a pre-validation rule causes the validation to stop, immediately return to stop the entire process.
            if (context.ToStopValidation())
            {
                return;
            }
            preIndex++;
        }

        // Reset index for main rules execution
        int mainIndex = 0;

        // --- Execute Main Validation Rules (only if pre-validation did not stop the process) ---
        while (mainIndex < _rules.Length)
        {
            // Execute the current main rule's synchronous validation logic.
            _rules[mainIndex].Validate(context);

            // Check if the validation context indicates that the entire validation process should stop.
            if (context.ToStopValidation())
            {
                break; // Exit the loop and stop further validation.
            }

            // Calculate the next index based on the context's logic (e.g., for conditional jumps).
            mainIndex = context.CalculateNextIndex(mainIndex);
        }
    }

    /// <summary>
    /// Asynchronously executes all compiled pre-validation rules, and if they all pass
    /// (or do not cause the validation to stop), then proceeds to execute the main validation rules.
    /// The execution flow can be controlled by the <paramref name="context"/>, allowing for
    /// short-circuiting or jumping between rules. Supports cancellation.
    /// </summary>
    /// <param name="context">The validation run context, containing the instance to validate,
    /// external resources, and state for tracking validation progress and failures.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous validation operation.</returns>
    public async ValueTask InternalValidateAsync(TContex context, CancellationToken ct)
    {
        // --- Execute Pre-Validation Rules ---
        int preIndex = 0;
        while (preIndex < _preValidationRules.Length)
        {
            ct.ThrowIfCancellationRequested(); // Check for cancellation before executing each pre-validation rule.

            await _preValidationRules[preIndex].PreValidateAsync(context, ct).ConfigureAwait(false);

            // If a pre-validation rule causes the validation to stop, immediately return to stop the entire process.
            if (context.ToStopValidation())
            {
                return;
            }
            preIndex++;
        }

        // Reset index for main rules execution
        int mainIndex = 0;

        // --- Execute Main Validation Rules (only if pre-validation did not stop the process) ---
        while (mainIndex < _rules.Length)
        {
            ct.ThrowIfCancellationRequested(); // Check for cancellation before executing each main rule.

            // Execute the current main rule's asynchronous validation logic.
            await _rules[mainIndex].ValidateAsync(context, ct).ConfigureAwait(false);

            // Check if a main rule causes the validation to stop.
            if (context.ToStopValidation())
            {
                break; // Exit the loop and stop further validation.
            }

            // Calculate the next index based on the context's logic (e.g., for conditional jumps).
            mainIndex = context.CalculateNextIndex(mainIndex);
        }
    }
}
