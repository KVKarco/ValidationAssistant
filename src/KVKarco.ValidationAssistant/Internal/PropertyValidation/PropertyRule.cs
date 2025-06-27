using System.Collections.Immutable;

namespace KVKarco.ValidationAssistant.Internal.PropertyValidation;

/// <summary>
/// Represents an abstract base class for a validation rule that operates on a specific property of the main instance.
/// This rule acts as a collection wrapper for individual <see cref="PropertyRuleComponent{T, TExternalResources, TProperty}"/>
/// instances, executing them sequentially for the target property.
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property this rule applies to.</typeparam>
/// <typeparam name="TContext">The specific type of <see cref="ValidatorRunCtx{T, TExternalResources}"/>
/// used for the validation run, ensuring context-specific operations.</typeparam>
internal abstract class PropertyRule<T, TExternalResources, TProperty, TContext> :
    ValidatorRule<T, TExternalResources, TContext>
    where TContext : ValidatorRunCtx<T, TExternalResources>
{
    /// <summary>
    /// A read-only collection of <see cref="PropertyRuleComponent{T, TExternalResources, TProperty}"/> instances
    /// that define the individual validation checks to be performed on this property.
    /// Each component encapsulates a synchronous or asynchronous validation logic along with its configuration.
    /// </summary>
    private readonly ImmutableArray<PropertyRuleComponent<T, TExternalResources, TProperty>> _ruleComponents;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyRule{T, TExternalResources, TProperty, TContext}"/> class.
    /// </summary>
    /// <param name="validatorName">The name of the validator to which this property rule belongs.</param>
    /// <param name="strategy">The <see cref="RuleFailureStrategy"/> to apply if this property rule fails.</param>
    /// <param name="declaredOnLine">The line number where this property rule was declared, used for debugging or reporting.</param>
    /// <param name="ruleComponents">A list of <see cref="PropertyRuleComponent{T, TExternalResources, TProperty}"/> instances
    /// that define the checks for this property.</param>
    protected PropertyRule(
        ReadOnlySpan<char> validatorName,
        RuleFailureStrategy strategy,
        int declaredOnLine,
        List<PropertyRuleComponent<T, TExternalResources, TProperty>> ruleComponents)
        // The base constructor (ValidatorRule) is called with 'true' if all contained rule components can run synchronously,
        // otherwise 'false' indicating that asynchronous execution is required.
        // It checks if no component exists that cannot run synchronously.
        : base(!ruleComponents.Exists(x => !x.CanRunSynchronously))
    {
        // Initializes the PropertyRuleFailureInfo for this property rule.
        // It uses a global default explanation factory if no specific one is provided.
        Info = new PropertyRuleFailureInfo<T, TExternalResources, TProperty>(
            (context, _) => ValidatorsConfig.GlobalDefaults.Messages.PropertyValueMissingExplanation(context),
            validatorName, RuleName, declaredOnLine, RuleFailureStrategy.Continue);
        _ruleComponents = [.. ruleComponents]; // Converts the list of components to an immutable array.
    }

    /// <summary>
    /// Gets the unique name of this property rule. It dynamically indicates whether the rule is synchronous or asynchronous
    /// based on the <see cref="ValidatorRule{T, TExternalResources, TContext}.CanRunSynchronously"/> property.
    /// This is a sealed override to ensure consistent naming for all property rules.
    /// </summary>
    public sealed override ReadOnlySpan<char> RuleName => CanRunSynchronously ? DefaultNaming.PropertyRule : DefaultNaming.PropertyAsyncRule;

    /// <summary>
    /// Gets the detailed failure information for this property rule, including its explanation factory,
    /// and strategy. This is a sealed override to ensure consistent failure reporting for property rules
    /// and provides a specific <see cref="PropertyRuleFailureInfo{T, TExternalResources, TProperty}"/> type.
    /// </summary>
    public sealed override PropertyRuleFailureInfo<T, TExternalResources, TProperty> Info { get; }

    /// <summary>
    /// Synchronously validates the property by iterating through its associated <see cref="PropertyRuleComponent{T, TExternalResources, TProperty}"/> instances.
    /// Validation stops if a component's failure strategy dictates an exit for the property rule
    /// (via <see cref="ValidatorRunCtx{T, TExternalResources}.ToExitPropertyRule()"/>).
    /// </summary>
    /// <param name="context">The validation run context containing the instance being validated and external resources.</param>
    public sealed override void Validate(TContext context)
    {
        // Attempts to extract the property value. If it doesn't exist or shouldn't be validated (as per HasValue logic),
        // the property rule components are not executed.
        if (HasValue(context, out Undefined<TProperty> property))
        {
            int index = 0;

            // Iterates through each property rule component.
            while (index < _ruleComponents.Length)
            {
                // Executes the synchronous validation logic of the current component.
                _ruleComponents[index].Validate(context, property);

                // Checks if the validation context indicates that the current property rule execution should stop.
                if (context.ToExitPropertyRule())
                {
                    break; // Exit the loop if the strategy dictates.
                }

                index++;
            }
        }
    }

    /// <summary>
    /// Asynchronously validates the property by iterating through its associated <see cref="PropertyRuleComponent{T, TExternalResources, TProperty}"/> instances.
    /// Validation stops if a component's failure strategy dictates an exit for the property rule
    /// (via <see cref="ValidatorRunCtx{T, TExternalResources}.ToExitPropertyRule()"/>).
    /// Supports cancellation via <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="context">The validation run context containing the instance being validated and external resources.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the validation to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public sealed override async ValueTask ValidateAsync(TContext context, CancellationToken ct)
    {
        // Attempts to extract the property value. If it doesn't exist or shouldn't be validated,
        // the property rule components are not executed.
        if (HasValue(context, out Undefined<TProperty> property))
        {
            int index = 0;

            // Iterates through each property rule component.
            while (index < _ruleComponents.Length)
            {
                // Throws an OperationCanceledException if cancellation is requested.
                ct.ThrowIfCancellationRequested();

                // Executes the asynchronous validation logic of the current component.
                await _ruleComponents[index].ValidateAsync(context, property, ct).ConfigureAwait(false);

                // Checks if the validation context indicates that the current property rule execution should stop.
                if (context.ToExitPropertyRule())
                {
                    break; // Exit the loop if the strategy dictates.
                }

                index++;
            }
        }
    }

    /// <summary>
    /// When overridden in a derived class, this method is responsible for extracting the property's value
    /// from the main instance within the given validation context.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <param name="property">When this method returns <see langword="true"/>, contains the extracted
    /// property value wrapped in an <see cref="Undefined{TProperty}"/> instance. This parameter will
    /// never be <see langword="null"/>; it will always contain a valid <see cref="Undefined{TProperty}"/>
    /// instance representing either a defined value or an undefined state.</param>
    /// <returns><see langword="true"/> if the property value could be successfully extracted and is available for validation;
    /// otherwise, <see langword="false"/> (e.g., if the property is null and not meant for validation in this scenario,
    /// or if the property's accessor threw an exception and the rule is designed to prevent further component execution).</returns>
    protected abstract bool HasValue(TContext context, out Undefined<TProperty> property);
}
