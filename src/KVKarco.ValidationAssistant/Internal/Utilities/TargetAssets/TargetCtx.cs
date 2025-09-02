namespace KVKarco.ValidationAssistant.Internal.Utilities.TargetAssets;

/// <summary>
/// Represents the abstract base class for a property context. This class encapsulates
/// the <see cref="TargetKey"/> which provides metadata about the property itself.
/// Derived generic classes will add the mechanism for extracting the property's value.
/// </summary>
internal abstract class TargetCtx
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TargetCtx"/> class.
    /// </summary>
    /// <param name="key">The <see cref="TargetKey"/> containing metadata about the property this context represents.</param>
    public TargetCtx(TargetKey key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets the <see cref="TargetKey"/> associated with this property context.
    /// This key provides information such as the property's <see cref="MemberInfo"/>,
    /// its full path, and whether it represents a collection.
    /// </summary>
    public TargetKey Key { get; }

    /// <summary>
    /// A static factory method to create a new, generic <see cref="TargetCtx{T, TProperty}"/> instance.
    /// This method is the recommended way to instantiate property contexts, providing both
    /// the property's metadata and the mechanism to resolve its value.
    /// </summary>
    /// <typeparam name="TSubject">The type of the main validation instance from which the property value is extracted.</typeparam>
    /// <typeparam name="TTarget">The type of the property whose value this context pertains to.</typeparam>
    /// <param name="meta">The <see cref="TargetKey"/> containing metadata for the property.</param>
    /// <param name="valueGetter">The <see cref="PropertyValueResolver{T, TProperty}"/> delegate used to extract the property's value from the main instance.</param>
    /// <returns>A new <see cref="TargetCtx{T, TProperty}"/> instance initialized with the provided metadata and value resolver.</returns>
    public static TargetCtx<TSubject, TTarget> Create<TSubject, TTarget>(
        TargetKey meta,
       Func<TSubject, Undefined<TTarget>> valueGetter)
    {
        return new TargetCtx<TSubject, TTarget>(meta, valueGetter);
    }
}

internal abstract class TargetCtx<TSubject> : TargetCtx
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TargetCtx{T}"/> class.
    /// </summary>
    /// <param name="key">The <see cref="TargetKey"/> containing metadata about the property.</param>
    public TargetCtx(TargetKey key) : base(key)
    {
    }
    /// <summary>
    /// Indicates whether the property context is undefined, meaning it does not have a valid value.
    /// This is typically used to check if the property was successfully resolved or if it is conceptually "undefined".
    /// </summary>
    public abstract (bool isUndefined, string? missingMember) Check(TSubject value);
}

/// <summary>
/// Represents a sealed, concrete generic implementation of a property context.
/// This class extends <see cref="TargetCtx"/> by adding a delegate for extracting
/// the specific property's value from the main validation instance.
/// </summary>
/// <typeparam name="TSubject">The type of the main validation instance.</typeparam>
/// <typeparam name="TTarget">The type of the property whose context is being managed.</typeparam>
internal sealed class TargetCtx<TSubject, TTarget> : TargetCtx<TSubject>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TargetCtx{T, TProperty}"/> class.
    /// </summary>
    /// <param name="key">The <see cref="TargetKey"/> containing metadata about the property.</param>
    /// <param name="valueGetter">The <see cref="PropertyValueResolver{T, TProperty}"/> delegate responsible for extracting the property's value.</param>
    public TargetCtx(TargetKey key, Func<TSubject, Undefined<TTarget>> valueGetter)
        : base(key) // Pass the PropertyKey to the base constructor.
    {
        ExtractValue = valueGetter;
    }

    /// <summary>
    /// Gets the delegate that can extract the value of the property from the main validation instance.
    /// This resolver returns the value wrapped in an <see cref="Undefined{TProperty}"/> to handle
    /// cases where the property might be null or conceptually "undefined".
    /// </summary>
    public Func<TSubject, Undefined<TTarget>> ExtractValue { get; }

    public override (bool isUndefined, string? missingMember) Check(TSubject value)
    {
        Undefined<TTarget> target = ExtractValue(value);
        return (!target.IsDefined, target.MissingMember);
    }
}
