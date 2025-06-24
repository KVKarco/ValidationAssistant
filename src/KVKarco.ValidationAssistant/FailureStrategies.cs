namespace KVKarco.ValidationAssistant;

/// <summary>
/// Defines the overall flow control behavior of the validation process when a
/// **validator rule** (e.g., a property rule, conditional flow rule, ...)
/// encounters a failure. This strategy dictates how the containing validator proceeds with other rules.
/// </summary>
public enum RuleFailureStrategy
{
    /// <summary>
    /// If this validator rule fails, the validation process continues to evaluate any
    /// subsequent rules defined within the containing validator.
    /// All failures from this rule and subsequent rules will be collected.
    /// </summary>
    Continue,

    /// <summary>
    /// If this validator rule fails, the entire validation process for the containing validator
    /// halts immediately. No further rules or validations will be evaluated,
    /// and only errors collected up to this point will be reported.
    /// </summary>
    Stop,
}

/// <summary>
/// Specifies the granular flow control behavior when an individual **validation component**
/// (e.g., a predicate, a custom validation rule, or a nested validator) within a single
/// validator rule (mostly used in property rules) fails.
/// </summary>
public enum ComponentFailureStrategy
{
    /// <summary>
    /// If an individual component fails, the current validator rule continues to execute any
    /// remaining components within itself. All failures from this and subsequent
    /// components in the same rule will be collected.
    /// </summary>
    Continue,

    /// <summary>
    /// If an individual component fails, the execution of the current validator rule
    /// stops immediately, skipping any remaining components within that rule.
    /// The containing validator may then proceed with other rules based on its
    /// <see cref="RuleFailureStrategy"/>.
    /// </summary>
    Exit,

    /// <summary>
    /// If an individual component fails, the execution of the current validator rule
    /// stops immediately, skipping any remaining components within that rule.
    /// Furthermore, this action explicitly halts the *entire validation process*
    /// for the containing validator, effectively overriding its <see cref="RuleFailureStrategy"/>.
    /// No further rules or components will be evaluated beyond this point.
    /// </summary>
    Stop,
}
