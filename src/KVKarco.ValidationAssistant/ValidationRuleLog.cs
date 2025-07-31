using KVKarco.ValidationAssistant.Exceptions;
using System.Globalization;
using System.Text;

namespace KVKarco.ValidationAssistant;

public readonly record struct ValidationRuleLog
{
    public ValidationRuleLog()
    {
        throw new ValidationAssistantException("Creating ValidationRuleExecutionInfo can be done only by the framework.");
    }

    internal ValidationRuleLog(ExecutionStatus status, int declaredOnLine, string? explanation, ValidationRuleFailure? failure)
    {
        Status = status;
        DeclaredOnLine = declaredOnLine;
        Explanation = explanation;
        Failure = failure;
    }

    public ExecutionStatus Status { get; }

    public int DeclaredOnLine { get; }

    public string? Explanation { get; }

    public ValidationRuleFailure? Failure { get; }

    internal static ValidationRuleLog ForSuccess(int declaredOnLine)
        => new(ExecutionStatus.Passed, declaredOnLine, null, null);

    internal static ValidationRuleLog ForFailure(int declaredOnLine, string code, Severity severity, string failureMessage)
        => new(ExecutionStatus.Failed, declaredOnLine, null, new ValidationRuleFailure(code, severity, failureMessage));

    internal static ValidationRuleLog ForInformational(int declaredOnLine, string explanation)
        => new(ExecutionStatus.Informational, declaredOnLine, explanation, null);

    internal void AttachToLog(StringBuilder sb)
    {
        sb.Append(CultureInfo.InvariantCulture,
            $"""

            ValidationStatus : {Status}
            DeclaredOnLine   : {DeclaredOnLine}
            """);

        if (Explanation is not null)
        {
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"Explanation      : {Explanation}");
        }



        if (Failure.HasValue)
        {
            Failure.Value.AttachToLog(sb);
        }

        sb.AppendLine();
    }
}

