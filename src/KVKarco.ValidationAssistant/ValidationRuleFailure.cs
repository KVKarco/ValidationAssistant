using KVKarco.ValidationAssistant.Exceptions;
using System.Globalization;
using System.Text;

namespace KVKarco.ValidationAssistant;

public readonly record struct ValidationRuleFailure
{
    public ValidationRuleFailure()
    {
        throw new ValidationAssistantException("Creating ValidationRuleFailures can be done only by the framework.");
    }

    internal ValidationRuleFailure(string code, Severity failureSeverity, string failureMessage)
    {
        Code = code;
        FailureSeverity = failureSeverity;
        FailureMessage = failureMessage;
    }

    public string Code { get; }

    public Severity FailureSeverity { get; }

    public string FailureMessage { get; }

    internal void AttachToLog(StringBuilder sb)
    {
        sb.AppendLine();

        sb.AppendLine("Failure: ");
        sb.AppendLine(CultureInfo.InvariantCulture,
            $"""
            RuleName(Code) : {Code}
            Severity       : {FailureSeverity}
            Message        : {FailureMessage}
            """);

        sb.AppendLine();
    }
}

