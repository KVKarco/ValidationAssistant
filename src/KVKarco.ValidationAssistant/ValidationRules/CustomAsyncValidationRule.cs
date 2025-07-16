using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant.ValidationRules;

/// <summary>
/// Provides an asynchronous abstract base class for creating custom, user-defined validation rules.
/// Consumers of the library should inherit from this class to implement their asynchronous validation logic.
/// This class handles the internal plumbing for asynchronous rule execution, delegating the core validation
/// to the abstract <see cref="IsValidAsync(IValidationCtx{T, TExternalResources}, TProperty, CancellationToken)"/> method.
/// </summary>
/// <typeparam name="T">The type of the root entity being validated.</typeparam>
/// <typeparam name="TExternalResources">The type representing any external resources required during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated by this rule.</typeparam>
public abstract class CustomAsyncValidationRule<T, TExternalResources, TProperty> :
    ValidationRule<T, TExternalResources, TProperty>,
    IAsyncValidationRule<T, TExternalResources, TProperty> // Implements the asynchronous contract
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomAsyncValidationRule{T, TExternalResources, TProperty}"/> class.
    /// This rule is always marked as asynchronous.
    /// </summary>
    /// <param name="canRunSynchronously">This parameter is kept for signature consistency but is ignored; this rule is inherently asynchronous.</param>
    protected CustomAsyncValidationRule(bool canRunSynchronously)
       : base(false) // Custom asynchronous rules always report as asynchronous.
    {
    }

    /// <inheritdoc cref="IAsyncValidationRule{T, TExternalResources, in TProperty}.IsValidAsync(IValidationCtx{T, TExternalResources}, TProperty, CancellationToken)"/>
    /// <summary>
    /// Abstract method to be implemented by custom rule creators to define the asynchronous validation logic.
    /// This method should return <c>true</c> if the property value is valid, <c>false</c> otherwise.
    /// </summary>
    public abstract Task<bool> IsValidAsync(IValidationCtx<T, TExternalResources> context, TProperty value, CancellationToken ct);

    /// <summary>
    /// <inheritdoc/>
    /// Overrides the internal <see cref="ValidationRule{T, TExternalResources, TProperty}.Validate"/> method.
    /// For asynchronous custom rules, calling this synchronous method is considered an error
    /// and will force the validation process to stop with a specific failure indicating
    /// that an asynchronous rule was invoked synchronously.
    /// </summary>
    /// <param name="context">The validator run context.</param>
    /// <param name="property">The <see cref="Undefined{TProperty}"/> wrapper containing the property value.</param>
    /// <param name="failureInfo">The failure information associated with this rule.</param>
    internal sealed override void Validate(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo)
    {
        // Asynchronous custom rules cannot be run synchronously.
        // This stops the validation process and reports a specific error.
        context.ForceStopAsyncValidationRuleCalledSynchronously(property, failureInfo);
    }

    /// <summary>
    /// <inheritdoc/>
    /// Overrides the internal <see cref="ValidationRule{T, TExternalResources, TProperty}.ValidateAsync"/> method to provide
    /// the concrete asynchronous execution flow for custom rules. It checks for missing property values
    /// and then delegates to the abstract <see cref="IsValidAsync(IValidationCtx{T, TExternalResources}, TProperty, CancellationToken)"/> method.
    /// If <see cref="IsValidAsync"/> returns <c>false</c>, a validation failure is added to the context.
    /// </summary>
    /// <param name="context">The validator run context.</param>
    /// <param name="property">The <see cref="Undefined{TProperty}"/> wrapper containing the property value.</param>
    /// <param name="failureInfo">The failure information associated with this rule.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous validation operation.</returns>
    internal sealed override async ValueTask ValidateAsync(
        ValidatorRunCtx<T, TExternalResources> context,
        Undefined<TProperty> property,
        ValidationRuleFailureInfo<T, TExternalResources, TProperty> failureInfo,
        CancellationToken ct)
    {
        // If the property value is missing, force a stop (as the rule cannot validate an undefined value).
        if (!property.HasValue)
        {
            context.ForceStopPropertyValueIsMissing(property);
            return; // Exit early as validation cannot proceed without a value
        }

        // Execute the custom asynchronous validation logic.
        // ConfigureAwait(false) is used to prevent deadlocks in certain synchronization contexts.
        if (!await IsValidAsync(context, property.Value, ct).ConfigureAwait(false))
        {
            context.AddValidationRuleFailure(property, failureInfo);
        }
    }
}