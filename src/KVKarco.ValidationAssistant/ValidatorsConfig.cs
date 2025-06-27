using KVKarco.ValidationAssistant.Abstractions;
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

    /// <summary>
    /// Provides a default explanation message for when a validation rule set (PropertyRule) stops
    /// execution because a specified validation snapshot is found to be invalid.
    /// </summary>
    /// <typeparam name="T">The type of the main object instance being validated.</typeparam>
    /// <typeparam name="TExternalResources">The type of external resources available to the rule.</typeparam>
    /// <typeparam name="TProperty">The type of the property being validated, if applicable (can be ignored if not directly used by the message).</typeparam>
    /// <param name="context">The message context, providing access to the validated object, external resources, and culture information.</param>
    /// <param name="value">The actual value of the property associated with the component (can be ignored if not applicable to the message).</param>
    /// <param name="snapShotIdentifier">The unique identifier of the snapshot that was found to be invalid.</param>
    /// <returns>A localized string explaining that the snapshot is invalid and has caused the validation rule set to stop.</returns>
    /// <exception cref="ValidationRunException">Thrown if the specified <paramref name="context"/> culture is not supported for message generation.</exception>
    public virtual string InvalidSnapShotComponentExplanation<T, TExternalResources, TProperty>(
       [NotNull] IMessageCtx<T, TExternalResources> context,
       TProperty value,
       ReadOnlySpan<char> snapShotIdentifier)
    {
        if (context.Culture == CultureInfo.GetCultureInfo("en-US"))
        {
            return $"SnapShot {snapShotIdentifier} is not valid ValidationRuleSet(PropertyRule) stop executing.";
        }

        throw new ValidationRunException($"{context.Culture} is not supported.");
    }

    #endregion

    #region ValidatorRules failure explanations

    /// <summary>
    /// Provides a default explanation message for when a property's value cannot be extracted or is considered missing during validation.
    /// This method can be overridden in derived classes to provide culture-specific or more detailed explanations.
    /// </summary>
    /// <typeparam name="T">The type of the instance being validated.</typeparam>
    /// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
    /// <param name="context">The message context, providing access to validation details like property name and culture information.</param>
    /// <returns>A string explaining that the property value could not be extracted or is missing.</returns>
    /// <exception cref="ValidationRunException">Thrown if the provided culture in the context is not supported (currently only "en-US" is supported).</exception>
    public virtual string PropertyValueMissingExplanation<T, TExternalResources>(
        [NotNull] IMessageCtx<T, TExternalResources> context)
    {
        // Checks if the current culture in the context is "en-US".
        if (context.Culture == CultureInfo.GetCultureInfo("en-US"))
        {
            // Returns a standard English explanation message including the property name.
            return $"Property : {context.PropertyName} value cant be extracted is missing.";
        }

        // Throws an exception for unsupported cultures, indicating that localization for that culture is not implemented.
        throw new ValidationRunException($"{context.Culture} is not supported.");
    }


    #endregion

    #region pre-validation 

    /// <summary>
    /// Provides a default error message for a failed main instance pre-validation rule.
    /// This message is used when a fundamental check on the main instance itself fails.
    /// </summary>
    /// <typeparam name="T">The type of the instance being validated.</typeparam>
    /// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
    /// <param name="context">The message context, providing access to validation details like property name (if applicable) and culture information.</param>
    /// <returns>A string representing the default error message for a main instance pre-validation failure.</returns>
    /// <exception cref="ValidationRunException">Thrown if the provided culture in the context is not supported (currently only "en-US" is supported).</exception>
    public virtual string MainInstancePreValidationError<T, TExternalResources>(
        [NotNull] IMessageCtx<T, TExternalResources> context)
    {
        if (context.Culture == CultureInfo.GetCultureInfo("en-US"))
        {
            return $"{context.PropertyName} Its not valid."; // Note: PropertyName might be less relevant for whole-instance checks.
        }

        throw new ValidationRunException($"{context.Culture} is not supported.");
    }

    /// <summary>
    /// Provides a default error message for a failed external resources pre-validation rule.
    /// This message is used when a fundamental check on the external resources fails, indicating an internal problem.
    /// </summary>
    /// <typeparam name="T">The type of the instance being validated.</typeparam>
    /// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
    /// <param name="context">The message context, providing access to validation details and culture information.</param>
    /// <returns>A string representing the default error message for an external resources pre-validation failure.</returns>
    /// <exception cref="ValidationRunException">Thrown if the provided culture in the context is not supported (currently only "en-US" is supported).</exception>
    public virtual string ExternalResourcesPreValidationError<T, TExternalResources>(
        [NotNull] IMessageCtx<T, TExternalResources> context)
    {
        if (context.Culture == CultureInfo.GetCultureInfo("en-US"))
        {
            return $"Internal problem during the validation process.";
        }

        throw new ValidationRunException($"{context.Culture} is not supported.");
    }

    /// <summary>
    /// Provides a default explanation message for when a pre-validation rule failure causes the entire validation run to stop.
    /// This message clarifies why the validation process was prematurely terminated.
    /// </summary>
    /// <typeparam name="T">The type of the instance being validated.</typeparam>
    /// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
    /// <param name="context">The message context, providing access to validation details and culture information.</param>
    /// <returns>A string explaining that a pre-validation rule failure caused the validation to stop.</returns>
    /// <exception cref="ValidationRunException">Thrown if the provided culture in the context is not supported (currently only "en-US" is supported).</exception>
    public virtual string PreValidationDefaultExplanation<T, TExternalResources>(
        [NotNull] IMessageCtx<T, TExternalResources> context)
    {
        if (context.Culture == CultureInfo.GetCultureInfo("en-US"))
        {
            return $"PreValidation rule failure caused validation run to stop.";
        }

        throw new ValidationRunException($"{context.Culture} is not supported.");
    }

    #endregion
}