using System.Collections.ObjectModel;
using System.Text;

namespace KVKarco.ValidationAssistant.Internal.FailureAssets;

/// <summary>
/// Failure from PropertyLogicalRule purely for explanation and collecting ValidationFailures,
/// does not invalidate the validation run by its on need to have ValidationFailures.
/// </summary>
internal sealed class PropertyRuleFailure : RuleFailure
{
    private List<ValidationFailure>? _validationFailures;

    public PropertyRuleFailure(string path, RuleFailureInfo info, string explanation) : base(path, info, explanation)
    {
    }

    public sealed override bool HasValidationFailures => _validationFailures is not null;

    public sealed override IReadOnlyCollection<ValidationFailure> ValidationFailures => _validationFailures is not null ? _validationFailures.AsReadOnly() : [];

    public sealed override IReadOnlyCollection<string> ValidationFailuresMessages =>
        _validationFailures is not null
        ? new ReadOnlyCollection<string>(_validationFailures.Where(x => x.Severity == FailureSeverity.Error).Select(x => x.Message).ToArray())
        : [];

    public sealed override void AddValidationFailure(ValidationFailure failure)
    {
        _validationFailures ??= [];
        _validationFailures.Add(failure);
    }

    public sealed override void AttachToExplanation(StringBuilder sb)
    {
        sb.AppendLine("---------------------------------------------------------------------------");
        sb.AppendLine(Info.Title);
        sb.Append(Explanation);

        if (_validationFailures is not null)
        {
            for (int i = 0; i < _validationFailures.Count; i++)
            {
                _validationFailures[i].AttachToExplanation(sb);
            }
        }

        sb.AppendLine("---------------------------------------------------------------------------");
    }
}