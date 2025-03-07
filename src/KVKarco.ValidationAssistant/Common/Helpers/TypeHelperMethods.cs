namespace KVKarco.ValidationAssistant.Common.Helpers;

internal static class TypeHelperMethods
{
    public static bool IsNullable(this Type type) =>
        !type.IsValueType || Nullable.GetUnderlyingType(type) != null;

    public static bool IsNullableStruct(this Type type) =>
        type.IsValueType && Nullable.GetUnderlyingType(type) != null;

    public static bool IsNotNullable(this Type type) =>
        !type.IsNullable();

    public static bool IsReferenceType(this Type type) =>
        !type.IsValueType;

    public static Type MakeNullable(this Type type) =>
        type.IsNullable() ? type : typeof(Nullable<>).MakeGenericType(type);

    public static Type RemoveNullable(this Type type) =>
        type.IsNullableStruct() ? type.GenericTypeArguments[0] : type;
}
