using KVKarco.ValidationAssistant.Exceptions;

namespace KVKarco.ValidationAssistant;

public sealed class ValidatorRunResult
{
    private readonly string _validatorName;
    private List<ValidatorComponentLog> _executionLogs;

    internal ValidatorRunResult(string producedFromValidator)
    {
        _validatorName = producedFromValidator;
        _executionLogs = [];
    }

    public bool IsFailed { get; internal set; }

    public ValidatorLog GetValidationLog()
    {
        return new ValidatorLog(_validatorName, !IsFailed, _executionLogs);
    }

    public IReadOnlyDictionary<string, IReadOnlyList<ValidationRuleFailure>> GetStructuredFailures()
    {
        if (!IsFailed)
        {
            throw new ValidationRunException("Can't get failures from a successful validation run.");
        }

        var result = new Dictionary<string, IReadOnlyList<ValidationRuleFailure>>();

        for (int i = 0; i < _executionLogs.Count; i++)
        {
            var log = _executionLogs[i];
            if (log.Status == ExecutionStatus.Failed)
            {
                if (result.TryGetValue(log.Path, out var list))
                {
                    var castList = (List<ValidationRuleFailure>)list;

                    foreach (var failure in log.GetFailures())
                    {
                        castList.Add(failure);
                    }
                }
                else
                {
                    List<ValidationRuleFailure> failures = [];
                    foreach (var failure in log.GetFailures())
                    {
                        failures.Add(failure);
                    }

                    if (failures.Count > 0)
                    {
                        result[log.Path] = failures;
                    }
                }
            }
        }

        return result.AsReadOnly();
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> GetStructuredFailureMessages()
    {
        if (!IsFailed)
        {
            throw new ValidationRunException("Can't get failures from a successful validation run.");
        }

        var result = new Dictionary<string, IReadOnlyList<string>>();

        for (int i = 0; i < _executionLogs.Count; i++)
        {
            var log = _executionLogs[i];
            if (log.Status == ExecutionStatus.Failed)
            {
                if (result.TryGetValue(log.Path, out var list))
                {
                    var castList = (List<string>)list;

                    foreach (var message in log.GetFailureMessages())
                    {
                        castList.Add(message);
                    }
                }
                else
                {
                    List<string> messages = [];
                    foreach (var message in log.GetFailureMessages())
                    {
                        messages.Add(message);
                    }

                    if (messages.Count > 0)
                    {
                        result[log.Path] = messages;
                    }
                }
            }
        }

        return result.AsReadOnly();
    }

    internal void AddComponentLog(ValidatorComponentLog log)
    {
        _executionLogs.Add(log);
    }
}
