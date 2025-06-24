using System.Text;

namespace KVKarco.ValidationAssistant.Internal.FailureAssets;

/// <summary>
/// Failure from ValidatorLogicalRule purely for explanation,
/// does not invalidate the validation run.
/// </summary>
internal sealed class LogicalRuleFailure : RuleFailure
{
    public LogicalRuleFailure(RuleFailureInfo info, string explanation) : base("", info, explanation)
    {
    }

    public sealed override bool HasValidationFailures => false;

    public sealed override IReadOnlyCollection<ValidationFailure> ValidationFailures => throw new NotImplementedException();

    public sealed override IReadOnlyCollection<string> ValidationFailuresMessages => throw new NotImplementedException();

    public sealed override void AddValidationFailure(ValidationFailure failure) => throw new NotImplementedException();

    public sealed override void AttachToExplanation(StringBuilder sb)
    {
        sb.AppendLine("---------------------------------------------------------------------------");
        sb.AppendLine(Info.Title);
        sb.Append(Explanation);
        sb.AppendLine("---------------------------------------------------------------------------");
    }
}
