using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.FailureAssets;

namespace KVKarco.ValidationAssistant;

/// <summary>
/// Represents result of current Validator run.
/// </summary>
public sealed class ValidatorRunResult
{
    private List<RuleFailure>? _failures;

    internal ValidatorRunResult(string producedFromValidator)
    {
        ProducedFromValidator = producedFromValidator;
    }

    /// <summary>
    /// Gets the name of the validator that produced this validation result.
    /// </summary>
    public string ProducedFromValidator { get; }

    /// <summary>
    /// Gets a value indicating whether the validation instance of T is valid. 
    /// </summary>
    public bool IsValid => (!HasPreValidationFailure && (_failures is null || !_failures.Any(x => x.HasValidationFailures)));

    internal bool HasPreValidationFailure { get; set; }

    internal string? PreValidationFailure { get; set; }

    /// <summary>
    /// Retrieves a dictionary of ValidationFailures grouped by property path.
    /// </summary>
    /// <returns>
    /// A dictionary where the keys are property path and the values are collections of ValidationFailures.
    /// </returns>
    /// <exception cref="ValidationRunException">
    /// Thrown if the validation result is successful.
    /// </exception>
    public IReadOnlyDictionary<string, IReadOnlyCollection<ValidationFailure>> GetFailures()
    {
        if (_failures is null)
        {
            throw new ValidationRunException("Cant get failures form successful validation.");
        }

        if (HasPreValidationFailure)
        {
            return new Dictionary<string, IReadOnlyCollection<ValidationFailure>>()
            {
                {"", [ValidationFailure.New(null!, FailureSeverity.Error, PreValidationFailure!)]}
            };
        }

        return _failures.Where(x => x.HasValidationFailures).ToDictionary(static x => x.Path!, static x => x.ValidationFailures);
    }

    /// <summary>
    /// Retrieves a dictionary of error messages grouped by property path.
    /// </summary>
    /// <returns>
    /// A dictionary where the keys are property path and the values are collections of error messages.
    /// </returns>
    /// <exception cref="ValidationRunException">
    /// Thrown if the validation result is successful.
    /// </exception>
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetErrorMessages()
    {
        if (_failures is null)
        {
            throw new ValidationRunException("Cant get failures form successful validation.");
        }

        if (HasPreValidationFailure)
        {
            return new Dictionary<string, IReadOnlyCollection<string>>
            {
                { "", [PreValidationFailure!] }
            };
        }

        return _failures.Where(x => x.HasValidationFailures).ToDictionary(static x => x.Path!, static x => x.ValidationFailuresMessages);
    }

    internal void AddRuleFailure(RuleFailure failure)
    {
        _failures ??= [];
        _failures.Add(failure);
    }
}
