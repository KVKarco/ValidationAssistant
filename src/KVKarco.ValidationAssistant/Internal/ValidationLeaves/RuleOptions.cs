namespace KVKarco.ValidationAssistant.Internal.ValidationRules;


/// <summary>
/// Provides a final, immutable implementation of the <see cref="IRuleOptions"/> interface.
/// </summary>
/// <remarks>
/// This sealed class serves as a Data Transfer Object (DTO) for the static configuration
/// and metadata of a validation rule. Its state is fully determined by the
/// constructor arguments, which are typically provided by a builder,
/// ensuring the object's consistency and immutability for thread-safe operations.
/// </remarks>
internal sealed class RuleOptions : IRuleOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuleOptions"/> class.
    /// </summary>
    /// <param name="code">The unique code for the rule.</param>
    /// <param name="declaredOnLine">The line number where the rule was declared.</param>
    /// <param name="failureSeverity">The severity of the failure.</param>
    /// <param name="failureLocalFlowImpact">The flow effect of the failure.</param>
    /// <param name="isValidationRule">A value indicating whether this is a validation rule.</param>
    /// <param name="canExceedSynchronously">A value indicating whether the rule can be executed synchronously.</param>
    /// <param name="hasAsyncCondition">A value indicating whether the rule has an asynchronous condition.</param>
    public RuleOptions(
        string code,
        int declaredOnLine,
        Severity failureSeverity,
        FlowEffect failureLocalFlowImpact,
        bool isValidationRule,
        bool canExceedSynchronously,
        bool hasAsyncCondition)
    {
        Code = code;
        DeclaredOnLine = declaredOnLine;
        FailureSeverity = failureSeverity;
        FailureLocalFlowImpact = failureLocalFlowImpact;
        IsValidationRule = isValidationRule;
        CanExceedSynchronously = canExceedSynchronously;
        HasAsyncCondition = hasAsyncCondition;
    }

    /// <inheritdoc/>
    public string Code { get; init; }

    /// <inheritdoc/>
    public int DeclaredOnLine { get; init; }

    /// <inheritdoc/>
    public Severity FailureSeverity { get; init; }

    /// <inheritdoc/>
    public FlowEffect FailureLocalFlowImpact { get; init; }

    /// <inheritdoc/>
    public bool IsValidationRule { get; init; }

    /// <inheritdoc/>
    public bool CanExceedSynchronously { get; }

    /// <inheritdoc/>
    public bool HasAsyncCondition { get; }
}
