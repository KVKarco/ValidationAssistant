using CommunityToolkit.HighPerformance.Buffers;
using KVKarco.ValidationAssistant.Abstractions.MessageTemplate;
using System.Buffers;

namespace KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;

/// <summary>
/// A high-performance, low-allocation message formatter that uses a pre-compiled template and a pooled character buffer.
/// </summary>
/// <remarks>
/// This class is designed for reuse and is typically managed by a factory or object pool to minimize garbage collection.
/// It builds the final message string segment by segment to avoid intermediate string allocations.
/// </remarks>
internal class MessageFormatter : IMessageResolver, IDisposable
{
    private CompiledTemplate _template;
    private char[] _buffer;
    private int _pos;
    private int _currentSegmentIndex;
    private const int InitialBufferSize = 256;
    private int _placeholdersReplaced;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageFormatter"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor rents an initial character buffer from a shared pool to be used for message building.
    /// </remarks>
    public MessageFormatter()
    {
        _template = null!; // Placeholder until a template is set
        _buffer = ArrayPool<char>.Shared.Rent(InitialBufferSize);
    }

    /// <summary>
    /// Prepares the formatter for a new message by setting the template and resetting its internal state.
    /// </summary>
    /// <param name="template">The pre-compiled message template to use for formatting.</param>
    /// <returns>The current <see cref="MessageFormatter"/> instance, enabling a fluent API.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="template"/> is null.</exception>
    public MessageFormatter ForNew(CompiledTemplate template)
    {
        _template = template ?? throw new ArgumentNullException(nameof(template));
        _pos = 0; // The current position in the buffer.
        _currentSegmentIndex = 0; // The index of the current segment in the template.
        _placeholdersReplaced = 0; // The count of replaced placeholders.
        return this;
    }

    /// <summary>
    /// Gets a value indicating whether the internal buffer has been returned to the pool.
    /// </summary>
    public bool IsBufferReturned => _buffer is null;

    /// <summary>
    /// Gets the final formatted message string.
    /// </summary>
    /// <param name="toPoolMessage">If <c>true</c>, the method attempts to retrieve the string from a shared string pool to prevent new allocations. Defaults to <c>false</c>.</param>
    /// <returns>The fully resolved message string.</returns>
    /// <exception cref="ValidationCompositionException">Thrown if not all placeholders were replaced before calling this method.</exception>
    public string GetMessage(bool toPoolMessage = false)
    {
        // First, check that all placeholders have been replaced to ensure a valid message can be created.
        Ensure.TemplateCanBeFormatted(_template, _placeholdersReplaced);

        // Process any remaining static text segments that follow the last replaced placeholder.
        while (_currentSegmentIndex < _template.Segments.Length)
        {
            if (_template.Segments[_currentSegmentIndex].Text is not null)
            {
                // If the segment is text, append it to the internal buffer.
                ResizeAndAppend(_template.Segments[_currentSegmentIndex].Text.AsSpan());
            }
            _currentSegmentIndex++;
        }

        // Return the final message. The `AsSpan` avoids creating a new string until the very end.
        // `StringPool` is used for caching common strings and preventing repeated allocations.
        return toPoolMessage ? StringPool.Shared.GetOrAdd(_buffer.AsSpan(0, _pos)) : new string(_buffer.AsSpan(0, _pos));
    }


    // --- Replace Overloads ---

