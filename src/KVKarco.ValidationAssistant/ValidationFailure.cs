using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.FailureAssets;
using System.Text;

namespace KVKarco.ValidationAssistant;

//concept normal return to UI failures sorted by property path/can be empty if the failure is for main instance or resources
public readonly record struct ValidationFailure
{
    public ValidationFailure()
    {
        throw new ValidationAssistantException("ValidationFailure can be created only by validators.");
    }

    private ValidationFailure(ValidationFailureInfo info, FailureSeverity severity, string message)
    {
        Info = info;
        Severity = severity;
        Message = message;
    }

    public readonly FailureSeverity Severity { get; }

    public readonly string Message { get; }

    internal readonly ValidationFailureInfo Info { get; }

    internal void AttachToExplanation(StringBuilder sb)
    {
        sb.AppendLine(Info.Title);
        sb.Append(Message);
    }

    internal static ValidationFailure New(ValidationFailureInfo info, FailureSeverity severity, string message)
        => new(info, severity, message);
}
