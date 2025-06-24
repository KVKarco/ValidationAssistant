namespace KVKarco.ValidationAssistant;

/// <summary>
/// Defines how a validation failure is treated and its impact on the validation result and client communication.
/// </summary>
public enum FailureSeverity
{
    /// <summary>
    /// The failure will be sent to the client (usually UI), and the validation result will be invalid.
    /// An explanation of the failure is added to the result, typically for logging.
    /// </summary>
    Error,

    /// <summary>
    /// The failure will not be sent to the client, but the validation result will still be invalid.
    /// An explanation of the failure is added to the result, typically for logging.
    /// </summary>
    Warning,

    /// <summary>
    /// The failure will not be sent to the client, and the validation result remains valid.
    /// An explanation of the failure is added to the result, typically for logging.
    /// </summary>
    Info
}