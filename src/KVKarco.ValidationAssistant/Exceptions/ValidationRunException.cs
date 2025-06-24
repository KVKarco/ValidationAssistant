namespace KVKarco.ValidationAssistant.Exceptions;

/// <summary>
/// Represents exception type that can be thrown by validators during the validation run.
/// </summary>
public sealed class ValidationRunException : ValidationAssistantException
{
    internal ValidationRunException()
    {
    }

    internal ValidationRunException(string message) : base(message)
    {
    }

    internal ValidationRunException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

