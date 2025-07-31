namespace KVKarco.ValidationAssistant.Internal;

internal static class DefaultCodes
{
    internal const string Lines = "--------------------------------------------------------------------------";

    #region generic validation rules names

    internal const string ConditionalValRule = "ConditionalFlowRule";
    internal const string PredicateValRule = "PredicateRule";
    internal const string CustomNestedValidatorValRule = "ValidatorAsRule";
    internal const string CustomNestedValidatorAsyncValRule = "ValidatorAsRule";
    internal const string SnapShotValRule = "SnapShotRule";

    #endregion

    #region build-in validation rules names

    internal const string NotNullValRule = "NotNullRule";

    #endregion
}
