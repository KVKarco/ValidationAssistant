using System.Text.Json;
using System.Text.Json.Serialization;

namespace KVKarco.ValidationAssistant.Internal.Utilities.TargetAssets;

/// <summary>
/// Represents a value that may be missing (due to a broken object path) or present (with potential null).
/// Used primarily for safe property resolution and validation scenarios.
/// </summary>
/// <typeparam name="TTarget">The type of the encapsulated value.</typeparam>
public readonly record struct Undefined<TTarget>
{
    private Undefined(TTarget value, string? missingMember)
    {
        Value = value;
        MissingMember = missingMember;
    }

    /// <summary>
    /// Internal static JSON serializer options for ToString() when value is a reference type.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.Preserve,
    };

    /// <summary>
    /// The actual resolved value. May be null if the property exists but its value is null.
    /// </summary>
    public readonly TTarget Value { get; }

    /// <summary>
    /// The name of the first missing member in the object path.
    /// Null if the value was successfully resolved.
    /// </summary>
    public readonly string? MissingMember { get; }

    /// <summary>
    /// Indicates whether the value was successfully resolved from the object path.
    /// </summary>
    public bool IsDefined => MissingMember is null;

    /// <summary>
    /// Indicates whether the resolved value is null (only meaningful if <see cref="IsDefined"/> is true).
    /// </summary>
    public bool IsNull => Value is null;

    /// <summary>
    /// Creates a new instance representing a broken path with a known missing member.
    /// </summary>
    /// <param name="missingValue">The name of the missing member in the object graph.</param>
    /// <returns>A new <see cref="Undefined{T}"/> representing an unresolved value.</returns>
    internal static Undefined<TTarget> WithNoValue(string missingValue) => new(default!, missingValue);

    /// <summary>
    /// Creates a new instance with a successfully resolved value.
    /// </summary>
    /// <param name="value">The resolved value.</param>
    /// <returns>A new <see cref="Undefined{T}"/> with the specified value.</returns>
    internal static Undefined<TTarget> WithValue(TTarget value) => new(value, null);

    /// <summary>
    /// Returns a string representation of the value or the state.
    /// </summary>
    /// <returns>
    /// A JSON string if <typeparamref name="TTarget"/> is a reference type and value is not null,
    /// or the result of <see cref="object.ToString"/> if it's a value type.
    /// Returns "Empty" if value is null or the path was broken.
    /// </returns>
    public override string ToString()
    {
        return !IsDefined || IsNull
            ? "Empty"
            : typeof(TTarget).IsValueType
            ? Value!.ToString()!
            : JsonSerializer.Serialize(Value!, _jsonOptions);
    }
}
