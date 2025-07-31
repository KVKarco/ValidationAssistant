using KVKarco.ValidationAssistant.Internal;
using System.Text;

namespace KVKarco.ValidationAssistant;

public sealed class ValidatorLog
{
    internal ValidatorLog(string validatorName, bool isValid, List<ValidatorComponentLog> componentsLogs)
    {
        FromValidator = validatorName;
        IsValidRun = isValid;
        ValidatorComponentsLogs = componentsLogs.AsReadOnly();
    }

    public string FromValidator { get; }

    public bool IsValidRun { get; }

    public IReadOnlyList<ValidatorComponentLog> ValidatorComponentsLogs { get; }

    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine(DefaultCodes.Lines);
        sb.AppendLine(DefaultCodes.Lines);

        sb.AppendLine();
        if (IsValidRun)
        {

            sb.Append($"Provided instance is valid!!!.");

            sb.AppendLine();
            foreach (var componentLog in ValidatorComponentsLogs)
            {
                componentLog.AttachToLog(sb);
            }
            sb.AppendLine();
        }

        sb.AppendLine();

        sb.AppendLine(DefaultCodes.Lines);
        sb.AppendLine(DefaultCodes.Lines);

        return sb.ToString();
    }
}