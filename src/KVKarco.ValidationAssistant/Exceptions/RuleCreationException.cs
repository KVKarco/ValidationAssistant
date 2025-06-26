using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Exceptions;

public sealed class RuleCreationException : ValidationAssistantException
{
    public RuleCreationException()
    {
    }

    public RuleCreationException(string message) : base(message)
    {
    }

    public RuleCreationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public static void ThrowIfNull<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
    {
        if (argument is null)
        {
            ThrowNull(argument, argumentName);
        }
    }

    public static void ThrowIfNullOrWhiteSpaces([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
    {
        ThrowIfNull(argument, argumentName);

        if (string.IsNullOrWhiteSpace(argumentName))
        {
            ThrowIfNull(argument, argumentName);
            throw new RuleCreationException(string.Concat(StringEmptyMsg, argumentName));
        }
    }

    //public static void ThrowIfInvalidSelector([NotNull] LambdaExpression? argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
    //{
    //    ThrowIfNull(argument, argumentName);

    //    if (argument.Body is ParameterExpression)
    //    {
    //        throw new RuleCreationException("Cant select main instance for validation.");
    //    }

    //    if (argument.Body is not MemberExpression memberExpression)
    //    {
    //        throw new RuleCreationException($"Only selecting properties or fields is allowed in {argumentName}.");
    //    }

    //    if (memberExpression.Member.Name == "Value" && memberExpression.Expression is ParameterExpression p && p.IsForNullableStruct())
    //    {
    //        throw new RuleCreationException("Cant select main instance for validation.");
    //    }

    //    if (memberExpression.Member.Name == "Value" && memberExpression.Expression is MemberExpression mb && mb.IsForNullableStruct())
    //    {
    //        throw new RuleCreationException("Don`t select Value from Nullable<>, select the property or field it self.");
    //    }
    //}

    //private static void ThrowIfInvalidMemberInMethodGuar([NotNull] Expression argument, string? argumentName)
    //{
    //    if (argument is not MemberExpression memberExpression)
    //    {
    //        throw new RuleCreationException($"Only selecting properties or fields is allowed in {argumentName}.");
    //    }

    //    if (memberExpression.Member.Name == "Value" && memberExpression.Expression is ParameterExpression p && p.IsForNullableStruct())
    //    {
    //        throw new RuleCreationException("Cant select main instance for validation.");
    //    }

    //    if (memberExpression.Member.Name == "Value" && memberExpression.Expression is MemberExpression mb && mb.IsForNullableStruct())
    //    {
    //        throw new RuleCreationException("Don`t select Value from Nullable<>, select the property or field it self.");
    //    }
    //}

    //public static void ThrowIfInvalidInputExtractor([NotNull] LambdaExpression? argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
    //{
    //    ThrowIfNull(argument, argumentName);

    //    if (argument.Body is NewExpression newExpression)
    //    {
    //        ReadOnlyCollection<Expression> arguments = newExpression.Arguments;

    //        foreach (Expression arg in arguments)
    //        {
    //            ThrowIfInvalidMemberInMethodGuar(arg, argumentName);
    //        }
    //    }
    //    else
    //    {
    //        ThrowIfInvalidMemberInMethodGuar(argument.Body, argumentName);
    //    }
    //}

    public static void ThrowIfNotMemberOrPropertyExpression([NotNull] MemberExpression? argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
    {
        ThrowIfNull(argument, argumentName);

        if (!(argument.Expression is ParameterExpression || argument.Expression is MemberExpression))
        {
            throw new RuleCreationException($"Only selecting properties or fields is allowed in {argumentName}.");
        }
    }


    private const string NullMsg = "Cannot create rule with null ";
    private const string StringEmptyMsg = "Cannot create rule with empty ";


    [DoesNotReturn]
    private static void ThrowNull<T>(T? argument, ReadOnlySpan<char> argumentName)
    {
        throw new RuleCreationException(string.Concat(NullMsg, argumentName));
    }
}
