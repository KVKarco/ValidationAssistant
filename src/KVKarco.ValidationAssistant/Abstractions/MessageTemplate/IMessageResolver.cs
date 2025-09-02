namespace KVKarco.ValidationAssistant.Abstractions.MessageTemplate;

/// <summary>
/// Provides a mechanism for resolving and formatting messages with placeholders.
/// </summary>
public interface IMessageResolver
{
    /// <summary>
    /// Replaces a specified placeholder with a value that can be formatted as a span.
    /// </summary>
    /// <typeparam name="T">The type of the value to replace the placeholder with. Must implement <see cref="ISpanFormattable"/>.</typeparam>
    /// <param name="placeholder">The placeholder string to search for, e.g., "{PropertyName}".</param>
    /// <param name="value">The value to replace the placeholder with.</param>
    /// <returns>A new <see cref="IMessageResolver"/> instance with the placeholder replaced.</returns>
    /// <exception cref="Exceptions.ValidationCompositionException">Thrown if the specified placeholder is not present in the message template.</exception>
    IMessageResolver Replace<T>(string placeholder, T value) where T : ISpanFormattable;

    /// <summary>
    /// Replaces a specified placeholder with a string value.
    /// </summary>
    /// <param name="placeholder">The placeholder string to search for, e.g., "{PropertyName}".</param>
    /// <param name="value">The string value to replace the placeholder with.</param>
    /// <returns>A new <see cref="IMessageResolver"/> instance with the placeholder replaced.</returns>
    /// <exception cref="Exceptions.ValidationCompositionException">Thrown if the specified placeholder is not present in the message template.</exception>
    IMessageResolver Replace(string placeholder, string value);

    /// <summary>
    /// Replaces a specified placeholder with an object's string representation.
    /// </summary>
    /// <param name="placeholder">The placeholder string to search for, e.g., "{PropertyName}".</param>
    /// <param name="value">The object whose string representation will replace the placeholder.</param>
    /// <returns>A new <see cref="IMessageResolver"/> instance with the placeholder replaced.</returns>
    /// <exception cref="Exceptions.ValidationCompositionException">Thrown if the specified placeholder is not present in the message template.</exception>
    IMessageResolver Replace(string placeholder, object? value);

    /// <summary>
    /// Gets the final resolved message string.
    /// </summary>
    /// <param name="toPoolMessage">A boolean value indicating whether the message should be returned to a pool (if applicable) after retrieval. Defaults to false.</param>
    /// <returns>The fully resolved message string with all placeholders replaced.</returns>
    /// <exception cref="Exceptions.ValidationCompositionException">Thrown if not all placeholders in the message template have been replaced.</exception>
    string GetMessage(bool toPoolMessage = false);
}
