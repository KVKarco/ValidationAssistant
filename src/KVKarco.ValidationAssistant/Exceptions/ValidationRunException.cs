using System.Diagnostics.CodeAnalysis;

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

    internal static void ThrowIfAsyncRuleIsCalledSynchronously(bool canRunSynchronously, ReadOnlySpan<char> ruleName)
    {
        if (!canRunSynchronously)
        {
            ThrowForSyncNull(ruleName);
        }
    }

    [DoesNotReturn]
    private static void ThrowForSyncNull(ReadOnlySpan<char> ruleName)
    {
        throw new ValidationRunException($"Cannot run validation synchronously for rule '{ruleName}' because it is an asynchronous rule. " +
            "Use the ValidateAsync method instead.");
    }
}

