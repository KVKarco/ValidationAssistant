using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// Defines a foundational, generic contract for building and configuring any type of validator
/// within the ValidationAssistant framework. It provides common capabilities for validator construction.
/// </summary>
/// <typeparam name="T">The type of the instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <remarks>
/// <para>
/// This interface extends <see cref="IConditionalFlowRuleBuilder{T, TExternalResources}"/>,
/// enabling the definition of rules that are conditionally applied.
/// </para>
/// <para>
/// It also introduces methods for setting default failure strategies at both the PropertyRule
/// and component levels. These defaults can be overridden by explicit configurations on individual rules.
/// </para>
/// </remarks>
public interface IValidationDefinitionBuilder<T, TExternalResources> :
    IConditionalFlowRuleBuilder<T, TExternalResources>
{
    /// <summary>
    /// Initiates the definition of validation rules for a specific property of the instance <typeparamref name="T"/>.
    /// This is the starting point for defining a *single PropertyRule* composed of sequential validation components.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being selected for validation.</typeparam>
    /// <param name="propertySelector">An expression that selects the property (e.g., <c>x => x.PropertyName</c>).</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number.</param>
    /// <returns>
    /// An <see cref="IInitialPropertyRuleBuilder{T, TExternalResources, TProperty}"/> for further fluent rule definition.
    /// </returns>
    /// <remarks>
    /// Use this method to chain property-specific rules like <c>.NotNull()</c> or <c>.Length()</c>.
    /// A PropertyRule's failure strategy determines if subsequent PropertyRules are executed.
    /// </remarks>
    IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> UseFor<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Sets the default behavior for all subsequent PropertyRules to continue evaluating
    /// other rules even if a PropertyRule fails.
    /// </summary>
    /// <remarks>
    /// This sets the default <c>RuleFailureStrategy</c> to <c>Continue</c>.
    /// This default can be overridden by explicit strategies on individual PropertyRules.
    /// It does not affect individual components within a PropertyRule, nor conditional flow rules.
    /// </remarks>
    void OnRuleFailureContinue();

    /// <summary>
    /// Sets the default behavior for all subsequent PropertyRules to stop validation
    /// immediately if any PropertyRule fails.
    /// </summary>
    /// <remarks>
    /// This sets the default <c>RuleFailureStrategy</c> to <c>Stop</c>.
    /// This default can be overridden by explicit strategies on individual PropertyRules.
    /// It does not affect individual components within a PropertyRule, nor conditional flow rules.
    /// </remarks>
    void OnRuleFailureStop();

    /// <summary>
    /// Sets the default behavior for all subsequent validation components within any PropertyRule
    /// to continue evaluating other components in the same chain, even if a component fails.
    /// </summary>
    /// <remarks>
    /// This sets the default <c>ComponentFailureStrategy</c> to <c>Continue</c>.
    /// This default can be overridden by explicit strategies on individual components.
    /// </remarks>
    void OnComponentFailureContinue();

    /// <summary>
    /// Sets the default behavior for all subsequent validation components within any PropertyRule
    /// to stop evaluating the rest of the rules in that specific PropertyRule chain if a component fails.
    /// </summary>
    /// <remarks>
    /// This sets the default <c>ComponentFailureStrategy</c> to <c>Exit</c>.
    /// This default can be overridden by explicit strategies on individual components.
    /// After exiting the component chain, the PropertyRule's own failure strategy applies.
    /// </remarks>
    void OnComponentFailureExit();

    /// <summary>
    /// Sets the default behavior for all subsequent validation components within any PropertyRule
    /// to stop the *entire validation process immediately* if any component fails.
    /// </summary>
    /// <remarks>
    /// This sets the default <c>ComponentFailureStrategy</c> to <c>Stop</c>.
    /// This default acts as a global "Fail Fast" mechanism and **overrides** any
    /// <c>RuleFailureStrategy</c> on the parent PropertyRule.
    /// </remarks>
    void OnComponentFailureStop();
}