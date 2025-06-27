namespace KVKarco.ValidationAssistant.Internal.PropertyValidation;

/// <summary>
/// Represents the abstract base class for a property context. This class encapsulates
/// the <see cref="PropertyKey"/> which provides metadata about the property itself.
/// Derived generic classes will add the mechanism for extracting the property's value.
/// </summary>
internal abstract class PropertyCtx
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyCtx"/> class.
    /// </summary>
    /// <param name="key">The <see cref="PropertyKey"/> containing metadata about the property this context represents.</param>
    public PropertyCtx(PropertyKey key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets the <see cref="PropertyKey"/> associated with this property context.
    /// This key provides information such as the property's <see cref="MemberInfo"/>,
    /// its full path, and whether it represents a collection.
    /// </summary>
    public PropertyKey Key { get; }

    /// <summary>
    /// A static factory method to create a new, generic <see cref="PropertyCtx{T, TProperty}"/> instance.
    /// This method is the recommended way to instantiate property contexts, providing both
    /// the property's metadata and the mechanism to resolve its value.
    /// </summary>
    /// <typeparam name="T">The type of the main validation instance from which the property value is extracted.</typeparam>
    /// <typeparam name="TProperty">The type of the property whose value this context pertains to.</typeparam>
    /// <param name="meta">The <see cref="PropertyKey"/> containing metadata for the property.</param>
    /// <param name="valueGetter">The <see cref="PropertyValueResolver{T, TProperty}"/> delegate used to extract the property's value from the main instance.</param>
    /// <returns>A new <see cref="PropertyCtx{T, TProperty}"/> instance initialized with the provided metadata and value resolver.</returns>
    public static PropertyCtx<T, TProperty> Create<T, TProperty>(
        PropertyKey meta,
        PropertyValueResolver<T, TProperty> valueGetter)
    {
        return new PropertyCtx<T, TProperty>(meta, valueGetter);
    }
}

/// <summary>
/// Represents a sealed, concrete generic implementation of a property context.
/// This class extends <see cref="PropertyCtx"/> by adding a delegate for extracting
/// the specific property's value from the main validation instance.
/// </summary>
/// <typeparam name="T">The type of the main validation instance.</typeparam>
/// <typeparam name="TProperty">The type of the property whose context is being managed.</typeparam>
internal sealed class PropertyCtx<T, TProperty> : PropertyCtx
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyCtx{T, TProperty}"/> class.
    /// </summary>
    /// <param name="key">The <see cref="PropertyKey"/> containing metadata about the property.</param>
    /// <param name="valueGetter">The <see cref="PropertyValueResolver{T, TProperty}"/> delegate responsible for extracting the property's value.</param>
    public PropertyCtx(PropertyKey key, PropertyValueResolver<T, TProperty> valueGetter)
        : base(key) // Pass the PropertyKey to the base constructor.
    {
        ExtractValue = valueGetter;
    }

    /// <summary>
    /// Gets the delegate that can extract the value of the property from the main validation instance.
    /// This resolver returns the value wrapped in an <see cref="Undefined{TProperty}"/> to handle
    /// cases where the property might be null or conceptually "undefined".
    /// </summary>
    public PropertyValueResolver<T, TProperty> ExtractValue { get; }
}
