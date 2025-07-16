using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant;

/// <summary>
/// Represents the comprehensive result of a single validator run.
/// This sealed class encapsulates all collected failures, whether from
/// pre-validation checks or specific validation rules, and provides methods
/// to query the validation status and retrieve detailed failure information.
/// </summary>
public sealed class ValidatorRunResult
{
    internal bool IsValidationRunForceStopped { get; private set; }

    /// <summary>
    /// A private list to store individual <see cref="RuleFailure"/> instances encountered during the validation run.
    /// This list is lazily initialized.
    /// </summary>
    private List<RuleFailure>? _failures;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorRunResult"/> class.
    /// </summary>
    /// <param name="producedFromValidator">The name or identifier of the validator that initiated and produced this result.</param>
    internal ValidatorRunResult(string producedFromValidator)
    {
        ProducedFromValidator = producedFromValidator;
        IsValidationRunForceStopped = false;
    }

    /// <summary>
    /// Gets the name or identifier of the validator that produced this validation result.
    /// </summary>
    public string ProducedFromValidator { get; }

    /// <summary>
    /// Gets a value indicating whether the instance being validated is considered valid.
    /// An instance is valid if there are no pre-validation failures and no rule failures
    /// that contain actual validation failures.
    /// </summary>
    public bool IsValid => _failures is null || !_failures.Any(x => x.HasValidationFailures);

    internal void Clear(RuleFailure ruleFailure)
    {
        IsValidationRunForceStopped = true;
        _failures?.Clear(); // Clear the list of failures if it exists
        _failures ??= [];
        _failures.Add(ruleFailure); // Add the provided rule failure to the list
    }
    //TODO: create PreValidationFailureInfo

    public IReadOnlyDictionary<string, IReadOnlyCollection<ValidationFailure>> GetFailures()
    {
        if (IsValid) // Changed from _failures is null to IsValid check based on typical usage
        {
            throw new ValidationRunException("Can't get failures from a successful validation run.");
        }

        // Filter for RuleFailures that actually contain validation failures and convert to a dictionary
        return _failures!.Where(x => x.HasValidationFailures)
                         .ToDictionary(static x => x.Path!, static x => x.ValidationFailures);
    }

    /// <summary>
    /// Retrieves a dictionary of error messages, grouped by property path.
    /// If a pre-validation failure occurred, its message is included under an empty string key ("").
    /// </summary>
    /// <returns>
    /// A dictionary where the keys are property paths (or an empty string for pre-validation failures)
    /// and the values are collections of corresponding error messages.
    /// </returns>
    /// <exception cref="ValidationRunException">
    /// Thrown if this method is called when the <see cref="ValidatorRunResult"/> indicates a successful validation (<see cref="IsValid"/> is <see langword="true"/>).
    /// </exception>
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetErrorMessages()
    {
        if (IsValid) // Changed from _failures is null to IsValid check based on typical usage
        {
            throw new ValidationRunException("Can't get error messages from a successful validation run.");
        }

        // Filter for RuleFailures that contain validation messages and convert to a dictionary of messages
        return _failures!.Where(x => x.HasValidationFailures)
                         .ToDictionary(static x => x.Path!, static x => x.ValidationFailuresMessages);
    }

    /// <summary>
    /// Adds a <see cref="RuleFailure"/> to the collection of failures for this validation result.
    /// The internal list of failures is initialized if it does not already exist.
    /// </summary>
    /// <param name="failure">The <see cref="RuleFailure"/> instance to add.</param>
    internal void AddRuleFailure(RuleFailure failure)
    {
        _failures ??= []; // Initialize list if null
        _failures.Add(failure); // Add the new failure
    }
}
