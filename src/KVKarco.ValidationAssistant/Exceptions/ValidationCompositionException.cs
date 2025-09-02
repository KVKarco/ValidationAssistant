using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Exceptions;

public sealed class ValidationCompositionException : ValidationAssistantException
{
    internal ValidationCompositionException(string message) : base(message)
    {
    }

    internal ValidationCompositionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    internal ValidationCompositionException()
    {
    }

    /// <summary>
    /// Throw when possible closure is detected.
    /// </summary>
    /// <param name="del"></param>
    /// <param name="paramName"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrClosure(
        Delegate? del,
        [CallerArgumentExpression("del")] string? paramName = null)
    {
        if (del is null)
        {
            ThrowOnNullPredicate(paramName ?? "unknown");
        }

        if (IsClosure(del))
        {
            ThrowOnClosure(paramName ?? "unknown", del);
        }

    }

    /// <summary>
    /// Throws if the selector expression is invalid according to the strict rules:
    /// - Cannot be null.
    /// - Reject constants anywhere.
    /// - Reject static properties/fields accessed anywhere.
    /// - Reject .Value only if it's the last member accessed
    /// - Reject method calls anywhere.
    /// - Reject indexer access anywhere.
    /// - Reject all casts (implicit or explicit) anywhere.
    /// - Reject assignment/mutation expressions anywhere.
    /// - Reject conditionals anywhere.
    /// - Must be pure property or field access chain.
    /// </summary>

    internal static void ThrowIfInvalidSelector(
        [NotNull] LambdaExpression? selector,
        [CallerArgumentExpression(nameof(selector))] string? selectorName = null)
    {
        if (selector is null)
        {
            throw new ValidationCompositionException(
                $"Selector '{selectorName}' cannot be null.");
        }

        Expression body = selector.Body;

        // Rule 2: Disallow literal constants at root, e.g. `x => 5` or `x => \"hello\"`
        if (body is ConstantExpression)
        {
            throw new ValidationCompositionException(
                "Cannot use constant values in a property selector. A property or field access is required.");
        }

        // Recursively validate the expression tree starting from the root
        ValidateExpressionTree(body, isRoot: true);
    }

    private static void ValidateExpressionTree(Expression expression, bool isRoot)
    {
        switch (expression)
        {
            case MemberExpression memberExpr:
                {
                    // Rule: Disallow static members, even if accessed through an instance chain
                    bool isStatic =
                        (memberExpr.Member is PropertyInfo pi && (pi.GetMethod?.IsStatic ?? false)) ||
                        (memberExpr.Member is FieldInfo fi && fi.IsStatic);

                    if (isStatic)
                    {
                        throw new ValidationCompositionException(
                            $"Static member '{memberExpr.Member.Name}' cannot be used in selector expressions. Select instance members only.");
                    }

                    // Rule: Disallow '.Value' property from Nullable<T> only at the end of the chain (root)
                    if (IsValueProperty(memberExpr.Member) && isRoot)
                    {
                        throw new ValidationCompositionException(
                            "Selector expression cannot end with the '.Value' property of a Nullable<T> type.");
                    }

                    // Recursively validate the inner expression (the object instance)
                    ValidateExpressionTree(memberExpr!.Expression!, isRoot: false);
                    break;
                }
            case ParameterExpression paramExpr:
                {
                    break;
                }
            case ConstantExpression constExpr:
                {
                    // Rule: No constants anywhere inside the selector expression
                    // (e.g., x => x.Prop + 5 is invalid)
                    throw new ValidationCompositionException(
                        "Constants are not allowed inside selector expressions.");
                }
            case MethodCallExpression methodCallExpr:
                {
                    // Rule: No method or delegate calls allowed anywhere
                    throw new ValidationCompositionException(
                        "Method calls are not allowed in selector expressions.");
                }
            case UnaryExpression unaryExpr when
                unaryExpr.NodeType == ExpressionType.Convert ||
                unaryExpr.NodeType == ExpressionType.ConvertChecked:
                {
                    // Rule: No explicit or implicit cast expressions allowed
                    throw new ValidationCompositionException(
                        "Cast expressions are not allowed in selector expressions.");
                }
            case IndexExpression _:
                {
                    // Rule: No indexer access anywhere (disallow collection element selection)
                    throw new ValidationCompositionException(
                        "Indexer access is not allowed in selector expressions.");
                }
            case BinaryExpression binaryExpr:
                {
                    // Rule: No assignments or mutations allowed (e.g., x => x.Prop = value)
                    if (binaryExpr.NodeType == ExpressionType.Assign)
                    {
                        throw new ValidationCompositionException(
                            "Assignment expressions are not allowed in selector expressions.");
                    }

                    // Disallow any other binary expressions (e.g., x => x.Prop1 + x.Prop2)
                    throw new ValidationCompositionException(
                        $"Binary expressions are not allowed in selector expressions (NodeType={binaryExpr.NodeType}).");
                }
            case InvocationExpression invocationExpr:
                {
                    // Rule: No invocation of delegates or lambdas allowed
                    throw new ValidationCompositionException(
                        "Invocation of delegates or lambdas is not allowed in selector expressions.");
                }

            case NewExpression newExpr:
                {
                    // Rule: No anonymous or new object constructions allowed
                    throw new ValidationCompositionException(
                        "Anonymous or new object constructions are not allowed in selector expressions.");
                }

            case ConditionalExpression conditionalExpr:
                {
                    // Rule: No conditional expressions (?:) allowed
                    throw new ValidationCompositionException(
                        "Conditional expressions are not allowed in selector expressions.");
                }

            case DefaultExpression defaultExpr:
                {
                    // Rule: Disallow default expressions anywhere
                    throw new ValidationCompositionException(
                        "Default expressions are not allowed in selector expressions.");
                }
            default:
                {
                    // For any unsupported or unexpected expression types
                    throw new ValidationCompositionException(
                        $"Unsupported expression type '{expression.NodeType}' in selector expressions.");
                }
        }
    }
    private static bool IsClosure(Delegate del)
    {
        // Any bound instance (non-static) implies possible closure or unsafe context.
        return del.Target is not null;
    }

    private static bool IsValueProperty(MemberInfo member)
    {
        return member is PropertyInfo pi && pi.Name == "Value";
    }

    [DoesNotReturn]
    private static void ThrowOnNullPredicate(string paramName)
        => throw new ValidationCompositionException(
            $"Delegate '{paramName}' is null. A valid delegate must be provided when defining validation logic.");

    [DoesNotReturn]
    private static void ThrowOnClosure(string paramName, Delegate del)
    {
        var targetType = del.Target?.GetType().FullName ?? "null";

        throw new ValidationCompositionException(
        $"""
        Invalid delegate detected: '{paramName}' is bound to an instance or captures closure context.

        Delegate target type: {targetType}
        ⚠️ This means the lambda or method you're using references variables or instance state from outside its own scope.

        ❌ Examples of problematic usage:
            - Ensure(x => x == _someField)
            - Ensure(x => SomeInstanceMethod(x))   // instance-bound method
            - Ensure(x => ctx.Resources.X + someOuterVar)

        ✅ Valid alternatives:
            - Ensure(x => x.Age > 18)
            - Ensure(static x => IsValid(x))       // static method call
            - Ensure(x => x.Status == Status.Ready)

        💡 Tip:
        Use static methods or pure lambdas that only rely on the provided context.
        Avoid capturing outer-scope variables or using instance methods from the validation class.

        🔒 Why it's disallowed:
        Captured state can introduce:
            - memory leaks (closure holds on to outer objects)
            - thread safety issues
            - nondeterministic behavior in reused validation logic

        """
        );
    }
}
