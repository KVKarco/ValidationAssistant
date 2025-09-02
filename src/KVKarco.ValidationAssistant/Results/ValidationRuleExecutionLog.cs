using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.Utilities;
using System.Text;

namespace KVKarco.ValidationAssistant.Results;

public readonly record struct ValidationRuleExecutionLog
{
    public ValidationRuleExecutionLog()
    {
        throw new ValidationAssistantException("Creating ValidationRuleExecutionInfo can be done only by the framework.");
    }

    internal ValidationRuleExecutionLog(
        string code,
        ExecutionStatus status,
        int declaredOnLine,
        string? hint,
        ValidationRuleFailure? failure)
    {
        Status = status;
        DeclaredOnLine = declaredOnLine;
        Hint = hint;
        Failure = failure;
        Code = code;
    }

    public string Code { get; }

    public ExecutionStatus Status { get; }

    public int DeclaredOnLine { get; }

    public string? Hint { get; }

    public ValidationRuleFailure? Failure { get; }

    internal void AppendToLog(StringBuilder sb)
    {
        sb.AppendLine(InternalDefaults.Code);
        sb.Append(Code);
        sb.AppendLine(InternalDefaults.Status);
        sb.Append(Status);
        sb.AppendLine(InternalDefaults.OnLine);
        sb.Append(DeclaredOnLine);
        if (Hint is not null)
        {
            sb.AppendLine(InternalDefaults.Hint);
            sb.Append(Hint);
        }
        if (Failure.HasValue)
        {
            sb.AppendLine();
            Failure.Value.AppendToLog(sb);
            sb.AppendLine();
        }
    }
}

