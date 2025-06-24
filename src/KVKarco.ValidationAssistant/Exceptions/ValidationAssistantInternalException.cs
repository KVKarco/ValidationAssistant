namespace KVKarco.ValidationAssistant.Exceptions;

/// <summary>
/// Represents exception type that can be thrown by validators when internal logic is wrong,
/// contact support when you run into this type of exception.
/// </summary>
public sealed class ValidationAssistantInternalException : ValidationAssistantException
{
    internal ValidationAssistantInternalException()
    {
    }

    internal ValidationAssistantInternalException(string message) : base(message)
    {
    }

    internal ValidationAssistantInternalException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

