using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant.ValidationRules;

/// <summary>
/// Provides a foundational abstract base class for all validation rules within the framework,
/// both built-in and custom. It encapsulates common rule properties and defines the internal
/// mechanism for synchronous and asynchronous validation execution.
/// This class is primarily used internally by the validation system, particularly within
/// <see cref="PropertyRule{T, TExternalResources, TProperty, TContext}"/> collections.
/// </summary>
/// <typeparam name="T">The type of the root entity being validated.</typeparam>
/// <typeparam name="TExternalResources">The type representing any external resources required during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property the rule operates on.</typeparam>
public abstract class ValidationRule<T, TExternalResources, TProperty>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationRule{T, TExternalResources, TProperty}"/> class.
    /// </summary>
    /// <param name="canRunSynchronously">
    /// A value indicating whether this rule can be executed synchronously.
    /// This property dictates whether the internal <see cref="Validate"/> method or
    /// <see cref="ValidateAsync"/> method should be called by the framework.
    /// </param>
    private protected ValidationRule(bool canRunSynchronously)
    {
        CanRunSynchronously = canRunSynchronously;
    }

    /// <summary>
    /// Gets a value indicating whether this validation rule can be executed synchronously.
    /// This property is set during construction and informs the validation engine
    /// which validation method (<see cref="Validate"/> or <see cref="ValidateAsync"/>) to invoke.
    /// </summary>
    public bool CanRunSynchronously { get; }

    /// <summary>
    /// Gets the name of the validation rule. This name is used for identification and reporting, but is not guaranteed to be unique.
    /// Derived classes must provide an implementation for this property.
    /// </summary>
    public abstract ReadOnlySpan<char> RuleName { get; }

    /// <summary>
    /// Gets the default failure message for this validation rule if it fails.
    /// Derived classes must provide an implementation for this method.
    /// </summary>
    /// <param name="context">The message context.</param>
    /// <param name="value">The property value being validated.</param>
    /// <returns>The default failure message as a string.</returns>
    public abstract string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value);

    /// <summary>
    /// Executes the internal synchronous validation logic for the rule.
    /// This method is intended for internal use by the validation framework.
    /// Implementations should perform the validation and potentially add failures to the context.
    /// </summary>
    /// <param name="context">The validator run context.</param>
    /// <param name="property">The <see cref="Undefined{TProperty}"/> wrapper containing the property value.</param>
    /// <param name="failureInfo">The failure information associated with this rule.</param>
    internal abstract void Validate(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo);

    /// <summary>
    /// Executes the internal asynchronous validation logic for the rule.
    /// This method is intended for internal use by the validation framework.
    /// Implementations should perform the asynchronous validation and potentially add failures to the context.
    /// </summary>
    /// <param name="context">The validator run context.</param>
    /// <param name="property">The <see cref="Undefined{TProperty}"/> wrapper containing the property value.</param>
    /// <param name="failureInfo">The failure information associated with this rule.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    internal abstract ValueTask ValidateAsync(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo,
        CancellationToken ct);
}
