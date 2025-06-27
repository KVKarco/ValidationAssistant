using System.Reflection;

namespace KVKarco.ValidationAssistant.Internal.PropertyValidation;

/// <summary>
/// Represents metadata about a member (property or field) of a validation instance.
/// This sealed class encapsulates information necessary to identify and describe
/// a specific property within the object graph being validated, including its
/// name, path, and whether it represents a collection.
/// </summary>
internal sealed class PropertyKey
{
    /// <summary>
    /// Represents an empty or default <see cref="PropertyKey"/> instance.
    /// This is typically used when no specific property is being referenced,
    /// such as for root-level validation or initialization.
    /// </summary>
    public static readonly PropertyKey Empty = new(null!, null!, false);

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyKey"/> class.
    /// This private constructor is used by the static factory method <see cref="Create"/>.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="MemberInfo"/> (e.g., <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>)
    /// representing the reflection metadata of the member. Can be <see langword="null"/> for <see cref="Empty"/>.</param>
    /// <param name="propertyPath">The full hierarchical path to the property within the object graph (e.g., "Address.Street").</param>
    /// <param name="isForCollection">A boolean indicating whether this property represents a collection type.</param>
    private PropertyKey(MemberInfo? propertyInfo, string propertyPath, bool isForCollection)
    {
        Info = propertyInfo;
        IsForCollection = isForCollection;
        PropertyPath = propertyPath;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PropertyKey"/> class.
    /// This is the recommended way to instantiate <see cref="PropertyKey"/> objects.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="MemberInfo"/> representing the reflection metadata of the property.</param>
    /// <param name="propertyPath">The full hierarchical path to the property (e.g., "User.Address.Street").</param>
    /// <param name="isForCollection">A boolean indicating whether this property represents a collection type.</param>
    /// <returns>A new <see cref="PropertyKey"/> instance.</returns>
    public static PropertyKey Create(MemberInfo propertyInfo, string propertyPath, bool isForCollection)
    {
        // Null-forgiving operators used assuming non-null inputs for Create based on usage context.
        return new PropertyKey(propertyInfo, propertyPath, isForCollection);
    }

    /// <summary>
    /// Gets the simple name of the property. If <see cref="Info"/> is <see langword="null"/> (e.g., for <see cref="Empty"/>),
    /// it defaults to the last segment of the <see cref="PropertyPath"/>.
    /// </summary>
    public ReadOnlySpan<char> PropertyName => Info is null ? PropertyPath.AsSpan() : Info.Name.AsSpan();

    /// <summary>
    /// Gets the <see cref="MemberInfo"/> (e.g., <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>)
    /// representing the reflection metadata of the member. This can be <see langword="null"/> for special cases
    /// like the <see cref="Empty"/> key.
    /// </summary>
    public MemberInfo? Info { get; }

    /// <summary>
    /// Gets a value indicating whether this property represents a collection type.
    /// This is important for rules that operate differently on collections (e.g., <c>ForEach</c> rules).
    /// </summary>
    public bool IsForCollection { get; }

    /// <summary>
    /// Gets the full hierarchical path to the property within the object graph.
    /// For a top-level property, it might just be the property's name. For nested properties,
    /// it would include parent property names (e.g., "Address.Street").
    /// </summary>
    public string PropertyPath { get; }
}
