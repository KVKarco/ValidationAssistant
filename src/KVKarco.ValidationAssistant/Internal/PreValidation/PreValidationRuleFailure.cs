using System.Text;

namespace KVKarco.ValidationAssistant.Internal.PreValidation;

internal sealed class PreValidationRuleFailure : RuleFailure
{
    private ValidationFailure _validationFailure;

    public PreValidationRuleFailure(string path, RuleFailureInfo info, string explanation) : base(path, info, explanation)
    {
    }

    public sealed override bool HasValidationFailures => true;

    public sealed override IReadOnlyCollection<ValidationFailure> ValidationFailures => [_validationFailure];

    public sealed override IReadOnlyCollection<string> ValidationFailuresMessages => [_validationFailure.Message];

    public sealed override void AddValidationFailure(ValidationFailure failure)
    {
        _validationFailure = failure;
    }

    public sealed override void AttachToExplanation(StringBuilder sb)
    {
        sb.AppendLine(DefaultNaming.Lines);
        sb.AppendLine(Info.Title);
        sb.Append(Explanation);
        sb.AppendLine();
        _validationFailure.AttachToExplanation(sb);
        sb.AppendLine(DefaultNaming.Lines);
    }
}