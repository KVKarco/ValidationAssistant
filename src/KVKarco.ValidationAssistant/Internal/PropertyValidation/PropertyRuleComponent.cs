using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.ValidationRules;

namespace KVKarco.ValidationAssistant.Internal.PropertyValidation;

/// <summary>
/// Represents a sealed concrete component that wraps either a synchronous (<see cref="IValidationRule{T, TExternalResources, TProperty}"/>)
/// or an asynchronous (<see cref="IAsyncValidationRule{T, TExternalResources, TProperty}"/>) validation rule.
/// This component also holds information related to potential validation failures and provides methods to execute the wrapped rule.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property currently being validated.</typeparam>
internal sealed class PropertyRuleComponent<T, TExternalResources, TProperty>
{
    /// <summary>
    /// The synchronous validation rule instance, if this component wraps a synchronous rule.
    /// This field will be <see langword="null"/> if <see cref="_asyncRule"/> is not <see langword="null"/>.
    /// </summary>
    private readonly IValidationRule<T, TExternalResources, TProperty>? _rule;

    /// <summary>
    /// The asynchronous validation rule instance, if this component wraps an asynchronous rule.
    /// This field will be <see langword="null"/> if <see cref="_rule"/> is not <see langword="null"/>.
    /// </summary>
    private readonly IAsyncValidationRule<T, TExternalResources, TProperty>? _asyncRule;

    /// <summary>
    /// Contains information about how a validation failure should be handled and the factory
    /// for generating custom failure messages for this specific component.
    /// </summary>
    private readonly ComponentFailureInfo<T, TExternalResources, TProperty> _info;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyRuleComponent{T, TExternalResources, TProperty}"/> class
    /// for a synchronous validation rule.
    /// </summary>
    /// <param name="rule">The synchronous validation rule to wrap. Must not be <see langword="null"/>.</param>
    /// <param name="info">The metadata describing how to handle failures for this component.
    /// This will be stored in the internal <see cref="_info"/> field.</param>
    public PropertyRuleComponent(
        IValidationRule<T, TExternalResources, TProperty> rule,
        ComponentFailureInfo<T, TExternalResources, TProperty> info)
    {
        _rule = rule;
        _asyncRule = null; // Explicitly set to null to indicate a synchronous rule.
        _info = info;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyRuleComponent{T, TExternalResources, TProperty}"/> class
    /// for an asynchronous validation rule.
    /// </summary>
    /// <param name="asyncRule">The asynchronous validation rule to wrap. Must not be <see langword="null"/>.</param>
    /// <param name="info">The metadata describing how to handle failures for this component.
    /// This will be stored in the internal <see cref="_info"/> field.</param>
    public PropertyRuleComponent(
        IAsyncValidationRule<T, TExternalResources, TProperty> asyncRule,
        ComponentFailureInfo<T, TExternalResources, TProperty> info)
    {
        _rule = null; // Explicitly set to null to indicate an asynchronous rule.
        _asyncRule = asyncRule;
        _info = info;
    }

    /// <summary>
    /// Gets a value indicating whether the wrapped validation rule is synchronous.
    /// This is <see langword="true"/> if an <see cref="IValidationRule{T, TExternalResources, TProperty}"/> was provided
    /// during construction, and <see langword="false"/> if an <see cref="IAsyncValidationRule{T, TExternalResources, TProperty}"/> was provided.
    /// </summary>
    public bool CanRunSynchronously => _asyncRule is null;

    /// <summary>
    /// Gets the name of the wrapped validation rule component. This property delegates to the
    /// <see cref="IValidationRule{T, TExternalResources, TProperty}.RuleName"/> or
    /// <see cref="IAsyncValidationRule{T, TExternalResources, TProperty}.RuleName"/> of the contained rule.
    /// </summary>
    public ReadOnlySpan<char> ComponentName => CanRunSynchronously ? _rule!.RuleName : _asyncRule!.RuleName;

    /// <summary>
    /// Executes the wrapped synchronous validation rule against the provided context and property value.
    /// If the rule fails, a validation failure is added to the context using the component's failure information.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <param name="property">The property value wrapped in an <see cref="Undefined{TProperty}"/>, allowing for the
    /// distinction between a null value and an undefined property.</param>
    /// <exception cref="ValidationRunException">Thrown if this method is called when the component wraps an asynchronous rule.</exception>
    public void Validate(ValidatorRunCtx<T, TExternalResources> context, Undefined<TProperty> property)
    {
        // Ensures that a synchronous rule is not attempting to execute an asynchronous validation.
        ValidationRunException.ThrowIfAsyncRuleIsCalledSynchronously(CanRunSynchronously, ComponentName);

        // Executes the synchronous validation rule. The null-forgiving operator (!) is used
        // because CanRunSynchronously ensures _rule is not null in this branch.
        if (!_rule!.IsValid(context, property.Value))
        {
            // If validation fails, add a failure to the context with the component's info.
            context.AddPropertyRuleComponentFailure(property, _info);
        }
    }

    /// <summary>
    /// Executes the wrapped validation rule (synchronous or asynchronous) against the provided context and property value.
    /// If the rule fails, a validation failure is added to the context using the component's failure information.
    /// Supports cancellation.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <param name="property">The property value wrapped in an <see cref="Undefined{TProperty}"/>.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous validation operation.</returns>
    public async ValueTask ValidateAsync(ValidatorRunCtx<T, TExternalResources> context, Undefined<TProperty> property, CancellationToken ct)
    {
        bool isValid = CanRunSynchronously
            ? _rule!.IsValid(context, property.Value)
            : await _asyncRule!.IsValidAsync(context, property.Value, ct).ConfigureAwait(false);

        if (!isValid)
        {
            // If validation fails, add a failure to the context with the component's info.
            context.AddPropertyRuleComponentFailure(property, _info);
        }
    }
}
