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
    public bool IsValid => (!HasPreValidationFailure && (_failures is null || !_failures.Any(x => x.HasValidationFailures)));

    /// <summary>
    /// Gets or sets a value indicating whether a pre-validation failure has occurred.
    /// </summary>
    internal bool HasPreValidationFailure { get; set; }

    /// <summary>
    /// Gets or sets the message associated with a pre-validation failure, if one occurred.
    /// This is typically the message from a <see cref="PreValidationException"/> (if exceptions were thrown)
    /// or a direct message indicating a foundational issue.
    /// </summary>
    internal string? PreValidationFailure { get; set; }

    //TODO: create PreValidationFailureInfo

    /// <summary>
    /// Retrieves a dictionary of <c>ValidationFailure</c> instances, grouped by their associated property path.
    /// If a pre-validation failure occurred, it is represented under an empty string key ("").
    /// </summary>
    /// <returns>
    /// A dictionary where the keys are property paths (or an empty string for pre-validation failures)
    /// and the values are collections of <c>ValidationFailure</c> objects.
    /// </returns>
    /// <exception cref="ValidationRunException">
    /// Thrown if this method is called when the <see cref="ValidatorRunResult"/> indicates a successful validation (<see cref="IsValid"/> is <see langword="true"/>).
    /// </exception>
    public IReadOnlyDictionary<string, IReadOnlyCollection<ValidationFailure>> GetFailures()
    {
        if (IsValid) // Changed from _failures is null to IsValid check based on typical usage
        {
            throw new ValidationRunException("Can't get failures from a successful validation run.");
        }

        if (HasPreValidationFailure)
        {
            // If there's a pre-validation failure, return it as a failure for the entire instance (empty path).
            // Assuming ValidationFailure.New can create an entry with null RuleFailureInfo, Error severity, and a message.
            return new Dictionary<string, IReadOnlyCollection<ValidationFailure>>()
            {
                // Note: The original code uses null! for the first parameter, implying a constructor or static method
                // that allows a null RuleFailureInfo. Ensure this aligns with your ValidationFailure.New implementation.
                {"", [ValidationFailure.ForPropertyComponent(null!, PreValidationFailure!)]}
            };
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

        if (HasPreValidationFailure)
        {
            // If there's a pre-validation failure, return its message for the entire instance (empty path).
            return new Dictionary<string, IReadOnlyCollection<string>>
            {
                { "", [PreValidationFailure!] }
            };
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
