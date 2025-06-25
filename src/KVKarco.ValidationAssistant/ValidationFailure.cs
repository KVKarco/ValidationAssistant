using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.FailureAssets;
using System.Text;

namespace KVKarco.ValidationAssistant;

/// <summary>
/// Represents a single, specific validation failure that occurs for a property or a validation component.
/// This is a read-only record struct designed to encapsulate the details of a validation issue,
/// including its severity, message, and underlying information.
/// </summary>
public readonly record struct ValidationFailure
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFailure"/> record struct.
    /// This constructor is explicitly designed to prevent direct instantiation outside of the validation framework,
    /// ensuring that <see cref="ValidationFailure"/> instances are only created by validator logic via the <see cref="New"/> method.
    /// </summary>
    /// <exception cref="ValidationAssistantException">Thrown to indicate that this constructor should not be called directly by consumers.</exception>
    public ValidationFailure()
    {
        throw new ValidationAssistantException("ValidationFailure can be created only by validators.");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFailure"/> record struct with specified details.
    /// This private constructor is used by the static <see cref="New"/> factory method.
    /// </summary>
    /// <param name="info">The <see cref="ComponentFailureInfo"/> containing metadata and configuration for this specific failure.</param>
    /// <param name="message">A descriptive message explaining the reason for this validation failure.</param>
    private ValidationFailure(ComponentFailureInfo info, string message)
    {
        Info = info;
        Message = message;
    }

    /// <summary>
    /// Gets the severity level of this validation failure, derived from its underlying <see cref="Info"/>.
    /// </summary>
    public readonly FailureSeverity Severity => Info.Severity;

    /// <summary>
    /// Gets the descriptive message associated with this validation failure.
    /// </summary>
    public readonly string Message { get; }

    /// <summary>
    /// Gets the internal <see cref="ComponentFailureInfo"/> object that provides detailed metadata
    /// and configuration for this specific validation failure.
    /// </summary>
    internal readonly ComponentFailureInfo Info { get; }

    /// <summary>
    /// Appends the title from <see cref="Info"/> and the <see cref="Message"/> of this validation failure
    /// to the provided <see cref="StringBuilder"/>.
    /// This is typically used for constructing detailed explanation strings.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to which the failure information will be appended.</param>
    internal void AttachToExplanation(StringBuilder sb)
    {
        sb.AppendLine(Info.Title);
        sb.Append(Message);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ValidationFailure"/> record struct.
    /// This is the recommended factory method for creating <see cref="ValidationFailure"/> instances.
    /// </summary>
    /// <param name="info">The <see cref="ComponentFailureInfo"/> containing metadata and configuration for the failure.</param>
    /// <param name="message">A descriptive message explaining the reason for the validation failure.</param>
    /// <returns>A new <see cref="ValidationFailure"/> instance.</returns>
    internal static ValidationFailure New(ComponentFailureInfo info, string message)
        => new(info, message);
}
