using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.Internal.Utilities;

/// <summary>
/// Provides a collection of static extension methods for <see cref="Expression"/> objects,
/// facilitating common checks and transformations related to type nullability within expression trees.
/// These helpers are designed to assist in dynamically building and manipulating expressions
/// for validation logic.
/// </summary>
internal static class ExpressionHelperMethods
{
    /// <summary>
    /// Determines whether the <see cref="Expression.Type"/> of the given expression is nullable.
    /// This is an extension method leveraging <see cref="TypeHelperMethods.IsNullable(Type)"/>.
    /// </summary>
    /// <param name="ex">The <see cref="Expression"/> to check.</param>
    /// <returns><see langword="true"/> if the expression's type is nullable; otherwise, <see langword="false"/>.</returns>
    public static bool IsForNullableType(this Expression ex) => ex.Type.IsNullable();

    /// <summary>
    /// Determines whether the <see cref="Expression.Type"/> of the given expression is a non-nullable value type.
    /// This is an extension method leveraging <see cref="TypeHelperMethods.IsNotNullable(Type)"/>.
    /// </summary>
    /// <param name="ex">The <see cref="Expression"/> to check.</param>
    /// <returns><see langword="true"/> if the expression's type is a non-nullable value type; otherwise, <see langword="false"/>.</returns>
    public static bool IsForNotNullableStruct(this Expression ex) => ex.Type.IsNotNullable();

    /// <summary>
    /// Determines whether the <see cref="Expression.Type"/> of the given expression is a nullable struct (<see cref="Nullable{T}"/>).
    /// This is an extension method leveraging <see cref="TypeHelperMethods.IsNullableStruct(Type)"/>.
    /// </summary>
    /// <param name="ex">The <see cref="Expression"/> to check.</param>
    /// <returns><see langword="true"/> if the expression's type is a nullable struct; otherwise, <see langword="false"/>.</returns>
    public static bool IsForNullableStruct(this Expression ex) => ex.Type.IsNullableStruct();

    /// <summary>
    /// Attaches a conversion to a nullable type to the given expression if it's not already nullable.
    /// If the expression's type is already nullable (reference type or Nullable&lt;T&gt;), the original expression is returned.
    /// Otherwise, an <see cref="Expression.Convert(Expression, Type)"/> expression is created to convert it to its nullable equivalent.
    /// </summary>
    /// <param name="ex">The <see cref="Expression"/> to modify.</param>
    /// <returns>An <see cref="Expression"/> that evaluates to a nullable type.</returns>
    public static Expression AttachNullConverter(this Expression ex) =>
        ex.Type.IsNullable() ? ex : Expression.Convert(ex, ex.Type.MakeNullable());

    /// <summary>
    /// Attaches a conversion to the underlying non-nullable type if the expression's type is a nullable struct.
    /// If the expression's type is not a nullable struct (e.g., it's already a non-nullable value type or a reference type),
    /// the original expression is returned. Otherwise, an <see cref="Expression.Convert(Expression, Type)"/> expression
    /// is created to convert it to its underlying non-nullable type.
    /// </summary>
    /// <param name="ex">The <see cref="Expression"/> to modify.</param>
    /// <returns>An <see cref="Expression"/> that evaluates to a non-nullable type (if applicable, otherwise the original expression's type).</returns>
    public static Expression AttachNotNullConverter(Expression ex) =>
        ex.Type.IsNullableStruct() ? Expression.Convert(ex, ex.Type.GenericTypeArguments[0]) : ex;
}
