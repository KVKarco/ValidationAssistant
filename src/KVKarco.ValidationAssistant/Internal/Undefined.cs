using System.Text.Json;
using System.Text.Json.Serialization;

namespace KVKarco.ValidationAssistant.Internal;

/// <summary>
/// Provides an abstract base for representing a value that might be undefined (missing) or null.
/// This class serves as a foundation for a more strongly-typed <see cref="Undefined{TProperty}"/>,
/// encapsulating the core logic for checking value presence and nullability.
/// </summary>
public abstract class Undefined
{
    /// <summary>
    /// Static <see cref="JsonSerializerOptions"/> used for serializing values in the <see cref="ToString"/> method
    /// of derived classes. Configured for pretty printing, ignoring nulls, and preserving references.
    /// </summary>
    private protected static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.Preserve,
    };

    /// <summary>
    /// Creates an empty or undefined instance of <see cref="Undefined{T}"/>.
    /// This indicates that no value is present.
    /// </summary>
    /// <typeparam name="T">The type of the value that would normally be contained.</typeparam>
    /// <returns>A new <see cref="Undefined{T}"/> instance representing a missing value.</returns>
    internal static Undefined<T> Empty<T>() => new();

    /// <summary>
    /// Creates a new <see cref="Undefined{T}"/> instance with the specified value and state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The actual value to encapsulate. Can be default if <paramref name="hasValue"/> is false or <paramref name="isNull"/> is true.</param>
    /// <param name="hasValue">A boolean indicating whether a value is actually present (not missing).</param>
    /// <param name="isNull">A boolean indicating whether the present value is null (only relevant if <paramref name="hasValue"/> is true).</param>
    /// <returns>A new <see cref="Undefined{T}"/> instance configured with the provided value and state.</returns>
    internal static Undefined<T> For<T>(T value, bool hasValue, bool isNull) => new(value, hasValue, isNull);

    /// <summary>
    /// Initializes a new instance of the <see cref="Undefined"/> class.
    /// </summary>
    /// <param name="hasValue">Indicates if a value is present.</param>
    /// <param name="isNull">Indicates if the present value is null.</param>
    private protected Undefined(bool hasValue, bool isNull)
    {
        HasValue = hasValue;
        IsNull = isNull;
    }

    /// <summary>
    /// Gets a value indicating whether the current value can be extracted or is present.
    /// Returns <see langword="true"/> if a value is set (even if it's null), <see langword="false"/> if the value is missing.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets a value indicating whether the current value, if <see cref="HasValue"/> is true, is null.
    /// Returns <see langword="true"/> if the value is null, <see langword="false"/> otherwise.
    /// </summary>
    public bool IsNull { get; }
}

/// <summary>
/// A simple, sealed type for representing a value that can be explicitly missing or null.
/// This is typically used in scenarios like safe property extraction with lambda expressions,
/// where a property's value might not exist on the source object, or might exist but be null.
/// </summary>
/// <typeparam name="TProperty">The type of the value that this instance encapsulates.</typeparam>
public sealed class Undefined<TProperty> : Undefined
{
    /// <summary>
    /// A private backing field indicating whether <typeparamref name="TProperty"/> is a value type.
    /// Used internally for efficient <see cref="ToString"/> serialization.
    /// </summary>
    private readonly bool _isValueType;

    /// <summary>
    /// Initializes a new instance of the <see cref="Undefined{TProperty}"/> class with a specified value and state.
    /// </summary>
    /// <param name="value">The actual value being encapsulated. Can be default if <paramref name="hasValue"/> is false or <paramref name="isNull"/> is true.</param>
    /// <param name="hasValue">Indicates whether a value is present.</param>
    /// <param name="isNull">Indicates whether the present value is null.</param>
    internal Undefined(TProperty value, bool hasValue, bool isNull)
        : base(hasValue, isNull)
    {
        _isValueType = typeof(TProperty).IsValueType;
        Value = value;
    }

    /// <summary>
    /// Initializes a new, empty (missing) instance of the <see cref="Undefined{TProperty}"/> class.
    /// This constructor is used when no value is available.
    /// </summary>
    internal Undefined() : this(default!, false, true)
    {
    }

    /// <summary>
    /// Gets the encapsulated member value. If <see cref="HasValue"/> is <see langword="false"/>,
    /// this will be the default value of <typeparamref name="TProperty"/>.
    /// If <see cref="IsNull"/> is <see langword="true"/>, this will be null (for reference types)
    /// or default (for value types).
    /// </summary>
    public TProperty Value { get; }

    /// <summary>
    /// Returns a string representation of the encapsulated value.
    /// If the value is missing (<see cref="IsNull"/> is <see langword="true"/>), returns "Empty".
    /// If it's a value type, it uses its default <see cref="object.ToString()"/>.
    /// For reference types, it serializes the value to a JSON string using internal options.
    /// </summary>
    /// <returns>A string representing the encapsulated value or "Empty" if missing.</returns>
    public override string ToString()
    {
        return IsNull ? "Empty" : _isValueType ? Value!.ToString()! : JsonSerializer.Serialize(Value!, _jsonOptions);
    }
}
