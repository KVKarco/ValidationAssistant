namespace KVKarco.ValidationAssistant.Internal.FailureAssets;

/// <summary>
/// Represents the structured title information for a ValidatorRule failure.
/// This class encapsulates details about the failing rule, including its name,
/// where it was declared, and how influence the validation run.
/// </summary>
internal sealed class RuleFailureInfo
{
    public RuleFailureInfo(ReadOnlySpan<char> validatorName, ReadOnlySpan<char> ruleName, int declaredOnLine, RuleFailureStrategy strategy)
    {
        Title = $"""

            InValidator       : {validatorName}
            RuleName          : {ruleName}
            DeclaredOnLine    : {declaredOnLine}
            Explanation       : 

            """;

        Strategy = strategy;
        DeclaredOnLine = declaredOnLine;
    }

    public int DeclaredOnLine { get; }

    public RuleFailureStrategy Strategy { get; }

    public string Title { get; set; }

    public static ValidationFailureInfo New(ReadOnlySpan<char> ruleName, int declaredOnLine, FailureSeverity severity, ComponentFailureStrategy strategy)
        => new(ruleName, declaredOnLine, severity, strategy);
}
