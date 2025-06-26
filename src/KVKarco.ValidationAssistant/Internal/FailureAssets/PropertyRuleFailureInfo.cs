namespace KVKarco.ValidationAssistant.Internal.FailureAssets;

internal sealed class PropertyRuleFailureInfo<T, TExternalResources, TProperty> :
    RuleFailureInfo
{
    public PropertyRuleFailureInfo(
        Func<ValidatorRunCtx<T, TExternalResources>, TProperty, string> explanationFactory,
        ReadOnlySpan<char> validatorName,
        ReadOnlySpan<char> ruleName,
        int declaredOnLine,
        RuleFailureStrategy strategy)
        : base(validatorName, ruleName, declaredOnLine, strategy, 0)
    {
        ExplanationFactory = explanationFactory;
    }

    public Func<ValidatorRunCtx<T, TExternalResources>, TProperty, string> ExplanationFactory { get; }
}