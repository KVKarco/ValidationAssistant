using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.Utilities;
using System.Text;

namespace KVKarco.ValidationAssistant.Results;

public readonly record struct ValidationRuleFailure
{
    public ValidationRuleFailure()
    {
        throw new ValidationAssistantException("Creating ValidationRuleFailures can be done only by the framework.");
    }

    internal ValidationRuleFailure(string code, Severity severity, string message)
    {
        Code = code;
        Severity = severity;
        Message = message;
    }

    public string Code { get; }

    public Severity Severity { get; }

    public string Message { get; }

    internal void AppendToLog(StringBuilder sb)
    {
        sb.AppendLine(InternalDefaults.Code);
        sb.Append(Code);
        sb.AppendLine(InternalDefaults.Severity);
        sb.Append(Severity);
        sb.AppendLine(InternalDefaults.Message);
        sb.Append(Message);
    }
}
