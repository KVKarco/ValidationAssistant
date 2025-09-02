using KVKarco.ValidationAssistant.Internal.Utilities;
using KVKarco.ValidationAssistant.Internal.ValidationRules;
using System.Text;

namespace KVKarco.ValidationAssistant.Results;

public sealed class ValidationComponentExecutionLog
{
    internal ValidationComponentExecutionLog(
        ComponentType type,
        string inValidator,
        string path,
        int declaredOnLine,
        ExecutionStatus status,
        string? explanation)
    {
        Type = type;
        InValidator = inValidator;
        TargetPath = path;
        KeyPath = path;
        DeclaredOnLine = declaredOnLine;
        Status = status;
        Explanation = explanation;
    }

    public ComponentType Type { get; }

    public string InValidator { get; }

    public string TargetPath { get; }

    public string KeyPath { get; internal set; }

    public int DeclaredOnLine { get; }

    public ExecutionStatus Status { get; internal set; }

    public string? Explanation { get; internal set; }

    public IReadOnlyList<ValidationRuleExecutionLog>? ExecutionLog { get; internal set; }

    internal void AttachToLog(IRuleOptions options, ExecutionStatus status, string? message)
    {
        ExecutionLog ??= new List<ValidationRuleExecutionLog>();

        string? internalMessage = status switch
        {
            ExecutionStatus.Passed => null,
            ExecutionStatus.Failed => message,
            ExecutionStatus.Skipped => "Rule is not executed because condition attached to it is not valid",
            _ => throw new NotImplementedException(),
        };

        ValidationRuleExecutionLog ruleLog = new(
            options.Code,
            status,
            options.DeclaredOnLine,
            options.IsValidationRule ? null : internalMessage,
            status == ExecutionStatus.Failed && options.IsValidationRule ? new(options.Code, options.FailureSeverity, message!) : null);

        ((List<ValidationRuleExecutionLog>)ExecutionLog).Add(ruleLog);
    }

    internal IEnumerable<ValidationRuleFailure> GetFailures()
    {
        if (ExecutionLog is null)
        {
            yield break;
        }

        foreach (var ruleLog in ExecutionLog)
        {
            if (ruleLog.Failure is not null)
            {
                yield return ruleLog.Failure.Value;
            }
        }
    }

    internal IEnumerable<string> GetFailureMessages()
    {
        if (ExecutionLog is null)
        {
            yield break;
        }

        foreach (var ruleLog in ExecutionLog)
        {
            if (ruleLog.Failure is not null)
            {
                yield return ruleLog.Failure.Value.Message;
            }
        }
    }

    internal void AppendToLog(StringBuilder sb)
    {
        sb.AppendLine(InternalDefaults.Lines);
        sb.AppendLine(InternalDefaults.ComponentType);
        sb.Append(Type);
        sb.AppendLine(InternalDefaults.InValidator);
        sb.Append(InValidator);
        sb.AppendLine(InternalDefaults.TargetPath);
        sb.Append(TargetPath);
        sb.AppendLine(InternalDefaults.KeyPath);
        sb.Append(KeyPath);
        sb.AppendLine(InternalDefaults.DeclaredOnLine);
        sb.Append(DeclaredOnLine);
        sb.AppendLine(InternalDefaults.Status);
        sb.Append(Status);

        if (Explanation is not null)
        {
            sb.AppendLine(InternalDefaults.Explanation);
            sb.Append(Explanation);
        }

        if (ExecutionLog is not null)
        {
            sb.AppendLine();
            foreach (var ruleLog in ExecutionLog)
            {
                ruleLog.AppendToLog(sb);
            }
            sb.AppendLine();
        }
        sb.AppendLine(InternalDefaults.Lines);
    }
}

