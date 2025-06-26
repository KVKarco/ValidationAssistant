namespace KVKarco.ValidationAssistant.Internal.FailureAssets;

internal sealed class LogicalRuleFailureInfo<T, TExternalResources> :
    RuleFailureInfo
{
    public LogicalRuleFailureInfo(
        int rulesToSkip,
        Func<ValidatorRunCtx<T, TExternalResources>, int, string> explanationFactory,
        ReadOnlySpan<char> validatorName,
        ReadOnlySpan<char> ruleName,
        int declaredOnLine,
        RuleFailureStrategy strategy)
        : base(validatorName, ruleName, declaredOnLine, strategy, rulesToSkip)
    {
        ExplanationFactory = explanationFactory;
    }

    public Func<ValidatorRunCtx<T, TExternalResources>, int, string> ExplanationFactory { get; }
}
