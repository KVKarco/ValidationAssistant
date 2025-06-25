using KVKarco.ValidationAssistant.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KVKarco.ValidationAssistant;

/// <summary>
/// Provides static access to global configuration settings for the Validation Assistant framework.
/// This class ensures a single point of access for default behaviors and messages across all validators.
/// </summary>
public static class ValidatorsConfig
{
    /// <summary>
    /// Gets the default global configuration settings for validators.
    /// This instance holds settings such as default culture, messages, and failure strategies.
    /// </summary>
    public static readonly DefaultConfiguration GlobalDefaults = new();
}

/// <summary>
/// Encapsulates default configuration settings for the Validation Assistant framework.
/// These settings can be overridden at more granular levels within individual validators or rules.
/// </summary>
public sealed class DefaultConfiguration
{
    /// <summary>
    /// Gets or sets the default culture to be used for validation messages and formatting.
    /// Defaults to "en-US".
    /// </summary>
    public CultureInfo DefaultCulture { get; set; } = CultureInfo.GetCultureInfo("en-US");

    /// <summary>
    /// Gets or sets the collection of default messages used for various validation outcomes.
    /// </summary>
    public DefaultMessages Messages { get; set; } = new DefaultMessages();

    /// <summary>
    /// Gets or sets the default strategy to apply when a validator rule failure occurs.
    /// Defaults to <see cref="RuleFailureStrategy.Continue"/>.
    /// </summary>
    public RuleFailureStrategy OnRuleFailure { get; set; } = RuleFailureStrategy.Continue;

    /// <summary>
    /// Gets or sets the default strategy to apply when a component(ValidationRule) failure occurs within a rule.
    /// Defaults to <see cref="ComponentFailureStrategy.Continue"/>.
    /// </summary>
    public ComponentFailureStrategy OnComponentFailure { get; set; } = ComponentFailureStrategy.Continue;
}

/// <summary>
/// Provides a collection of default, customizable messages used throughout the Validation Assistant framework.
/// These messages are typically used for explanations or error descriptions when no specific message is provided.
/// </summary>
public class DefaultMessages
{
    #region Components info messages

    /// <summary>
    /// Provides the default explanation message for when a logical component stopped PropertyRule(ValidatorRule) execution.
    /// </summary>
    /// <typeparam name="T">The type of the main validation instance.</typeparam>
    /// <typeparam name="TExternalResources">The type of external resources.</typeparam>
    /// <typeparam name="TProperty">The type of the property involved, if any.</typeparam>
    /// <param name="context">The current validation message context, providing access to culture and property name.</param>
    /// <param name="value">The value of the property associated with the component (can be ignored if not applicable).</param>
    /// <returns>A localized string explaining the PropertyRule stop.</returns>
    /// <exception cref="ValidationRunException">Thrown if the specified <paramref name="context"/> culture is not supported.</exception>
    public virtual string LogicalComponentStopExplanation<T, TExternalResources, TProperty>(
        [NotNull] IMessageCtx<T, TExternalResources> context,
        TProperty value)
    {
        if (context.Culture == CultureInfo.GetCultureInfo("en-US"))
        {
            return $"PropertyRule for {context.PropertyName} is stopped from failed ValidationRule.";
        }

        throw new ValidationRunException($"{context.Culture} is not supported.");
    }

    /// <summary>
    /// Provides the default error message for when a ValidationRule(component) condition is not met.
    /// </summary>
    /// <typeparam name="T">The type of the main validation instance.</typeparam>
    /// <typeparam name="TExternalResources">The type of external resources.</typeparam>
    /// <typeparam name="TProperty">The type of the property involved, if any.</typeparam>
    /// <param name="context">The current validation message context, providing access to culture and property name.</param>
    /// <param name="value">The value of the property associated with the component (can be ignored if not applicable).</param>
    /// <returns>A localized string describing the validation component error message.</returns>
    /// <exception cref="ValidationRunException">Thrown if the specified <paramref name="context"/> culture is not supported.</exception>
    public virtual string ValidationComponentError<T, TExternalResources, TProperty>(
        [NotNull] IMessageCtx<T, TExternalResources> context,
        TProperty value)
    {
        if (context.Culture == CultureInfo.GetCultureInfo("en-US"))
        {
            return $"The specified condition was not met for {context.PropertyName}.";
        }

        throw new ValidationRunException($"{context.Culture} is not supported.");
    }

    /// <summary>
    /// Provides the default explanation message for when a child validator component(ValidationRule) fails validation for a property.
    /// </summary>
    /// <typeparam name="T">The type of the main validation instance.</typeparam>
    /// <typeparam name="TExternalResources">The type of external resources.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated by the child validator.</typeparam>
    /// <param name="context">The current validation message context, providing access to culture and property name.</param>
    /// <param name="value">The value of the property that the child validator attempted to validate.</param>
    /// <returns>A localized string explaining the child validator failure.</returns>
    /// <exception cref="ValidationRunException">Thrown if the specified <paramref name="context"/> culture is not supported.</exception>
    public virtual string ChildValidatorComponentExplanation<T, TExternalResources, TProperty>(
        [NotNull] IMessageCtx<T, TExternalResources> context,
        TProperty value)
    {
        if (context.Culture == CultureInfo.GetCultureInfo("en-US"))
        {
            return $"PropertyValidator failed validating property: {context.PropertyName}.";
        }

        throw new ValidationRunException($"{context.Culture} is not supported.");
    }

    #endregion
}