using Ardalis.GuardClauses;
using KVKarco.ValidationAssistant.Common.Helpers.CustomGuards;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace KVKarco.ValidationAssistant.Common.Helpers;

internal static class ExpressionFactory
{
    private static readonly ConstantExpression _forNull = Expression.Constant(null);

    public static PropertyKey ExtractPropertyKey(MemberExpression mainMemberExpression)
    {
        MemberInfo memberInfo = mainMemberExpression.Member;

        if (mainMemberExpression.Expression is ParameterExpression)
        {
            return new PropertyKey(memberInfo, null);
        }

        MemberExpression? nextMemberExpression = Guard.Against.NotMemberOrPropertyExpression(mainMemberExpression);

        StringBuilder sb = new();

        while (nextMemberExpression is not null)
        {
            if (nextMemberExpression.Member.Name == "Value")
            {
                if (nextMemberExpression.Expression is ParameterExpression p && p.IsForNotNullableStruct())
                {
                    nextMemberExpression = nextMemberExpression.Expression as MemberExpression;
                    continue;
                }

                if (nextMemberExpression.Expression is MemberExpression es && es.IsForNullableStruct())
                {
                    nextMemberExpression = es;
                    continue;
                }
            }

            if (sb.Length == 0)
            {
                sb.Insert(0, nextMemberExpression.Member.Name);
            }
            else
            {
                sb.Insert(0, '.');
                sb.Insert(0, nextMemberExpression.Member.Name);
            }

            nextMemberExpression = Guard.Against.NotMemberOrPropertyExpression(nextMemberExpression);
        }

        return new PropertyKey(memberInfo, sb.ToString());
    }
    internal static Predicate<TEntity> CreateValueNaNEvaluator<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
    {
        ParameterExpression parameterExpression = propertySelector.Parameters[0];

        List<MemberExpression> internalMembers = [];

        MemberExpression? member = propertySelector.Body as MemberExpression;

        while (member is not null)
        {
            internalMembers.Add(member);

            if (member.Expression is MemberExpression newMember)
            {
                member = newMember;
            }
            else
            {
                member = null;
            }
        }

        internalMembers.RemoveAt(0);
        internalMembers.Reverse();

        if (internalMembers.Count == 0)
        {
            return Expression.Lambda<Predicate<TEntity>>(Expression.Constant(true), parameterExpression).Compile();
        }

        Expression test = Expression.NotEqual(CreateNullPropagationBody(parameterExpression, internalMembers, 0), _forNull);
        Expression ifTrue = Expression.Constant(true);
        Expression ifFalse = Expression.Constant(false);
        Expression condition = Expression.Condition(test, ifTrue, ifFalse);

        return Expression.Lambda<Predicate<TEntity>>(condition, parameterExpression).Compile();
    }

    private static Expression CreateNullPropagationBody(Expression expression, List<MemberExpression> members, int toContinueFrom)
    {
        if (members.Count == toContinueFrom)
        {
            expression = Expression.PropertyOrField(expression, members[toContinueFrom].Member.Name);
            return expression.AttachNullConverter();
        }

        while (members[toContinueFrom].IsForNotNullableStruct())
        {
            expression = Expression.PropertyOrField(expression, members[toContinueFrom].Member.Name);

            toContinueFrom++;

            if (toContinueFrom < members.Count)
            {
                continue;
            }
            else
            {
                return expression.AttachNullConverter();
            }
        }

        expression = Expression.PropertyOrField(expression, members[toContinueFrom].Member.Name);
        toContinueFrom++;

        if (toContinueFrom >= members.Count)
        {
            return expression;
        }

        ParameterExpression caller = Expression.Variable(expression.Type, members[toContinueFrom - 1].Member.Name);
        BinaryExpression assign = Expression.Assign(caller, expression);

        Expression access = CreateNullPropagationBody(caller, members, toContinueFrom);

        var test = Expression.Equal(caller, _forNull);
        var ifTrue = Expression.Constant(null, access.Type);
        var ifFalse = access;

        var ternary = Expression.Condition(test, ifTrue, ifFalse);

        return Expression.Block(type: access.Type, variables: [caller], expressions: [assign, ternary]);
    }
}
