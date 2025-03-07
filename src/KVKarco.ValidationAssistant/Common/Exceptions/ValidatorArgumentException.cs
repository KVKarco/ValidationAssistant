namespace KVKarco.ValidationAssistant.Common.Exceptions;

/// <summary>
/// Custom exception for better developer experience.
/// </summary>
public class ValidatorArgumentException : Exception
{
    /// <summary>
    /// Creates ValidatorArgumentException with message.
    /// </summary>
    public ValidatorArgumentException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates ValidatorArgumentException with message, and inner exception.
    /// </summary>
    public ValidatorArgumentException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates empty ValidatorArgumentException.
    /// </summary>
    public ValidatorArgumentException()
    {
    }
}
