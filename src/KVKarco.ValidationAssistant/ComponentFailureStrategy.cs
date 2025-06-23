namespace KVKarco.ValidationAssistant;

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