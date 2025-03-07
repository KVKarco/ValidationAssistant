using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.Common.Helpers;

internal static class ExpressionHelperMethods
{
    public static bool IsForNullableType(this Expression ex) => ex.Type.IsNullable();

    public static bool IsForNotNullableStruct(this Expression ex) => ex.Type.IsNotNullable();

    public static bool IsForNullableStruct(this Expression ex) => ex.Type.IsNullableStruct();

    public static Expression AttachNullConverter(this Expression ex) =>
        ex.Type.IsNullable() ? ex : Expression.Convert(ex, ex.Type.MakeNullable());
    public static Expression AttachNotNullConverter(Expression ex) =>
        ex.Type.IsNullableStruct() ? Expression.Convert(ex, ex.Type.GenericTypeArguments[0]) : ex;
}
