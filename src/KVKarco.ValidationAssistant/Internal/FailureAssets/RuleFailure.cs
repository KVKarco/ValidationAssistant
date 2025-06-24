using System.Text;

namespace KVKarco.ValidationAssistant.Internal.FailureAssets;

internal abstract class RuleFailure
{
    protected RuleFailure(string path, RuleFailureInfo info, string explanation)
    {
        Path = path;
        Info = info;
        Explanation = explanation;
    }

    public string Path { get; }

    public RuleFailureInfo Info { get; }

    public string Explanation { get; }

    public abstract bool HasValidationFailures { get; }

    public abstract IReadOnlyCollection<ValidationFailure> ValidationFailures { get; }

    public abstract IReadOnlyCollection<string> ValidationFailuresMessages { get; }

    public abstract void AddValidationFailure(ValidationFailure failure);

    public abstract void AttachToExplanation(StringBuilder sb);

    //public bool CausedShortCircuit { get; }
}
