using KVKarco.ValidationAssistant.Internal;
using System.Globalization;
using System.Text;

namespace KVKarco.ValidationAssistant;

public sealed class ValidatorComponentLog
{
    private List<ValidationRuleLog>? _ruleLogs;

    internal ValidatorComponentLog(ComponentType type, string inValidator, string path, int declaredOnLine, ExecutionStatus status, string? explanation)
    {
        Type = type;
        InValidator = inValidator;
        Path = path;
        DeclaredOnLine = declaredOnLine;
        Status = status;
        Explanation = explanation;
    }

    public ComponentType Type { get; }

    public string InValidator { get; }

    public string Path { get; }

    public int DeclaredOnLine { get; }

    public ExecutionStatus Status { get; internal set; }

    public string? Explanation { get; internal set; }

    internal void AttachToLog(StringBuilder sb)
    {
        sb.AppendLine(DefaultCodes.Lines);
        sb.AppendLine(CultureInfo.InvariantCulture,
            $"""
            ComponentType    : {Type}
            InValidator      : {InValidator}
            Path             : {Path}
            DeclaredOnLine   : {DeclaredOnLine}
            ValidationStatus : {Status}
            Explanation      :
            """);
        if (Explanation is not null)
        {
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"Explanation      : {Explanation}");
        }

        sb.AppendLine();

        if (_ruleLogs is not null)
        {
            foreach (var ruleLog in _ruleLogs)
            {
                ruleLog.AttachToLog(sb);
            }
        }
        sb.AppendLine(DefaultCodes.Lines);
    }

    internal void RuleSucceeded(int declaredOnLine)
    {
        _ruleLogs ??= [];
        _ruleLogs.Add(ValidationRuleLog.ForSuccess(declaredOnLine));
    }

    internal void RuleFailed(int declaredOnLine, string code, Severity severity, string failureMessage)
    {
        _ruleLogs ??= [];
        _ruleLogs.Add(ValidationRuleLog.ForFailure(declaredOnLine, code, severity, failureMessage));
    }

    internal void RuleInformational(int declaredOnLine, string explanation)
    {
        _ruleLogs ??= [];
        _ruleLogs.Add(ValidationRuleLog.ForInformational(declaredOnLine, explanation));
    }

    internal IEnumerable<ValidationRuleFailure> GetFailures()
    {
        if (_ruleLogs is null)
        {
            yield break;
        }

        foreach (var ruleLog in _ruleLogs)
        {
            if (ruleLog.Failure is not null)
            {
                yield return ruleLog.Failure.Value;
            }
        }
    }

    internal IEnumerable<string> GetFailureMessages()
    {
        if (_ruleLogs is null)
        {
            yield break;
        }

        foreach (var ruleLog in _ruleLogs)
        {
            if (ruleLog.Failure is not null)
            {
                yield return ruleLog.Failure.Value.FailureMessage;
            }
        }
    }



    internal static ValidatorComponentLog ForLogicalBlock(string inValidator, string path, int declaredOnLine, string explanation)
        => new(ComponentType.ConditionalBlock, inValidator, path, declaredOnLine, ExecutionStatus.Informational, explanation);

    internal static ValidatorComponentLog ForProperty(string inValidator, string path, int declaredOnLine)
        => new(ComponentType.PropertyValidator, inValidator, path, declaredOnLine, ExecutionStatus.Passed, "Has no failed validation rules.");
}

