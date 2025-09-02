namespace KVKarco.ValidationAssistant.Internal.ValidationRules;

/// <summary>
/// Provides a read-only contract for the static configuration options of a validation rule.
/// </summary>
/// <remarks>
/// This interface is an internal-facing contract used by framework components, such as contexts,
/// message formatters, and logging services, to access a rule's metadata. It ensures that these
/// components can obtain essential information about a rule without needing to know its generic types or execution logic.
/// </remarks>
internal interface IRuleOptions
{
    /// <summary>
    /// Gets a unique code that identifies the validation rule.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Gets the line number where the validation rule was declared, useful for debugging.
    /// </summary>
    int DeclaredOnLine { get; }

    /// <summary>
    /// Gets the severity of the rule's failure.
    /// </summary>
    Severity FailureSeverity { get; }

    /// <summary>
    /// Gets the flow effect of a failure, indicating how the validation should proceed.
    /// </summary>
    FlowEffect FailureLocalFlowImpact { get; }

    /// <summary>
    /// Gets a value indicating whether this rule is a validation rule.
    /// </summary>
    bool IsValidationRule { get; init; }

    /// <summary>
    /// Gets a value indicating whether the rule can be executed synchronously.
    /// </summary>
    public bool CanExceedSynchronously { get; }

    /// <summary>
    /// Gets a value indicating whether the rule's condition is asynchronous.
    /// </summary>
    bool HasAsyncCondition { get; }
}
