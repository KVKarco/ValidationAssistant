using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.ValidationRules;
using System.Collections.Immutable;

namespace KVKarco.ValidationAssistant.Internal.PropertyValidation;

/// <summary>
/// Provides an abstract base class for creating validation rules that operate on a specific property
/// within the main entity being validated. It extends <see cref="ValidatorRule{T, TExternalResources, TContext}"/>
/// and manages a collection of individual <see cref="ValidationRule{T, TExternalResources, TProperty}"/> instances
/// that apply to the property.
/// </summary>
/// <typeparam name="T">The type of the root entity being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources used during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property this rule is validating.</typeparam>
/// <typeparam name="TContext">The type of the validation run context.</typeparam>
internal abstract class PropertyRule<T, TExternalResources, TProperty, TContext> :
    ValidatorRule<T, TExternalResources, TContext>
    where TContext : ValidatorRunCtx<T, TExternalResources>
{
    private readonly ImmutableArray<ValidationRule<T, TExternalResources, TProperty>> _validationRules;
    private readonly ImmutableArray<ValidationRuleFailureInfo<T, TExternalResources, TProperty>> _validationRulesFailureInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyRule{T, TExternalResources, TProperty, TContext}"/> class.
    /// </summary>
    /// <param name="validatorName">The name of the validator declaring this property rule.</param>
    /// <param name="strategy">The <see cref="RuleFailureStrategy"/> to apply for this property rule (e.g., continue on first failure, stop on first failure).</param>
    /// <param name="declaredOnLine">The line number in the source code where this property rule was declared, for debugging/logging.</param>
    /// <param name="validationRules">A list of specific <see cref="ValidationRule{T, TExternalResources, TProperty}"/> instances to apply to the property.</param>
    /// <param name="validationRulesFailureInfo">A list of corresponding <see cref="ValidationRuleFailureInfo{T, TExternalResources, TProperty}"/> instances for each validation rule.</param>
    /// <exception cref="ValidationAssistantInternalException">
    /// Thrown if the count of <paramref name="validationRules"/> does not match the count of <paramref name="validationRulesFailureInfo"/>.
    /// This indicates an internal configuration error.
    /// </exception>
    protected PropertyRule(
        ReadOnlySpan<char> validatorName,
        RuleFailureStrategy strategy,
        int declaredOnLine,
        List<ValidationRule<T, TExternalResources, TProperty>> validationRules,
        List<ValidationRuleFailureInfo<T, TExternalResources, TProperty>> validationRulesFailureInfo)
        : base(!validationRules.Exists(x => !x.CanRunSynchronously))
    {
        if (validationRules.Count != validationRulesFailureInfo.Count)
        {
            throw new ValidationAssistantInternalException("Debug how validation rules and their failure info count is not the some.");
        }
        Info = new PropertyRuleFailureInfo<T, TExternalResources, TProperty>(
            (context, _) => ValidatorsConfig.GlobalDefaults.Messages.PropertyValueMissingExplanation(context),
            validatorName, RuleName, declaredOnLine, strategy);
        _validationRules = [.. validationRules];
        _validationRulesFailureInfo = [.. validationRulesFailureInfo];
    }

    /// <inheritdoc/>
    /// <summary>
    /// <inheritdoc cref="ValidatorRule{T, TExternalResources, TContext}.RuleName"/>
    /// This property rule's name is dynamically set to either <see cref="DefaultNaming.PropertyRule"/>
    /// or <see cref="DefaultNaming.PropertyAsyncRule"/> based on its synchronous capability.
    /// It is not guaranteed to be unique globally.
    /// </summary>
    public sealed override ReadOnlySpan<char> RuleName => CanRunSynchronously ? DefaultNaming.PropertyRule : DefaultNaming.PropertyAsyncRule;

    /// <inheritdoc/>
    public sealed override PropertyRuleFailureInfo<T, TExternalResources, TProperty> Info { get; }

    /// <summary>
    /// Executes the synchronous property validation by iterating through the configured
    /// <see cref="ValidationRule{T, TExternalResources, TProperty}"/> instances.
    /// Each rule is applied to the property, and the process continues until all rules are evaluated
    /// or the validation context indicates an early exit for this property rule.
    /// If <see cref="CanRunSynchronously"/> is <c>false</c>, calling this method will
    /// result in the validation process ending with a single failure and an explanation (inherited behavior).
    /// </summary>
    /// <param name="context">The validation run context.</param>
    public sealed override void Validate(TContext context)
    {
        StartValidation(context, out Undefined<TProperty> property);

        int index = 0;

        while (index < _validationRules.Length)
        {
            _validationRules[index].Validate(context, property, _validationRulesFailureInfo[index]);

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
    /// Executes the asynchronous property validation by iterating through the configured
    /// <see cref="ValidationRule{T, TExternalResources, TProperty}"/> instances.
    /// Each rule is applied asynchronously to the property, and the process continues until all rules are evaluated
    /// or the validation context indicates an early exit for this property rule.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous validation operation.</returns>
    public sealed override async ValueTask ValidateAsync(TContext context, CancellationToken ct)
    {
        StartValidation(context, out Undefined<TProperty> property);

        int index = 0;

        while (index < _validationRules.Length)
        {
            ct.ThrowIfCancellationRequested(); // Check for cancellation before awaiting each validation rule.

            await _validationRules[index].ValidateAsync(context, property, _validationRulesFailureInfo[index], ct).ConfigureAwait(false);

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
    protected abstract void StartValidation(TContext context, out Undefined<TProperty> property);
}