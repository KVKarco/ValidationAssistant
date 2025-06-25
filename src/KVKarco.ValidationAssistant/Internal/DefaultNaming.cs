namespace KVKarco.ValidationAssistant.Internal;

internal static class DefaultNaming
{
    internal const string Lines = "--------------------------------------------------------------------------";

    #region generic validation rules names

    internal const string ConditionalValRule = "ConditionalFlow-ValidationRule";
    internal const string ConditionalAsyncValRule = "ConditionalFlow-AsyncValidationRule";

    internal const string PredicateValRule = "Predicate-ValidationRule";
    internal const string PredicateAsyncValRule = "Predicate-AsyncValidationRule";

    internal const string ContextAwarePropertyValRule = "ContextAwarePredicate-ValidationRule";
    internal const string ContextAwarePredicateAsyncValRule = "ContextAwarePredicate-AsyncValidationRule";

    internal const string CustomNestedValidatorValRule = "CustomNestedValidator-ValidationRule";
    internal const string CustomNestedValidatorAsyncValRule = "CustomNestedValidator-AsyncValidationRule";

    internal const string InlineNestedValidatorValRule = "InlineNestedValidator-ValidationRule";
    internal const string InlineNestedValidatorAsyncValRule = "InlineNestedValidator-AsyncValidationRule";

    internal const string SnapShotValRule = "SnapShot-ValidationRule";

    internal const string MainInstancePreValidationName = "MainInstance-PreValidationRule";
    internal const string MainInstanceAsyncPreValidationName = "MainInstance-AsyncPreValidationRule";

    internal const string ExternalResourcesPreValidationName = "ExternalResources-PreValidationRule";
    internal const string ExternalResourcesAsyncPreValidationName = "ExternalResources-AsyncPreValidationRule";

    #endregion

    #region build-in validation rules names

    internal const string NotNullValRule = "NotNull-ValidationRule";
    internal const string MustBeNullValRule = "MustBeNull-ValidationRule";

    #endregion

    #region generic validator rules names

    internal const string ConditionWhenRule = "ConditionWhen-ValidatorRule";
    internal const string ConditionWhenAsyncRule = "ConditionWhen-AsyncValidatorRule";

    internal const string ConditionOtherwiseRule = "ConditionOtherwise-ValidatorRule";
    internal const string ConditionOtherwiseAsyncRule = "ConditionOtherwise-AsyncValidatorRule";

    internal const string PropertyRule = "Property-ValidatorRule";
    internal const string PropertyAsyncRule = "Property-AsyncValidatorRule";

    #endregion
}
