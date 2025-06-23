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
