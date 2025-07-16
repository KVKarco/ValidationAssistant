using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant.ValidationRules;

/// <summary>
/// Provides a synchronous abstract base class for creating custom, user-defined validation rules.
/// Consumers of the library should inherit from this class to implement their synchronous validation logic.
/// This class handles the internal plumbing for rule execution, delegating the core validation
/// to the abstract <see cref="IsValid(IValidationCtx{T, TExternalResources}, TProperty)"/> method.
/// </summary>
/// <typeparam name="T">The type of the root entity being validated.</typeparam>
/// <typeparam name="TExternalResources">The type representing any external resources required during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated by this rule.</typeparam>
public abstract class CustomValidationRule<T, TExternalResources, TProperty> :
    ValidationRule<T, TExternalResources, TProperty>,
    IValidationRule<T, TExternalResources, TProperty> // Implements the synchronous contract
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomValidationRule{T, TExternalResources, TProperty}"/> class.
    /// This rule is always marked as synchronous.
    /// </summary>
    /// <param name="canRunSynchronously">This parameter is kept for signature consistency but is ignored; this rule is inherently synchronous.</param>
    protected CustomValidationRule(bool canRunSynchronously)
       : base(true) // Custom synchronous rules always report as synchronous.
    {
    }

    /// <inheritdoc cref="IValidationRule{T, TExternalResources, in TProperty}.IsValid(IValidationCtx{T, TExternalResources}, TProperty)"/>
    /// <summary>
    /// Abstract method to be implemented by custom rule creators to define the synchronous validation logic.
    /// This method should return <c>true</c> if the property value is valid, <c>false</c> otherwise.
    /// </summary>
    public abstract bool IsValid(IValidationCtx<T, TExternalResources> context, TProperty value);

    /// <summary>
    /// <inheritdoc/>
    /// Overrides the internal <see cref="ValidationRule{T, TExternalResources, TProperty}.Validate"/> method to provide
    /// the concrete synchronous execution flow for custom rules. It checks for missing property values
    /// and then delegates to the abstract <see cref="IsValid(IValidationCtx{T, TExternalResources}, TProperty)"/> method.
    /// If <see cref="IsValid"/> returns <c>false</c>, a validation failure is added to the context.
    /// </summary>
    /// <param name="context">The validator run context.</param>
    /// <param name="property">The <see cref="Undefined{TProperty}"/> wrapper containing the property value.</param>
    /// <param name="failureInfo">The failure information associated with this rule.</param>
    internal sealed override void Validate(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo)
    {
        // If the property value is missing, force a stop (as the rule cannot validate an undefined value).
        if (!property.HasValue)
        {
            context.ForceStopPropertyValueIsMissing(property);
            return; // Exit early as validation cannot proceed without a value
        }

        // Execute the custom synchronous validation logic.
        if (!IsValid(context, property.Value))
        {
            context.AddValidationRuleFailure(property, failureInfo);
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// Provides a synchronous implementation for the asynchronous validation method by simply
    /// calling the synchronous <see cref="Validate(ValidatorRunCtx{T, TExternalResources}, Undefined{TProperty}, ValidationRuleFailureInfo{T, TExternalResources, TProperty})"/> method
    /// and immediately returning a completed task. This ensures synchronous custom rules can be invoked uniformly.
    /// </summary>
    /// <param name="context">The validator run context.</param>
    /// <param name="property">The <see cref="Undefined{TProperty}"/> wrapper containing the property value.</param>
    /// <param name="failureInfo">The failure information associated with this rule.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe (though typically not used in a synchronous wrapper).</param>
    /// <returns>A <see cref="ValueTask"/> that is already completed.</returns>
    internal sealed override ValueTask ValidateAsync(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo,
        CancellationToken ct)
    {
        // For synchronous custom rules, the async method just wraps the sync validation.
        Validate(context, property, failureInfo);
        return ValueTask.CompletedTask;
    }
}