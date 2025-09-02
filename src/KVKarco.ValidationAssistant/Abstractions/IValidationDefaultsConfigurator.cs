using System.Globalization;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// A fluent interface for configuring the default settings of a validator.
/// These defaults can be overridden on a per-call, component, or rule basis.
/// </summary>
public interface IValidationDefaultsConfigurator
{
    /// <summary>
    /// Sets the default culture for generating validation messages.
    /// If not set, the global default culture is used.
    /// Setting this default multiple times will throw a <see cref="ValidatorCompositionException"/>.
    /// The culture can be overridden on a per-call basis in the Validate/ValidateAsync methods.
    /// </summary>
    /// <param name="culture">The culture information to use.</param>
    /// <returns>The same <see cref="IValidationDefaultsConfigurator"/> instance for chaining.</returns>
    IValidationDefaultsConfigurator SetCulture(CultureInfo culture);

    /// <summary>
    /// Sets the default severity for new validation failures.
    /// This sets the default severity for all validation rules in this validator.
    /// If not set, the global default severity is used.
    /// Setting this default multiple times will throw a <see cref="ValidatorCompositionException"/>.
    /// This value can be overridden on a component or a single rule basis.
    /// </summary>
    /// <param name="severity">The default severity level.</param>
    /// <returns>The same <see cref="IValidationDefaultsConfigurator"/> instance for chaining.</returns>
    IValidationDefaultsConfigurator SetSeverity(Severity severity);

    /// <summary>
    /// Sets the default flow effect for a component's rules.
    /// This determines the flow impact on the component level when one of its rules fails.
    /// If not set, the global default flow effect is used.
    /// Setting this default multiple times will throw a <see cref="ValidatorCompositionException"/>.
    /// This value can be overridden on a per-component or per-rule basis.
    /// </summary>
    /// <param name="effect">The default flow effect.</param>
    /// <returns>The same <see cref="IValidationDefaultsConfigurator"/> instance for chaining.</returns>
    IValidationDefaultsConfigurator SetRuleLevelFlowEffect(FlowEffect effect);

    /// <summary>
    /// Sets the default flow effect for components within this validator.
    /// This determines the flow impact on the validator level when a component is considered failed.
    /// If not set, the global default flow effect is used.
    /// Setting this default multiple times will throw a <see cref="ValidatorCompositionException"/>.
    /// This value can be overridden on a per-component basis.
    /// </summary>
    /// <param name="effect">The default flow effect.</param>
    /// <returns>The same <see cref="IValidationDefaultsConfigurator"/> instance for chaining.</returns>
    IValidationDefaultsConfigurator SetValidatorLevelFlowEffect(FlowEffect effect);
}
