using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.Utilities;
using System.Text;

namespace KVKarco.ValidationAssistant.Results;

public sealed class ValidatorRunResult
{
    private readonly string _validatorName;
    private List<ValidationComponentExecutionLog> _executionLogs;

    internal ValidatorRunResult(string producedFromValidator)
    {
        _validatorName = producedFromValidator;
        _executionLogs = [];
    }

    public bool IsValid { get; internal set; }

    public override string ToString()
    {

        StringBuilder sb = new();
        sb.AppendLine();
        sb.Append(InternalDefaults.ResultTitle);
        sb.Append(_validatorName);

        if (IsValid)
        {
            sb.Append(InternalDefaults.ResultSuccess);
        }
        else
        {
            sb.Append(InternalDefaults.ResultFailure);
        }
        sb.AppendLine();

        foreach (var log in _executionLogs)
        {
            log.AppendToLog(sb);
        }

        return sb.ToString();
    }

    public IReadOnlyDictionary<string, IReadOnlyList<ValidationRuleFailure>> GetStructuredFailures()
    {
        if (IsValid)
        {
            throw new ValidationAssistantException("Can't get failures from a successful validation run.");
        }

        var result = new Dictionary<string, IReadOnlyList<ValidationRuleFailure>>();

        for (int i = 0; i < _executionLogs.Count; i++)
        {
            var log = _executionLogs[i];
            if (log.Status == ExecutionStatus.Failed)
            {
                if (result.TryGetValue(log.KeyPath, out var list))
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
                        result[log.KeyPath] = failures;
                    }
                }
            }
        }

        return result.AsReadOnly();
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> GetStructuredFailureMessages()
    {
        if (IsValid)
        {
            throw new ValidationAssistantException("Can't get failures messages from a successful validation run.");
        }

        var result = new Dictionary<string, IReadOnlyList<string>>();

        for (int i = 0; i < _executionLogs.Count; i++)
        {
            var log = _executionLogs[i];
            if (log.Status == ExecutionStatus.Failed)
            {
                if (result.TryGetValue(log.KeyPath, out var list))
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
                        result[log.KeyPath] = messages;
                    }
                }
            }
        }

        return result.AsReadOnly();
    }

    internal void AddComponentLog(ValidationComponentExecutionLog log)
    {
        _executionLogs.Add(log);
    }

    internal void FailFast()
    {
        if (_executionLogs.Count > 1)
        {
            // Calculate the number of elements to remove (all but the last one).
            int elementsToRemove = _executionLogs.Count - 1;

            // Remove elements from the beginning of the list up to the second-to-last element.
            _executionLogs.RemoveRange(0, elementsToRemove);
        }
    }
}