    /// <summary>
    /// Replaces a specified placeholder with a value that implements <see cref="ISpanFormattable"/>, in an allocation-free manner.
    /// </summary>
    /// <typeparam name="T">The type of the value to format.</typeparam>
    /// <param name="placeholder">The placeholder string to replace (e.g., "{PropertyName}").</param>
    /// <param name="value">The value to use for replacement.</param>
    /// <returns>The current <see cref="IMessageResolver"/> instance.</returns>
    /// <exception cref="ValidationCompositionException">Thrown if the placeholder is not found in the template.</exception>
    public IMessageResolver Replace<T>(string placeholder, T value) where T : ISpanFormattable
    {
        // Verify the placeholder exists in the template.
        Ensure.TemplatePlaceholderCanBeReplaced(_template, placeholder);

        // Iterate through the segments, appending static text and then replacing the target placeholder.
        while (_currentSegmentIndex < _template.Segments.Length)
        {
            if (_template.Segments[_currentSegmentIndex].Text is not null)
            {
                ResizeAndAppend(_template.Segments[_currentSegmentIndex].Text.AsSpan());
                _currentSegmentIndex++;
            }
            else if (_template.Segments[_currentSegmentIndex].Placeholder is not null &&
                _template.Segments[_currentSegmentIndex]!.Placeholder!.Equals(placeholder, StringComparison.Ordinal))
            {
                // Found the placeholder, format the value directly into the buffer.
                int charsWritten = FormatValue(value!);
                _pos += charsWritten;
                _currentSegmentIndex++;
                break; // Stop after replacing the placeholder. The next Replace call will continue from here.
            }
            else
            {
                _currentSegmentIndex++;
            }
        }
        _placeholdersReplaced++;
        return this;
    }

    /// <summary>
    /// Replaces a specified placeholder with an object's string representation.
    /// </summary>
    /// <param name="placeholder">The placeholder string to replace.</param>
    /// <param name="value">The object whose string representation will be used.</param>
    /// <returns>The current <see cref="IMessageResolver"/> instance.</returns>
    /// <exception cref="ValidationCompositionException">Thrown if the placeholder is not found in the template.</exception>
    public IMessageResolver Replace(string placeholder, object? value)
    {
        Ensure.TemplatePlaceholderCanBeReplaced(_template, placeholder);

        while (_currentSegmentIndex < _template.Segments.Length)
        {
            if (_template.Segments[_currentSegmentIndex].Text is not null)
            {
                ResizeAndAppend(_template.Segments[_currentSegmentIndex].Text.AsSpan());
                _currentSegmentIndex++;
            }
            else if (_template.Segments[_currentSegmentIndex].Placeholder is not null &&
                _template.Segments[_currentSegmentIndex]!.Placeholder!.Equals(placeholder, StringComparison.Ordinal))
            {
                // This is a fallback for general objects, which may result in an allocation via `ToString()`.
                int charsWritten = FormatValue(value!);
                _pos += charsWritten;
                _currentSegmentIndex++;
                break;
            }
            else
            {
                _currentSegmentIndex++;
            }
        }
        _placeholdersReplaced++;
        return this;
    }

    /// <summary>
    /// Replaces a specified placeholder with a string value.
    /// </summary>
    /// <param name="placeholder">The placeholder string to replace.</param>
    /// <param name="value">The string to use for replacement.</param>
    /// <returns>The current <see cref="IMessageResolver"/> instance.</returns>
    /// <exception cref="ValidationCompositionException">Thrown if the placeholder is not found in the template.</exception>
    public IMessageResolver Replace(string placeholder, string value)
    {
        Ensure.TemplatePlaceholderCanBeReplaced(_template, placeholder);

        while (_currentSegmentIndex < _template.Segments.Length)
        {
            if (_template.Segments[_currentSegmentIndex].Text is not null)
            {
                ResizeAndAppend(_template.Segments[_currentSegmentIndex].Text.AsSpan());
                _currentSegmentIndex++;
            }
            else if (_template.Segments[_currentSegmentIndex].Placeholder is not null &&
                _template.Segments[_currentSegmentIndex]!.Placeholder!.Equals(placeholder, StringComparison.Ordinal))
            {
                // Format the string value into the buffer. This is optimized to be allocation-free.
                int charsWritten = FormatValue(value!);
                _pos += charsWritten;
                _currentSegmentIndex++;
                break;
            }
            else
            {
                _currentSegmentIndex++;
            }
        }
        _placeholdersReplaced++;
        return this;
    }


    // --- FormatValue Overloads  ---

