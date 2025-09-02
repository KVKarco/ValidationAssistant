using KVKarco.ValidationAssistant.Internal.ValidationRules;

namespace KVKarco.ValidationAssistant.Internal.Utilities;

internal static class InternalDefaults
{
    internal const string Lines = "--------------------------------------------------------------------------";
    internal const string ComponentType = "ComponentType  : ";
    internal const string InValidator = "InValidator    : ";
    internal const string TargetPath = "TargetPath     : ";
    internal const string KeyPath = "KeyPath        : ";
    internal const string DeclaredOnLine = "DeclaredOnLine : ";
    internal const string Status = "Status         : ";
    internal const string Explanation = "Explanation    : ";
    internal const string Code = "Code     :";
    internal const string OnLine = "OnLine   : ";
    internal const string Severity = "Severity : ";
    internal const string Message = "Message  : ";
    internal const string Hint = "Hint     : ";
    internal const string ResultTitle = "Validation run from validator '";
    internal const string ResultSuccess = "' was successful.";
    internal const string ResultFailure = "' failed.";

    #region generic validation rules names

    internal const string ConditionalValRule = "ConditionalFlowRule";
    internal const string ConditionalAsyncValRule = "ConditionalFlowAsyncRule";

    internal const string PredicateValRule = "PredicateRule";
    internal const string PredicateAsyncValRule = "PredicateAsyncRule";

    internal const string CustomValidatorValRule = "ValidatorAsRule";
    internal const string CustomValidatorAsyncValRule = "ValidatorAsAsyncRule";
    internal const string SnapShotValRule = "SnapShotRule";

    #endregion

    #region build-in validation rules names

    internal const string NotNullValRule = "NotNullRule";

    #endregion

    #region exceptions messages

    //message templates exception messages:
    public const string TemplateNotSet = "Template not set. Call ForNew() first.";
    public const string TemplateNameNullOrEmpty = "Can`t register message template, templateName is missing.";
    public const string TemplateSourceNull = "Can`t register message template/s from empty source.";
    public const string TemplatePlaceholderReplacementNullOrEmpty = "Placeholder replacement can`t be empty.";
    public const string TemplateStringNullOrEmpty = "Can`t register message template from empty string";
    public const string TemplateCultureNull = "Can`t register message template with null culture info.";
    public const string TemplateMissingPlaceholderReplacementsCantBeFormatted = "When getting message you must first replace all placeholders.";
    public const string TemplatePlaceholderReplacementNotInTemplate = "Placeholder provided is not part of the current message template.";


    public const string SelectorNull = "Selector cannot be null.";
    public const string ConstantsNotAllowed = "Cannot use constant values in a property selector. A property or field access is required.";
    public const string StaticMemberNotAllowed = "Static member cannot be used in selector expressions. Select instance members only.";
    public const string ValuePropertyAtRootNotAllowed = "Selector expression cannot end with the '.Value' property of a Nullable<T> type.";
    public const string MethodCallsNotAllowed = "Method calls are not allowed in selector expressions.";
    public const string CastsNotAllowed = "Cast expressions are not allowed in selector expressions.";
    public const string IndexersNotAllowed = "Indexer access is not allowed in selector expressions.";
    public const string BinaryExpressionsNotAllowed = "Binary expressions are not allowed in selector expressions.";
    public const string AssignmentsNotAllowed = "Assignment expressions are not allowed in selector expressions.";
    public const string InvocationNotAllowed = "Invocation of delegates or lambdas is not allowed in selector expressions.";
    public const string NewObjectsNotAllowed = "Anonymous or new object constructions are not allowed in selector expressions.";
    public const string ConditionalsNotAllowed = "Conditional expressions are not allowed in selector expressions.";
    public const string DefaultsNotAllowed = "Default expressions are not allowed in selector expressions.";
    public const string UnsupportedExpression = "Unsupported expression type in selector expressions.";

    public const string PredicateCannotBeNull = "Predicate cannot be null. A valid Predicate must be provided when defining validation logic.";
    public const string PredicateHasClosure = """
                Invalid Predicate detected: is bound to an instance or captures closure context.

                ⚠️ This means the lambda or method you're using references variables or instance state from outside its own scope.
                
                💡 Tip:
                Use static methods or pure lambdas that only rely on the provided context.
                Avoid capturing outer-scope variables or using instance methods from the validation class.
                
                🔒 Why it's disallowed:
                Captured state can introduce:
                - memory leaks (closure holds on to outer objects)
                - thread safety issues
                - nondeterministic behavior in reused validation logic
                """;

    public const string ConditionCannotBeNull = "Condition cannot be null. A valid Condition must be provided when defining validation logic.";
    public const string ConditionHasClosure = """
                Invalid Condition detected: is bound to an instance or captures closure context.

                ⚠️ This means the lambda or method you're using references variables or instance state from outside its own scope.
                
                💡 Tip:
                Use static methods or pure lambdas that only rely on the provided context.
                Avoid capturing outer-scope variables or using instance methods from the validation class.
                
                🔒 Why it's disallowed:
                Captured state can introduce:
                - memory leaks (closure holds on to outer objects)
                - thread safety issues
                - nondeterministic behavior in reused validation logic
                """;

    public static string AsyncRuleCalledSync(IRuleOptions options)
    {
        if (options.HasAsyncCondition)
        {
            return $"ValidationRule '{options.Code}' has a asynchronous condition attached but was call synchronously";
        }

        return $"ValidationRule '{options.Code}' is asynchronous but was call synchronously";
    }

    public const string CriticalFailure = "Internal problem.";
    public const string InvalidInput = "Invalid input.";
    public const string AsyncValidatorCalledSync = "Validation contains asynchronous code but was call synchronously.";

    public const string CodeMissing = "Can`t create validation rule with empty Code.";
    public const string SnapShotNullOrEmpty = "SnapShot can`t be null or empty.";
    public const string SnapShotIsNotCaptured = "To use SnapShot first need to be Captured.";
    public const string FailureMessageNullOrEmpty = "Can`t create validation rule with empty failure message";
    public const string MessageFactoryNull = "Can`t create validation rule with null FailureMessageFactory";
    public const string MessageFactoryClosure = """
                Invalid FailureMessageFactory detected: is bound to an instance or captures closure context.

                ⚠️ This means the lambda or method you're using references variables or instance state from outside its own scope.
                
                💡 Tip:
                Use static methods or pure lambdas that only rely on the provided context.
                Avoid capturing outer-scope variables or using instance methods from the validation class.
                
                🔒 Why it's disallowed:
                Captured state can introduce:
                - memory leaks (closure holds on to outer objects)
                - thread safety issues
                - nondeterministic behavior in reused validation logic
                """;

    public static string PropertyValueUndefined(ValidationCtx context, string missingMember) =>
        $"Property value can`t be extracted member: '{missingMember}' is null in the PropertyPath: '{context.CorrectPropertyPath}'";

    #endregion



    internal const string RuleNameNotEmpty = "Name supplied is empty, ValidationRule need a valid name.";
}
