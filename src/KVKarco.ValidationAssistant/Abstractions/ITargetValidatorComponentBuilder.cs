using KVKarco.ValidationAssistant.Abstractions.ValidationRules;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// A fluent builder for defining a set of validation rules for a specific property.
/// The rules within this set can influence the execution of subsequent rules upon failure.
/// </summary>
/// <typeparam name="TSubject">The type of the main validation subject.</typeparam>
/// <typeparam name="TResources">The type of external resources available to the validation process.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated (covariant).</typeparam>
public interface ITargetValidatorComponentBuilder<TSubject, TResources, out TProperty>
{
    /// <summary>
    /// Configures the flow impact on the overall validator when this component(rule set) is considered failed.
    /// This flow effect is triggered when one or more rules within this component fail.
    /// </summary>
    /// <param name="flowEffect">The flow effect to apply on failure.</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> WithFlowImpact(
        FlowEffect flowEffect);

    /// <summary>
    /// Sets the default flow effect for all rules within this component to use on failure.
    /// This value can be overridden on a per-rule basis.
    /// </summary>
    /// <param name="flowEffect">The default flow effect for all rules in the set.</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> WithDefaultRuleFlowEffect(
        FlowEffect flowEffect);

    /// <summary>
    /// Sets the default severity for all rules within this component.
    /// This value can be overridden on a per-rule basis.
    /// </summary>
    /// <param name="severity">The default severity for all rules in the set.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> WithDefaultRuleSeverity(
        Severity severity);

    /// <summary>
    /// Captures the current state of the validation flow for this rule set into a named snapshot.
    /// This snapshot can be referenced later to execute conditional logic.
    /// </summary>
    /// <param name="snapShot">The name of the snapshot to capture.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> Capture(
        string snapShot,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Executes the subsequent rules only if a previously captured snapshot evaluates to true.
    /// </summary>
    /// <param name="snapShot">The name of the snapshot to check against.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> ApplyWhen(
        string snapShot,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Executes the subsequent rules only if the provided synchronous condition evaluates to true.
    /// </summary>
    /// <param name="condition">A synchronous delegate that evaluates the condition.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> ApplyWhen(
        Condition<TSubject, TResources> condition,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Executes the subsequent rules only if the provided asynchronous condition evaluates to true.
    /// </summary>
    /// <param name="asyncCondition">An asynchronous delegate that evaluates the condition.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> ApplyWhenAsync(
        AsyncCondition<TSubject, TResources> asyncCondition,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Executes the subsequent rules only if a previously captured snapshot evaluates to false.
    /// </summary>
    /// <param name="snapShot">The name of the snapshot to check against.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> ApplyUnless(
        string snapShot,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Executes the subsequent rules only if the provided synchronous condition evaluates to false.
    /// Essentially, this means "if not".
    /// </summary>
    /// <param name="condition">A synchronous delegate that evaluates the condition.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> ApplyUnless(
        Condition<TSubject, TResources> condition,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Executes the subsequent rules only if the provided asynchronous condition evaluates to false.
    /// Essentially, this means "if not".
    /// </summary>
    /// <param name="asyncCondition">An asynchronous delegate that evaluates the condition.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> ApplyUnlessAsync(
        AsyncCondition<TSubject, TResources> asyncCondition,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Registers a simple synchronous validation rule. The rule passes if the predicate returns <c>true</c>.
    /// </summary>
    /// <param name="predicate">A synchronous predicate that validates the property value.</param>
    /// <param name="options">An optional delegate for configuring the rule's options.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> Ensure(
        Predicate<TProperty> predicate,
        Action<IValidationRuleOptionsBuilder<TSubject, TResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Registers a context-aware synchronous validation rule. The rule passes if the predicate returns <c>true</c>.
    /// </summary>
    /// <param name="predicate">A synchronous, context-aware predicate that validates the property value.</param>
    /// <param name="options">An optional delegate for configuring the rule's options.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> Ensure(
        Predicate<TSubject, TResources, TProperty> predicate,
        Action<IValidationRuleOptionsBuilder<TSubject, TResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Registers a context-aware asynchronous validation rule. The rule passes if the predicate returns <c>true</c>.
    /// </summary>
    /// <param name="predicate">An asynchronous, context-aware predicate that validates the property value.</param>
    /// <param name="options">An optional delegate for configuring the rule's options.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> EnsureAsync(
        AsyncPredicate<TSubject, TResources, TProperty> predicate,
        Action<IValidationRuleOptionsBuilder<TSubject, TResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Registers a predefined synchronous validation rule object. The rule's logic is encapsulated
    /// within the provided rule instance.
    /// </summary>
    /// <param name="rule">A synchronous validation rule instance.</param>
    /// <param name="options">An optional delegate for configuring the rule's options.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> EnsureRule(
        IValidationRule<TSubject, TResources, TProperty> rule,
        Action<IValidationRuleOptionsBuilder<TSubject, TResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Registers a predefined asynchronous validation rule object. The rule's logic is encapsulated
    /// within the provided asynchronous rule instance.
    /// </summary>
    /// <param name="rule">An asynchronous validation rule instance.</param>
    /// <param name="options">An optional delegate for configuring the rule's options.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> EnsureRuleAsync(
        IAsyncValidationRule<TSubject, TResources, TProperty> rule,
        Action<IValidationRuleOptionsBuilder<TSubject, TResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Registers a custom validator to be applied to the property as a single, composite rule.
    /// The validator instance is created via a factory function that resolves from the available resources.
    /// This allows for composing complex, reusable validation logic.
    /// </summary>
    /// <param name="validatorFactory">A factory function that creates a custom validator instance using the available resources.</param>
    /// <param name="options">An optional delegate for configuring the rule's options.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> EnsureValidator(
        Func<TResources, ICustomValidator<TProperty>> validatorFactory,
        Action<IValidationRuleOptionsBuilder<TSubject, TResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Registers a custom asynchronous validator to be applied to the property as a single, composite rule.
    /// The validator instance is created via a factory function that resolves from the available resources.
    /// This allows for composing complex, reusable asynchronous validation logic.
    /// </summary>
    /// <param name="validatorFactory">A factory function that creates a custom asynchronous validator instance using the available resources.</param>
    /// <param name="options">An optional delegate for configuring the rule's options.</param>
    /// <param name="callingFileLineNumber">The line number where this rule was defined (auto-populated).</param>
    /// <returns>The same <see cref="ITargetValidatorComponentBuilder{TSubject, TResources, TProperty}"/> instance for chaining.</returns>
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> EnsureValidatorAsync(
        Func<TResources, ICustomValidator<TProperty>> validatorFactory,
        Action<IValidationRuleOptionsBuilder<TSubject, TResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);
}


