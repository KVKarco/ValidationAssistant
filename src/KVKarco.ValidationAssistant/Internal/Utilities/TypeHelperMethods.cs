namespace KVKarco.ValidationAssistant.Internal.Utilities;

/// <summary>
/// Provides a collection of static extension methods for <see cref="Type"/> objects,
/// simplifying common type-related checks and manipulations, particularly concerning nullability
/// and value/reference type distinctions. These helpers are internal and designed for use
/// within the ValidationAssistant framework.
/// </summary>
internal static class TypeHelperMethods
{
    /// <summary>
    /// Determines whether a given <see cref="Type"/> is nullable. A type is considered nullable
    /// if it's a reference type (e.g., <see cref="string"/>, <see cref="object"/>, custom classes)
    /// or a nullable value type (e.g., <c>int?</c>, <c>DateTime?</c>).
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check for nullability.</param>
    /// <returns><see langword="true"/> if the type is nullable; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullable(this Type type) =>
        !type.IsValueType || Nullable.GetUnderlyingType(type) != null;

    /// <summary>
    /// Determines whether a given <see cref="Type"/> is a nullable struct (i.e., a <see cref="Nullable{T}"/>).
    /// Examples include <c>int?</c>, <c>bool?</c>, or <c>DateTime?</c>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <returns><see langword="true"/> if the type is a nullable struct; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullableStruct(this Type type) =>
        type.IsValueType && Nullable.GetUnderlyingType(type) != null;

    /// <summary>
    /// Determines whether a given <see cref="Type"/> is *not* nullable. This means it must be a non-nullable
    /// value type (e.g., <c>int</c>, <c>bool</c>, <c>DateTime</c>).
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check for non-nullability.</param>
    /// <returns><see langword="true"/> if the type is a non-nullable value type; otherwise, <see langword="false"/>.</returns>
    public static bool IsNotNullable(this Type type) =>
        !type.IsNullable();

    /// <summary>
    /// Determines whether a given <see cref="Type"/> is a reference type.
    /// Examples include <see cref="string"/>, <see cref="object"/>, arrays, and custom classes.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <returns><see langword="true"/> if the type is a reference type; otherwise, <see langword="false"/>.</returns>
    public static bool IsReferenceType(this Type type) =>
        !type.IsValueType;

    /// <summary>
    /// Converts a given <see cref="Type"/> to its nullable equivalent if it's a non-nullable value type.
    /// If the type is already nullable (either a reference type or a nullable value type), it returns the original type.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to make nullable.</param>
    /// <returns>
    /// The nullable equivalent of the input type (e.g., <c>int</c> becomes <c>int?</c>),
    /// or the original type if it was already nullable or a reference type.
    /// </returns>
    public static Type MakeNullable(this Type type) =>
        type.IsNullable() ? type : typeof(Nullable<>).MakeGenericType(type);

    /// <summary>
    /// Removes the nullable wrapper from a nullable value type (e.g., <c>int?</c> becomes <c>int</c>).
    /// If the type is not a nullable struct (i.e., it's a non-nullable value type or a reference type),
    /// it returns the original type unchanged.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> from which to remove the nullable wrapper.</param>
    /// <returns>
    /// The underlying type if it was a nullable struct; otherwise, the original type.
    /// </returns>
    public static Type RemoveNullable(this Type type) =>
        type.IsNullableStruct() ? type.GenericTypeArguments[0] : type;
}
