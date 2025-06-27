using KVKarco.ValidationAssistant.Internal.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Exceptions;


/// <summary>
/// Represents an exception that is thrown when an error occurs during the creation or definition
/// of a validation rule. This exception typically indicates invalid input to rule building methods,
/// such as null arguments, improperly formed expressions, or attempts to define rules in unsupported ways.
/// </summary>
public sealed class RuleCreationException : ValidationAssistantException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuleCreationException"/> class.
    /// </summary>
    public RuleCreationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleCreationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RuleCreationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleCreationException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference
    /// (<see langword="Nothing"/> in Visual Basic) if no inner exception is specified.</param>
    public RuleCreationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Throws a <see cref="RuleCreationException"/> if the specified <paramref name="argument"/> is <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="argument">The argument to check for <see langword="null"/>.</param>
    /// <param name="argumentName">The name of the argument, automatically captured by <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <exception cref="RuleCreationException">Thrown if <paramref name="argument"/> is <see langword="null"/>.</exception>
    public static void ThrowIfNull<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
    {
        if (argument is null)
        {
            ThrowNull(argument!, argumentName); // argument! is safe because ThrowNull has [DoesNotReturn]
        }
    }

    /// <summary>
    /// Throws a <see cref="RuleCreationException"/> if the specified string <paramref name="argument"/> is <see langword="null"/>,
    /// empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="argument">The string argument to check.</param>
    /// <param name="argumentName">The name of the argument, automatically captured by <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <exception cref="RuleCreationException">
    /// Thrown if <paramref name="argument"/> is <see langword="null"/>, empty, or consists only of white-space characters.
    /// </exception>
    public static void ThrowIfNullOrWhiteSpaces([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
    {
        ThrowIfNull(argument, argumentName); // Checks for null first

        if (string.IsNullOrWhiteSpace(argument)) // Corrected: Checking the 'argument' content
        {
            // If argument is null, ThrowIfNull already handles it.
            // If it's whitespace or empty, this throws.
            throw new RuleCreationException(string.Concat(StringEmptyMsg, argumentName));
        }
    }

    /// <summary>
    /// Throws a <see cref="RuleCreationException"/> if the provided <paramref name="argument"/>
    /// (a <see cref="LambdaExpression"/> used as a property selector) is invalid.
    /// Valid selectors must target a property or field, not the entire instance,
    /// and should not directly select the `.Value` property of a `Nullable<T>` type.
    /// </summary>
    /// <param name="argument">The lambda expression to validate as a property selector.</param>
    /// <param name="argumentName">The name of the argument, automatically captured by <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <exception cref="RuleCreationException">Thrown if the selector is <see langword="null"/> or invalid as per the rules.</exception>
    public static void ThrowIfInvalidSelector([NotNull] LambdaExpression? argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
    {
        ThrowIfNull(argument, argumentName);

        // Disallow selecting the root instance itself (e.g., `x => x`)
        if (argument.Body is ParameterExpression)
        {
            throw new RuleCreationException("Cannot select the main instance for validation directly. A property or field must be selected.");
        }

        // Validate the expression body as a member access.
        ValidateMemberAccess(argument.Body, argumentName);
    }

    /// <summary>
    /// Internal helper method to validate that an <see cref="Expression"/> represents a valid
    /// property or field access, and does not incorrectly select `.Value` from a `Nullable<T>`.
    /// This method is designed to be reusable by other validation methods.
    /// </summary>
    /// <param name="expressionToValidate">The expression to validate.</param>
    /// <param name="argumentName">The name of the argument from the calling context.</param>
    /// <exception cref="RuleCreationException">Thrown if the expression is invalid.</exception>
    private static void ValidateMemberAccess([NotNull] Expression expressionToValidate, string? argumentName)
    {
        // Ensure the expression is a MemberExpression (property or field access)
        if (expressionToValidate is not MemberExpression memberExpression)
        {
            throw new RuleCreationException($"Only selecting properties or fields is allowed in '{argumentName}'.");
        }

        // Disallow selecting `.Value` from a Nullable<T> if it's directly from the root parameter
        if (memberExpression.Member.Name == "Value" && memberExpression.Expression is ParameterExpression p && p.Type.IsNullableStruct())
        {
            throw new RuleCreationException($"Cannot select the main instance for validation indirectly via '.Value' of a nullable property at the root.");
        }

        // Disallow selecting `.Value` from a nested Nullable<T> member
        if (memberExpression.Member.Name == "Value" && memberExpression.Expression is MemberExpression mb && mb.Type.IsNullableStruct())
        {
            throw new RuleCreationException("Do not select '.Value' from Nullable<>. Select the property or field itself.");
        }
    }


    /// <summary>
    /// Throws a <see cref="RuleCreationException"/> if the provided <paramref name="argument"/>
    /// (a <see cref="LambdaExpression"/> used as an input extractor) is invalid.
    /// This method supports extracting multiple properties (e.g., `new { x.Prop1, x.Prop2 }`)
    /// and validates each extracted member.
    /// </summary>
    /// <param name="argument">The lambda expression to validate as an input extractor.</param>
    /// <param name="argumentName">The name of the argument, automatically captured by <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <exception cref="RuleCreationException">Thrown if the extractor is <see langword="null"/> or invalid as per the rules.</exception>
    public static void ThrowIfInvalidInputExtractor([NotNull] LambdaExpression? argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
    {
        ThrowIfNull(argument, argumentName);

        // If the body is a NewExpression (e.g., anonymous object creation `new { x.Prop1, x.Prop2 }`)
        if (argument.Body is NewExpression newExpression)
        {
            ReadOnlyCollection<Expression> arguments = newExpression.Arguments;

            foreach (Expression arg in arguments)
            {
                // Validate each argument within the NewExpression
                ValidateMemberAccess(arg, argumentName);
            }
        }
        else // If the body is a single property/field access
        {
            ValidateMemberAccess(argument.Body, argumentName);
        }
    }

    /// <summary>
    /// Throws a <see cref="RuleCreationException"/> if the provided <paramref name="argument"/>
    /// (a <see cref="MemberExpression"/>) is not a valid property or field access.
    /// It specifically checks that the expression's parent is either a <see cref="ParameterExpression"/>
    /// (root object) or another <see cref="MemberExpression"/> (nested property).
    /// </summary>
    /// <param name="argument">The member expression to validate.</param>
    /// <param name="argumentName">The name of the argument, automatically captured by <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <exception cref="RuleCreationException">Thrown if the argument is <see langword="null"/> or invalid.</exception>
    public static void ThrowIfNotMemberOrPropertyExpression([NotNull] MemberExpression? argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
    {
        ThrowIfNull(argument, argumentName);

        // A valid member expression must have its 'Expression' property pointing to
        // either the root parameter (e.g., `x.Prop`) or another member expression (e.g., `x.Obj.Prop`).
        if (!(argument.Expression is ParameterExpression || argument.Expression is MemberExpression))
        {
            throw new RuleCreationException($"Only selecting properties or fields is allowed in '{argumentName}'.");
        }
    }


    /// <summary>The default message prefix for null argument exceptions.</summary>
    private const string NullMsg = "Cannot create rule with null ";
    /// <summary>The default message prefix for empty/whitespace string argument exceptions.</summary>
    private const string StringEmptyMsg = "Cannot create rule with empty ";


    /// <summary>
    /// Internal helper method that actually throws a <see cref="RuleCreationException"/> for a null argument.
    /// This method is marked with <see cref="DoesNotReturnAttribute"/> to inform the compiler
    /// that it will always throw an exception, which can help with static analysis for nullability.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="argument">The argument that was null (value is ignored as it's known to be null).</param>
    /// <param name="argumentName">The name of the argument.</param>
    /// <exception cref="RuleCreationException">Always thrown.</exception>
    [DoesNotReturn]
    private static void ThrowNull<T>(T? argument, ReadOnlySpan<char> argumentName)
    {
        throw new RuleCreationException(string.Concat(NullMsg, argumentName.ToString()));
    }
}