    /// <summary>
    /// Formats a value that implements <see cref="ISpanFormattable"/> directly into the buffer.
    /// This is the most efficient and allocation-free method for most primitive types.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to format.</param>
    /// <returns>The number of characters written to the buffer.</returns>
    private int FormatValue<T>(T value) where T : ISpanFormattable
    {
        Span<char> currentSpan = _buffer.AsSpan(_pos);
        int charsWritten = 0;

        // Use a loop to handle cases where the buffer is too small, resizing as needed.
        while (!value.TryFormat(currentSpan, out charsWritten, default, _template.Culture))
        {
            ResizeBuffer(_pos + charsWritten);
            currentSpan = _buffer.AsSpan(_pos);
        }

        return charsWritten;
    }

    /// <summary>
    /// Formats a string value directly into the buffer. This is also allocation-free.
    /// </summary>
    /// <param name="value">The string to format.</param>
    /// <returns>The number of characters written.</returns>
    private int FormatValue(string value)
    {
        Span<char> currentSpan = _buffer.AsSpan(_pos);
        if (currentSpan.Length < value.Length)
        {
            // If the buffer is too small, resize it.
            ResizeBuffer(_pos + value.Length);
            currentSpan = _buffer.AsSpan(_pos);
        }
        // Copy the string's content to the buffer without creating an intermediate string.
        value.AsSpan().CopyTo(currentSpan);

        return value.Length;
    }

    /// <summary>
    /// Formats an object's string representation into the buffer as a fallback.
    /// Note: This method may cause an allocation if the `ToString()` method is called on a value type.
    /// </summary>
    /// <param name="value">The object to format.</param>
    /// <returns>The number of characters written.</returns>
    private int FormatValue(object value)
    {
        // For general objects, we convert to a string which might allocate.
        string toStringValue = value.ToString() ?? "Empty";
        Span<char> currentSpan = _buffer.AsSpan(_pos);

        while (currentSpan.Length < toStringValue.Length)
        {
            ResizeBuffer(_pos + toStringValue.Length);
            currentSpan = _buffer.AsSpan(_pos);
        }
        toStringValue.AsSpan().CopyTo(currentSpan);

        return toStringValue.Length;
    }


    // Internal helper methods for resizing and formatting

    /// <summary>
    /// Appends a span of characters to the internal buffer, resizing if necessary.
    /// </summary>
    /// <param name="source">The character span to append.</param>
    private void ResizeAndAppend(ReadOnlySpan<char> source)
    {
        if (_buffer.Length - _pos < source.Length)
        {
            // Not enough space, so increase buffer size.
            ResizeBuffer(_pos + source.Length);
        }
        // Copy the source span to the correct position in the buffer.
        source.CopyTo(_buffer.AsSpan(_pos));
        _pos += source.Length;
    }

    /// <summary>
    /// Increases the size of the internal character buffer by renting a larger one from the pool.
    /// </summary>
    /// <param name="minLength">The minimum required length for the new buffer.</param>
    private void ResizeBuffer(int minLength)
    {
        // Determine the new size, doubling the current size or using the minimum required length.
        int newSize = Math.Max(_buffer.Length * 2, minLength);
        char[] newBuffer = ArrayPool<char>.Shared.Rent(newSize);
        // Copy data from the old buffer to the new one.
        _buffer.CopyTo(newBuffer, 0);
        // Return the old buffer to the pool for reuse.
        ArrayPool<char>.Shared.Return(_buffer);
        _buffer = newBuffer;
    }


    // Dispose/Refresh
    /// <summary>
    /// Resets the formatter's state and returns the internal buffer to the shared pool.
    /// </summary>
    public void Clear()
    {
        _template = null!;
        _pos = 0;
        _currentSegmentIndex = 0;
        _placeholdersReplaced = 0;

        // Return the buffer to the pool if it hasn't been already.
        if (_buffer != null)
        {
            ArrayPool<char>.Shared.Return(_buffer);
            _buffer = null!;
        }
    }

    /// <summary>
    /// Disposes the object by returning its internal buffer to the shared pool.
    /// </summary>
    public void Dispose()
    {
        Clear();
    }
}