using KVKarco.ValidationAssistant.ValidationRules;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// Defines the fluent API for the initial configuration of a property validation rule.
/// This interface allows setting **rule-specific default failure strategies**, overriding global defaults.
/// These settings must be applied before any validation components are added to the rule.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the rule is being built.</typeparam>
/// <remarks>
/// Once a validation or conditional component is added, these initial configuration methods are no longer available.
/// </remarks>
public interface IInitialPropertyRuleBuilder<T, TExternalResources, out TProperty> :
    IPropertyRuleBuilder<T, TExternalResources, TProperty>
{
    /// <summary>
    /// Sets the failure strategy for **this PropertyRule** to <c>Continue</c>.
    /// If this rule fails, the validator will continue to evaluate other top-level PropertyRules.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// This overrides any global default rule failure strategy. It affects only this PropertyRule's
    /// relation to other top-level rules, not its internal components.
    /// </remarks>
    IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> OnFailureContinue();

    /// <summary>
    /// Sets the failure strategy for **this PropertyRule** to <c>Stop</c>.
    /// If this rule fails, the validator will immediately cease all further evaluation of top-level rules.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// This overrides any global default rule failure strategy. It affects only this PropertyRule's
    /// relation to other top-level rules. Note that an individual component set to <see cref="OnComponentFailureStop"/>
    /// can still cause an immediate global stop regardless of this setting.
    /// </remarks>
    IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> OnFailureStop();

    /// <summary>
    /// Sets the default failure strategy for **validation components within this PropertyRule** to <c>Continue</c>.
    /// If a component in this rule fails, its errors are collected, but subsequent components in this rule's chain are still evaluated.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// This overrides any global default component failure strategy. Applies only to validation components
    /// (e.g., <c>Ensure</c>, <c>UseRule</c>), not conditional components (e.g., <c>ContinueWhen</c>).
    /// </remarks>
    IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> OnComponentFailureContinue();

    /// <summary>
    /// Sets the default failure strategy for **validation components within this PropertyRule** to <c>Exit</c>.
    /// If a component in this rule fails, its errors are collected, and subsequent components *within this same PropertyRule* are skipped.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// This overrides any global default component failure strategy. After exiting the component chain,
    /// this PropertyRule's own rule-level strategy determines further validation flow.
    /// Applies only to validation components.
    /// </remarks>
    IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> OnComponentFailureExit();

    /// <summary>
    /// Sets the default failure strategy for **validation components within this PropertyRule** to <c>Stop</c>.
    /// If any component in this rule fails, its errors are collected, and the validator will **immediately cease all further validation** for the entire object.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// This acts as a powerful "Fail Fast" mechanism triggered by any component failure within this PropertyRule
    /// and **overrides** any rule-level strategy (<see cref="OnFailureContinue"/> or <see cref="OnFailureStop"/>)
    /// set for this PropertyRule.
    /// Applies only to validation components.
    /// </remarks>
    IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> OnComponentFailureStop();
}

/// <summary>
/// Defines the core fluent API for building and configuring validation rules for a specific property.
/// This interface combines capabilities for adding both conditional (flow control) and validation (actual checks) components.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the rule is being built.</typeparam>
/// <remarks>
/// This interface consolidates functionalities from <see cref="IPropertyRuleConditionComponentBuilder{T, TExternalResources, TProperty}"/>
/// and <see cref="IPropertyRuleValidationComponentBuilder{T, TExternalResources, TProperty}"/>, enabling seamless fluent chaining.
/// </remarks>
public interface IPropertyRuleBuilder<T, TExternalResources, out TProperty> :
    IPropertyRuleConditionComponentBuilder<T, TExternalResources, TProperty>,
    IPropertyRuleValidationComponentBuilder<T, TExternalResources, TProperty>
{
}


