namespace KVKarco.ValidationAssistant.Internal.ValidationFlow;

internal sealed class LogicalRuleFailureInfo<T, TExternalResources> :
    RuleFailureInfo
{
    public LogicalRuleFailureInfo(
        int rulesToSkip,
        Func<ValidatorRunCtx<T, TExternalResources>, string> explanationFactory,
        ReadOnlySpan<char> validatorName,
        ReadOnlySpan<char> ruleName,
        int declaredOnLine,
        RuleFailureStrategy strategy)
        : base(validatorName, ruleName, declaredOnLine, strategy, rulesToSkip)
    {
        ExplanationFactory = explanationFactory;
    }

    public Func<ValidatorRunCtx<T, TExternalResources>, string> ExplanationFactory { get; }
}
