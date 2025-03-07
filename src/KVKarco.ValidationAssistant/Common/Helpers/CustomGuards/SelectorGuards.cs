using Ardalis.GuardClauses;
using KVKarco.ValidationAssistant.Common.Exceptions;
using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.Common.Helpers.CustomGuards;

internal static class SelectorGuards
{
    public static void NullSelector(
        this IGuardClause guardClause,
        LambdaExpression propertySelector)
    {
        if (propertySelector is null)
        {
            throw new InvalidSelectorException("Null propertySelector was supplied.");
        }
    }

    public static MemberExpression EntityOrValueSelection(this IGuardClause guardClause, LambdaExpression expression)
    {
        if (expression.Body is ParameterExpression)
        {
            throw new InvalidSelectorException("Cant select main entity for validation.");
        }

        if (expression.Body is not MemberExpression memberExpression)
        {
            throw new InvalidSelectorException("Only selecting properties or fields is allowed in propertySelector.");
        }

        if (memberExpression.Member.Name == "Value" && memberExpression.Expression is ParameterExpression p && p.IsForNullableStruct())
        {
            throw new InvalidSelectorException("Cant select main entity for validation.");
        }

        if (memberExpression.Member.Name == "Value" && memberExpression.Expression is MemberExpression mb && mb.IsForNullableStruct())
        {
            throw new InvalidSelectorException("Don`t select Value from Nullable<>, select the property or field it self.");
        }

        return memberExpression;
    }

    public static MemberExpression? NotMemberOrPropertyExpression(this IGuardClause clause, MemberExpression memberExpression)
    {
        if (memberExpression.Expression is ParameterExpression)
        {
            return null;
        }

        if (memberExpression.Expression is not MemberExpression mb)
        {
            throw new InvalidSelectorException("Only selecting properties or fields is allowed in propertySelector.");
        }

        return mb;
    }

    public static PropertyKey InvalidPropertySelector<TEntity, TProperty>(
        this IGuardClause guardClause,
        Expression<Func<TEntity, TProperty>> propertySelector)
    {
        Guard.Against.NullSelector(propertySelector);

        if (propertySelector.Parameters[0].Name == "x")
        {
            return propertySelector.GetOrAddKey();
        }

        return propertySelector.Rewrite().GetOrAddKey();
    }
}
