namespace KVKarco.ValidationAssistant.Exceptions;

/// <summary>
/// Represents the base exception type that can be thrown by validator methods within the ValidationAssistant system.
/// </summary>
/// <remarks>
/// This exception serves as the primary mechanism for signaling unexpected conditions encountered during the validation process.
/// Developers should catch this type to handle specific validation-related errors.
/// </remarks>
public class ValidationAssistantException : Exception
{
    internal ValidationAssistantException()
    {
    }

    internal ValidationAssistantException(string message) : base(message)
    {
    }

    internal ValidationAssistantException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

