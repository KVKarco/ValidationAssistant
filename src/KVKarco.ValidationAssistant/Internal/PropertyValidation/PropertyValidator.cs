using System.Collections.Immutable;

namespace KVKarco.ValidationAssistant.Internal.PropertyValidation;

/// <summary>
/// Provides an abstract base class for creating validation rules that operate on a specific property
/// within the main entity being validated. It extends <see cref="IValidatorComponent{T, TExternalResources, TContext}"/>
/// and manages a collection of individual <see cref="ValidationComponent{T, TExternalResources, TProperty}"/> instances
/// that apply to the property.
/// </summary>
/// <typeparam name="T">The type of the root entity being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources used during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property this rule is validating.</typeparam>
/// <typeparam name="TContext">The type of the validation run context.</typeparam>
internal abstract class PropertyValidator<T, TExternalResources, TProperty, TContext> :
    IValidatorComponent<T, TExternalResources, TContext>
    where TContext : ValidatorRunCtx<T, TExternalResources>
{
    private readonly ImmutableArray<ValidationRule<T, TExternalResources, TProperty>> _rules;
    private readonly ImmutableArray<ValidationRuleMetaData<T, TExternalResources, TProperty>> _rulesMetaData;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyValidator{T, TExternalResources, TProperty, TContext}"/> class.
    /// </summary>
    /// <param name="validationRules">
    /// A list of specific <see cref="ValidationComponent{T, TExternalResources, TProperty}"/> instances to apply to the property.
    /// </param>
    /// <param name="validationRulesMetaData">
    /// A list of corresponding <see cref="ValidationRuleMetaData{T, TExternalResources, TProperty}"/> instances for each validation rule.
    /// </param>
    protected PropertyValidator(
        List<ValidationRule<T, TExternalResources, TProperty>> validationRules,
        List<ValidationRuleMetaData<T, TExternalResources, TProperty>> validationRulesMetaData)
    {

    }

    /// <summary>
    /// Executes the synchronous property validation by iterating through the configured
    /// <see cref="ValidationComponent{T, TExternalResources, TProperty}"/> instances.
    /// Each rule is applied to the property, and the process continues until all rules are evaluated
    /// or the validation context indicates an early exit for this property rule.
    /// If <see cref="CanRunSynchronously"/> is <c>false</c>, calling this method will
    /// result in the validation process ending with a single failure and an explanation (inherited behavior).
    /// </summary>
    /// <param name="context">The validation run context.</param>
    public sealed override void Validate(TContext context, ValidatorComponentMetaData metaData)
    {
        StartValidation(context, metaData, out Undefined<TProperty> property);

        int index = 0;

        while (index < _rules.Length)
        {
            _rules[index].Validate(context, property, _rulesMetaData[index]);

            // Check if the current validation strategy or a critical failure
            // requires exiting further validation for this property rule.
            if (context.ToStopPropertyValidator())
            {
                break; // Exit the loop, stopping further property rule component validation.
            }

            index++;
        }

        if (context.IsPropertyValidatorValid)
        {
            // add info that the property validator ran successfully
        }
        else
        {
            // add info that the property validator failed
        }
    }

    /// <summary>
    /// Executes the asynchronous property validation by iterating through the configured
    /// <see cref="ValidationComponent{T, TExternalResources, TProperty}"/> instances.
    /// Each rule is applied asynchronously to the property, and the process continues until all rules are evaluated
    /// or the validation context indicates an early exit for this property rule.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous validation operation.</returns>
    public sealed override async ValueTask ValidateAsync(TContext context, in CancellationToken ct)
    {
        StartValidation(context, out Undefined<TProperty> property);

        int index = 0;

        while (index < _rules.Length)
        {
            ct.ThrowIfCancellationRequested(); // Check for cancellation before awaiting each validation rule.

            await _rules[index].ValidateAsync(context, property, _rulesMetaData[index], ct).ConfigureAwait(false);

            // Check if the current validation strategy or a critical failure
            // requires exiting further validation for this property rule.
            if (context.ToExitPropertyRule())
            {
                break; // Exit the loop, stopping further property rule component validation.
            }

            index++;
        }
    }

    /// <summary>
    /// Abstract method to be implemented by derived classes to perform specific setup for property validation,
    /// such as preparing the context for property-level validation and extracting the property's value.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <param name="property">An <see cref="Undefined{TProperty}"/> instance that will hold the extracted property value.</param>
    protected abstract void StartValidation(TContext context, ValidatorComponentMetaData metaData, out Undefined<TProperty> property);
}