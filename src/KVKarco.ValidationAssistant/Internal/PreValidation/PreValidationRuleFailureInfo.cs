namespace KVKarco.ValidationAssistant.Internal.PreValidation;

internal sealed class PreValidationRuleFailureInfo<T, TExternalResources> :
    RuleFailureInfo
{
    public PreValidationRuleFailureInfo(
        Func<ValidatorRunCtx<T, TExternalResources>, int, string> explanationFactory,
        ReadOnlySpan<char> validatorName,
        ReadOnlySpan<char> ruleName,
        int declaredOnLine,
        RuleFailureStrategy strategy)
        : base(validatorName, ruleName, declaredOnLine, strategy, 0)
    {
        ExplanationFactory = explanationFactory;
    }

    public Func<ValidatorRunCtx<T, TExternalResources>, int, string> ExplanationFactory { get; }
}