/// <summary>
/// Defines fluent methods for adding **conditional components** to a property rule.
/// These components control the **flow of rule execution based on conditions**, rather than causing direct validation failures.
/// </summary>
/// <remarks>
/// Conditional components do not cause validation failures. Their primary purpose is to determine
/// if subsequent parts of the rule should run. If a condition is not met, an optional <paramref name="failureInfoMessage"/>
/// can provide context as to why the rule was skipped.
/// The default strategy for <c>ContinueWhen</c> components is always <see cref="ComponentFailureStrategy.Exit"/>.
/// </remarks>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the rule is being built.</typeparam>
public interface IPropertyRuleConditionComponentBuilder<T, TExternalResources, out TProperty>
{
    /// <summary>
    /// Captures the current validation state as a named **snapshot**.
    /// Used by <see cref="ContinueWhen(string, string?, int)"/> to conditionally execute later rules based on prior validity.
    /// </summary>
    /// <param name="snapShotIdentifier">A unique string identifier for this snapshot point.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IPropertyRuleBuilder<T, TExternalResources, TProperty> CaptureSnapShot(
        string snapShotIdentifier,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Continues rule execution **only if** a previously defined <paramref name="snapShotIdentifier"/> is valid.
    /// If the snapshot is invalid, subsequent components in this rule are skipped.
    /// </summary>
    /// <param name="snapShotIdentifier">The unique identifier of the snapshot to check.</param>
    /// <param name="failureInfoMessage">Optional informational message if the snapshot is invalid (rule skipped).</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// Use this method when your conditional logic affects **only this specific property rule**.
    /// If the condition needs to affect **multiple rules or an entire block of rules**, use the
    /// broader <see cref="IConditionalFlowRuleBuilder{T, TExternalResources}.UseWhen"/> or
    /// <see cref="IConditionalFlowRuleBuilder{T, TExternalResources}.UseWhenAsync"/> methods.
    /// </remarks>
    IPropertyRuleBuilder<T, TExternalResources, TProperty> ContinueWhen(
        string snapShotIdentifier,
        string? failureInfoMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Continues rule execution **only if** the synchronous <paramref name="condition"/> returns <see langword="true"/>.
    /// If the condition is not met, subsequent components in this rule are skipped.
    /// </summary>
    /// <param name="condition">A synchronous predicate (context and property value) to determine continuation.</param>
    /// <param name="failureInfoMessage">Optional informational message if the condition is not met (rule skipped).</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// Use this method when your conditional logic affects **only this specific property rule**.
    /// If the condition needs to affect **multiple rules or an entire block of rules**, use the
    /// broader <see cref="IConditionalFlowRuleBuilder{T, TExternalResources}.UseWhen"/> or
    /// <see cref="IConditionalFlowRuleBuilder{T, TExternalResources}.UseWhenAsync"/> methods.
    /// </remarks>
    IPropertyRuleBuilder<T, TExternalResources, TProperty> ContinueWhen(
        ValidationCondition<T, TExternalResources> condition,
        string? failureInfoMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Continues rule execution **only if** the asynchronous <paramref name="asyncCondition"/> returns <see langword="true"/>.
    /// If the condition is not met, subsequent components in this rule are skipped.
    /// </summary>
    /// <param name="asyncCondition">An asynchronous predicate (context and property value) to determine continuation.</param>
    /// <param name="failureInfoMessage">Optional informational message if the condition is not met (rule skipped).</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// Use this method when your conditional logic affects **only this specific property rule**.
    /// If the condition needs to affect **multiple rules or an entire block of rules**, use the
    /// broader <see cref="IConditionalFlowRuleBuilder{T, TExternalResources}.UseWhen"/> or
    /// <see cref="IConditionalFlowRuleBuilder{T, TExternalResources}.UseWhenAsync"/> methods.
    /// </remarks>
    IPropertyRuleBuilder<T, TExternalResources, TProperty> ContinueWhenAsync(
        AsyncValidationCondition<T, TExternalResources> asyncCondition,
        string? failureInfoMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);
}

/// <summary>
/// Defines fluent methods for adding *validation components* to a property rule.
/// These components are predicates, rules, or other validation logic that perform
/// actual checks on the property's value.
/// </summary>
/// <remarks>
/// Default failure strategies can be set for the validation component, which will
/// override the global default strategies set in the validator or global configuration.
/// Severity can be set for the validation component, which will override the default
/// <see cref="FailureSeverity.Error"/> severity.
/// The failure message can be set for the validation component, which will override
/// the default failure message provided by the rule or predicate.
/// If severity is set to <see cref="FailureSeverity.Info"/>, failure of the component
/// will not cause a real validation failure, but will be reported as an informational message.
/// </remarks>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the rule is being built.</typeparam>
public interface IPropertyRuleValidationComponentBuilder<T, TExternalResources, out TProperty>
{
    /// <summary>
    /// Adds a synchronous validation component that checks the property value against a simple predicate.
    /// </summary>
    /// <param name="predicate">A synchronous function that takes the property value and returns <see langword="true"/> if valid, <see langword="false"/> otherwise.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called.</param>
    /// <returns>An <see cref="IValidationComponentFailureOptionsBuilder{T, TExternalResources, TProperty}"/> for further configuration of this component's failure behavior.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> Ensure(
        PropertyPredicate<TProperty> predicate,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Adds an asynchronous validation component that checks the property value against a simple asynchronous predicate.
    /// </summary>
    /// <param name="asyncPredicate">An asynchronous function that takes the property value and returns a <see cref="ValueTask{Boolean}"/> indicating validity.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called.</param>
    /// <returns>An <see cref="IValidationComponentFailureOptionsBuilder{T, TExternalResources, TProperty}"/> for further configuration of this component's failure behavior.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> EnsureAsync(
        AsyncPropertyPredicate<TProperty> asyncPredicate,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Adds a synchronous validation component that checks the property value against a context-aware predicate.
    /// The predicate has access to the main instance and external resources.
    /// </summary>
    /// <param name="predicate">A synchronous function that takes the validation context and property value, returning <see langword="true"/> if valid.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called.</param>
    /// <returns>An <see cref="IValidationComponentFailureOptionsBuilder{T, TExternalResources, TProperty}"/> for further configuration of this component's failure behavior.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> Ensure(
        ContextAwarePropertyPredicate<T, TExternalResources, TProperty> predicate,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Adds an asynchronous validation component that checks the property value against a context-aware asynchronous predicate.
    /// The predicate has access to the main instance and external resources.
    /// </summary>
    /// <param name="asyncPredicate">An asynchronous function that takes the validation context and property value, returning a <see cref="ValueTask{Boolean}"/> indicating validity.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called.</param>
    /// <returns>An <see cref="IValidationComponentFailureOptionsBuilder{T, TExternalResources, TProperty}"/> for further configuration of this component's failure behavior.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> EnsureAsync(
        AsyncContextAwarePropertyPredicate<T, TExternalResources, TProperty> asyncPredicate,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Adds a custom synchronous validation rule (<see cref="IValidationRule{T, TExternalResources, TProperty}"/>) as a component.
    /// </summary>
    /// <param name="rule">The custom synchronous validation rule instance.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called.</param>
    /// <returns>An <see cref="IValidationComponentFailureOptionsBuilder{T, TExternalResources, TProperty}"/> for further configuration of this component's failure behavior.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> UseRule(
        IValidationRule<T, TExternalResources, TProperty> rule,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Adds a custom asynchronous validation rule (<see cref="IAsyncValidationRule{T, TExternalResources, TProperty}"/>) as a component.
    /// </summary>
    /// <param name="asyncRule">The custom asynchronous validation rule instance.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called.</param>
    /// <returns>An <see cref="IValidationComponentFailureOptionsBuilder{T, TExternalResources, TProperty}"/> for further configuration of this component's failure behavior.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> UseRuleAsync(
        IAsyncValidationRule<T, TExternalResources, TProperty> asyncRule,
        [CallerLineNumber] int callingFileLineNumber = 0);
}

/// <summary>
/// Defines fluent methods for configuring the **failure options of the most recently added validation component**.
/// This includes setting its failure strategy, custom message, and severity.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the rule is being built.</typeparam>
/// <remarks>
/// This interface allows fine-grained control over a specific component's behavior upon failure.
/// It inherits from <see cref="IPropertyRuleBuilder{T, TExternalResources, TProperty}"/>, allowing continued chaining.
/// </remarks>
public interface IValidationComponentFailureOptionsBuilder<T, TExternalResources, out TProperty> :
    IPropertyRuleBuilder<T, TExternalResources, TProperty>
{
    /// <summary>
    /// Sets the failure strategy for this component to <c>Continue</c>.
    /// If this component fails, errors are collected, but subsequent components in this PropertyRule chain are still evaluated.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// Control returns to the parent PropertyRule, which then applies its own rule-level strategy.
    /// </remarks>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> OnFailureContinue();

    /// <summary>
    /// Sets the failure strategy for this component to <c>Exit</c>.
    /// If this component fails, errors are collected, and subsequent components *within this PropertyRule* are skipped.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// Control returns to the parent PropertyRule, which then applies its own rule-level strategy.
    /// </remarks>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> OnFailureExit();

    /// <summary>
    /// Sets the failure strategy for this component to <c>Stop</c>.
    /// If this component fails, errors are collected, and the validator will **immediately cease all further validation** for the entire object.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// This acts as a powerful "Fail Fast" mechanism and overrides any rule-level strategy for the parent PropertyRule.
    /// </remarks>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> OnFailureStop();

    /// <summary>
    /// Sets the severity of this component's failure to <see cref="FailureSeverity.Error"/> (the default).
    /// Indicates a standard validation failure.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> SeverityError();

    /// <summary>
    /// Sets the severity of this component's failure to <see cref="FailureSeverity.Warning"/>.
    /// Indicates a non-critical issue that is reported but does not necessarily mark validation as invalid.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> SeverityWarning();

    /// <summary>
    /// Sets the severity of this component's failure to <see cref="FailureSeverity.Info"/>.
    /// An informational message; does **not** represent a validation failure.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    /// <remarks>
    /// If severity is Info, this component's "failure" will **not** trigger any stop or exit strategies.
    /// Validation will always proceed, merely logging the informational message.
    /// </remarks>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> SeverityInfo();

    /// <summary>
    /// Sets a **static custom failure message** for this validation component.
    /// This message will be used if the component fails, overriding any default.
    /// </summary>
    /// <param name="failureMessage">The static string message to use.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> WithMessage(
        string failureMessage);

    /// <summary>
    /// Sets a **dynamic custom failure message factory** for this validation component.
    /// The factory function will be invoked if the component fails to generate the message at runtime,
    /// allowing for context-specific data in the message.
    /// </summary>
    /// <param name="failureMessageFactory">A function that generates the message, providing context and property value.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> WithMessage(
        FailureMessageFactory<T, TExternalResources, TProperty> failureMessageFactory);
}