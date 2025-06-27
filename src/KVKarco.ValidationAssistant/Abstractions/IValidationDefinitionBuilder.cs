using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// Defines a foundational, generic contract for building and configuring any type of validator
/// within the ValidationAssistant framework. This interface serves as the base for
/// more specific validator definition builders (e.g., for ExpressValidator, FluentValidator, etc.),
/// providing common capabilities or markers for validator construction.
/// </summary>
/// <typeparam name="T">The type of the instance being validated by the builder.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies that
/// might be required for validator construction or evaluation.</typeparam>
public interface IValidationDefinitionBuilder<T, TExternalResources>
{
    /// <summary>
    /// Initiates the definition of validation rules for a specific property of the instance <typeparamref name="T"/>.
    /// This method is the starting point for defining a *single PropertyRule* composed of one or more
    /// sequential validation rules that are dependent on each other. This method is common to all validators
    /// that support property-based validation.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being selected for validation.</typeparam>
    /// <param name="propertySelector">
    /// An expression that selects the property for which rules are being defined (e.g., <c>x => x.PropertyName</c>).
    /// </param>
    /// <param name="callingFileLineNumber">
    /// The line number in the source file where this method is called. This is automatically
    /// populated by the compiler and is primarily used for debugging or enhanced error reporting.
    /// </param>
    /// <returns>
    /// An <see cref="IInitialPropertyRuleBuilder{T, TExternalResources, TProperty}"/> instance,
    /// providing further fluent methods to specify validation criteria for the selected property.
    /// </returns>
    /// <remarks>
    /// Use this method to chain property-specific rules such as <c>.NotNull()</c>, <c>.NotEmpty()</c>,
    /// <c>.MatchesRegex()</c>, etc., all of which contribute to the single PropertyRule being built.
    /// </remarks>
    /// <example>
    /// <code>
    /// UseFor(x => x.Name)
    ///    .NotNullOrEmpty()
    ///    .Length(1, 100);
    /// </code>
    /// </example>
    /// <exception cref="RuleCreationException">
    /// Thrown if the <paramref name="propertySelector"/> is <see langword="null"/> or if it represents an invalid
    /// property selection (e.g., not a member access expression for a property or field).
    /// </exception>
    IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> UseFor<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        [CallerLineNumber] int callingFileLineNumber = 0);
}
