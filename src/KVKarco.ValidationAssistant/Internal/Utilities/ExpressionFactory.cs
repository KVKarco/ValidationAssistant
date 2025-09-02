using KVKarco.ValidationAssistant.Internal.Utilities.TargetAssets;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace KVKarco.ValidationAssistant.Internal.Utilities;

internal static class ExpressionFactory
{
    private const BindingFlags _bindingFlags = BindingFlags.Static | BindingFlags.Public;

    public static TargetCtx<T, TProperty> CreateCtx<T, TProperty>(
        Expression<Func<T, TProperty>> expr,
        bool removeStartName = false,
        bool isForCollection = false)
    {
        var parameter = expr.Parameters[0];

        // Case: x => x
        if (expr.Body == parameter)
        {
            var lambda = Expression.Lambda<Func<T, Undefined<TProperty>>>(
                Expression.Call(GetWithValueMethod<TProperty>(), parameter),
                parameter
            );

            return TargetCtx.Create(TargetKey.Empty, lambda.Compile());
        }

        // Traverse member chain
        var memberChain = new List<MemberExpression>();
        var sb = new StringBuilder();
        var memberExpr = expr.Body as MemberExpression;
        var finalMember = memberExpr!.Member;

        while (memberExpr is not null)
        {
            memberChain.Insert(0, memberExpr);

            if (memberExpr.Member.Name != "Value" || memberChain[0].IsForNullableType())
            {
                if (sb.Length > 0) sb.Insert(0, '.');
                sb.Insert(0, memberExpr.Member.Name);
            }

            memberExpr = memberExpr.Expression as MemberExpression;
        }

        // If removeStartName is true, remove the first member's name from the generated path.
        if (removeStartName)
        {
            // Ensure there's a name to remove and it matches the expected length.
            if (sb.Length >= memberChain[0].Member.Name.Length)
            {
                sb.Remove(0, memberChain[0].Member.Name.Length);
                // If the path started with "Name.SubName" and "Name" is removed, it becomes ".SubName". Remove the leading dot.
                if (sb.Length > 0 && sb[0] == '.')
                {
                    sb.Remove(0, 1);
                }
            }
        }

        var instanceVar = Expression.Variable(typeof(T), "instance");
        var vars = new List<ParameterExpression> { instanceVar };
        var body = new List<Expression>();
        var returnLabel = Expression.Label(typeof(Undefined<TProperty>));

        // Assign instance
        body.Add(Expression.Assign(instanceVar, parameter));

        // Null check for instance
        body.Add(Expression.IfThen(
            Expression.Equal(instanceVar, Expression.Constant(null, typeof(T))),
            Expression.Return(returnLabel, Expression.Call(GetWithNoValueMethod<TProperty>(), Expression.Constant("MainInstance")))
        ));

        Expression current = instanceVar;

        for (int i = 0; i < memberChain.Count - 1; i++)
        {
            var member = memberChain[i].Member;
            var access = Expression.MakeMemberAccess(current, member);

            if (!IsNullable(access.Type))
            {
                current = access;
                continue;
            }

            var tempVar = Expression.Variable(access.Type, $"var{i}");
            vars.Add(tempVar);
            body.Add(Expression.Assign(tempVar, access));

            body.Add(Expression.IfThen(
                Expression.Equal(tempVar, Expression.Constant(null, access.Type)),
                Expression.Return(returnLabel,
                    Expression.Call(GetWithNoValueMethod<TProperty>(), Expression.Constant(member.Name)))
            ));

            current = tempVar;
        }

        // Final access
        var finalAccess = Expression.MakeMemberAccess(current, memberChain.Last().Member);

        // Return with value
        body.Add(Expression.Return(returnLabel,
            Expression.Call(GetWithValueMethod<TProperty>(), finalAccess)));

        body.Add(Expression.Label(returnLabel, Expression.Default(typeof(Undefined<TProperty>))));

        var block = Expression.Block(typeof(Undefined<TProperty>), vars, body);

        var lambdaExpr = Expression.Lambda<Func<T, Undefined<TProperty>>>(block, parameter);

        return TargetCtx.Create(TargetKey.Create(finalMember, sb.ToString(), isForCollection), lambdaExpr.Compile());
    }

    private static bool IsNullable(Type type) =>
        !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

    private static MethodInfo GetWithNoValueMethod<TProperty>() =>
        typeof(Undefined<TProperty>).GetMethod(nameof(Undefined<TProperty>.WithNoValue), _bindingFlags)!;

    private static MethodInfo GetWithValueMethod<TProperty>() =>
        typeof(Undefined<TProperty>).GetMethod(nameof(Undefined<TProperty>.WithValue), _bindingFlags)!;
}
