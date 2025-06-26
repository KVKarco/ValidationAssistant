using KVKarco.ValidationAssistant.Internal.PropertyValidation;
using KVKarco.ValidationAssistant.Internal.ValidationFlow;

namespace KVKarco.ValidationAssistant.Internal;

/// <summary>
/// Represents the structured title information for a ValidatorRule failure.
/// This class encapsulates details about the failing rule, including its name,
/// where it was declared, and how influence the validation run.
/// </summary>
internal abstract class RuleFailureInfo
{
    public RuleFailureInfo(ReadOnlySpan<char> validatorName, ReadOnlySpan<char> ruleName, int declaredOnLine, RuleFailureStrategy strategy, int rulesToSkip)
    {
        Title = $"""

            InValidator       : {validatorName}
            RuleName          : {ruleName}
            DeclaredOnLine    : {declaredOnLine}
            Explanation       : 
            """;

        Strategy = strategy;
        DeclaredOnLine = declaredOnLine;
        RulesToSkip = rulesToSkip;
    }

    public int RulesToSkip { get; }

    public int DeclaredOnLine { get; }

    public RuleFailureStrategy Strategy { get; }

    public string Title { get; set; }

    public static LogicalRuleFailureInfo<T, TExternalResources> ForLogicalRule<T, TExternalResources>(
        int rulesToSkip,
        Func<ValidatorRunCtx<T, TExternalResources>, int, string> explanationFactory,
        ReadOnlySpan<char> validatorName,
        ReadOnlySpan<char> ruleName,
        int declaredOnLine,
        RuleFailureStrategy strategy)
        => new(rulesToSkip, explanationFactory, validatorName, ruleName, declaredOnLine, strategy);

    public static PropertyRuleFailureInfo<T, TExternalResources, TProperty> ForLogicalRule<T, TExternalResources, TProperty>(
        Func<ValidatorRunCtx<T, TExternalResources>, TProperty, string> explanationFactory,
        ReadOnlySpan<char> validatorName,
        ReadOnlySpan<char> ruleName,
        int declaredOnLine,
        RuleFailureStrategy strategy)
        => new(explanationFactory, validatorName, ruleName, declaredOnLine, strategy);
}
