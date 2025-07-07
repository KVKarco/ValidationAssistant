namespace KVKarco.ValidationAssistant.Internal.PreValidation;

internal sealed class PreValidationRuleFailureInfo<T, TExternalResources> :
    RuleFailureInfo
{
    public PreValidationRuleFailureInfo(
        ReadOnlySpan<char> validatorName,
        ReadOnlySpan<char> ruleName,
        int declaredOnLine,
        string? explanationMessage)
        : base(validatorName, ruleName, declaredOnLine, RuleFailureStrategy.Stop, 0)
    {
        ExplanationFactory = explanationMessage is null ? ValidatorsConfig.GlobalDefaults.Messages.PreValidationDefaultExplanation : (_) => explanationMessage;
    }

    public Func<ValidatorRunCtx<T, TExternalResources>, string> ExplanationFactory { get; }
}