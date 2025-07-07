using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// Defines the fluent API for the initial configuration of a property validation rule.
/// This interface allows setting default failure strategies for the rule itself and its
/// components, overriding global defaults. These settings can only be applied at the
/// very beginning of the rule definition, before any validation components are added.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the rule is being built.</typeparam>
public interface IInitialPropertyRuleBuilder<T, TExternalResources, out TProperty> :
    IPropertyRuleBuilder<T, TExternalResources, TProperty>
{
    /// <summary>
    /// Sets the default <see cref="ComponentFailureStrategy"/> and <see cref="FailureSeverity"/>
    /// for all subsequent validation components added to this property rule.
    /// This overrides any global default strategies set in the validator or global configuration.
    /// This method can only be used at the start of the rule definition, before any components are added.
    /// Strategies and severity do not apply to conditional components, as they have their own flow control.
    /// </summary>
    /// <param name="onFailure">The default strategy to apply when a component fails.</param>
    /// <param name="severity">The default severity level for component failures. Defaults to <see cref="FailureSeverity.Error"/>.</param>
    /// <returns>The current initial property rule builder for fluent chaining.</returns>
    IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> DefaultOnComponentFailure(
        ComponentFailureStrategy onFailure,
        FailureSeverity severity = FailureSeverity.Error);

    /// <summary>
    /// Sets the default <see cref="RuleFailureStrategy"/> for this property rule.
    /// This overrides any global default strategy set in the validator or global configuration.
    /// This method can only be used at the start of the rule definition, before any components are added.
    /// </summary>
    /// <param name="onFailure">The default strategy to apply when the rule itself fails (i.e., when a component within it causes a rule-level failure).</param>
    /// <returns>The current initial property rule builder for fluent chaining.</returns>
    IInitialPropertyRuleBuilder<T, TExternalResources, TProperty> DefaultOnRuleFailure(
        RuleFailureStrategy onFailure);

}

/// <summary>
/// Defines the core fluent API for building and configuring validation rules for a specific property.
/// This interface combines capabilities for adding both conditional components (which control rule flow)
/// and validation components (which perform actual validation checks).
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the rule is being built.</typeparam>
public interface IPropertyRuleBuilder<T, TExternalResources, out TProperty> :
    IPropertyRuleConditionComponentBuilder<T, TExternalResources, TProperty>,
    IPropertyRuleValidationComponentBuilder<T, TExternalResources, TProperty>
{

}


/// <summary>
/// Defines fluent methods for adding *conditional components* to a property rule.
/// These components control the flow of the rule's execution based on conditions,
/// rather than causing direct validation failures. They are convenient for complex
/// rules with multiple conditions instead of wrapping the entire rule in a <c>UseWhen</c> clause
/// if the logic affects only a single rule.
/// </summary>
/// <remarks>
/// Conditional components do not cause real validation failures. Their primary purpose is
/// to determine whether subsequent parts of the rule should be executed.
/// Only a <see cref="failureInfoMessage"/> can be overridden for these components,
/// as the condition itself is not a validation rule and does not have a typical failure message.
/// The message is used to provide context when the condition is not met, allowing the user
/// to understand why the rule was not executed.
/// <para>
/// The default failure strategy for conditional components is always <see cref="ComponentFailureStrategy.Exit"/>,
/// except for the <see cref="CaptureSnapShot"/> method, which does not have a failure strategy as it does not perform validation.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the rule is being built.</typeparam>
public interface IPropertyRuleConditionComponentBuilder<T, TExternalResources, out TProperty>
{
    /// <summary>
    /// Adds a component that captures the current validation state as a snapshot.
    /// If any subsequent rule or component (up to the next snapshot or end of validation) fails,
    /// this snapshot will be marked as invalid. This allows for conditional execution of later rules
    /// based on the validity of a specific section of the validation.
    /// </summary>
    /// <param name="snapShotIdentifier">A unique string identifier for this snapshot point.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called.</param>
    /// <returns>The current property rule builder for fluent chaining.</returns>
    IPropertyRuleBuilder<T, TExternalResources, TProperty> CaptureSnapShot(
        string snapShotIdentifier,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Adds a conditional component that allows the rule to continue execution *only if*
    /// a previously defined snapshot is valid. If the snapshot is invalid, the rule (or its subsequent components)
    /// will be skipped.
    /// </summary>
    /// <param name="snapShotIdentifier">The unique identifier of the snapshot to check.</param>
    /// <param name="failureInfoMessage">An optional informational message if the condition is not met (i.e., the snapshot is invalid).</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called.</param>
    /// <returns>The current property rule builder for fluent chaining.</returns>
    IPropertyRuleBuilder<T, TExternalResources, TProperty> ContinueWhen(
        string snapShotIdentifier,
        string? failureInfoMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Adds a synchronous conditional component that allows the rule to continue execution *only if*
    /// the provided <paramref name="condition"/> predicate returns <see langword="true"/>.
    /// If the condition is not met, the rule (or its subsequent components) will be skipped.
    /// </summary>
    /// <param name="condition">A synchronous predicate that determines whether to continue execution.</param>
    /// <param name="failureInfoMessage">An optional informational message if the condition is not met.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called.</param>
    /// <returns>The current property rule builder for fluent chaining.</returns>
    IPropertyRuleBuilder<T, TExternalResources, TProperty> ContinueWhen(
        ValidationCondition<T, TExternalResources> condition,
        string? failureInfoMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Adds an asynchronous conditional component that allows the rule to continue execution *only if*
    /// the provided <paramref name="asyncCondition"/> predicate returns <see langword="true"/>.
    /// If the condition is not met, the rule (or its subsequent components) will be skipped.
    /// </summary>
    /// <param name="asyncCondition">An asynchronous predicate that determines whether to continue execution.</param>
    /// <param name="failureInfoMessage">An optional informational message if the condition is not met.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file where this method is called.</param>
    /// <returns>The current property rule builder for fluent chaining.</returns>
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
/// Defines fluent methods for configuring the failure options of the *most recently added*
/// validation component within a property rule. This includes setting the component's
/// failure strategy, custom message, and severity.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <typeparam name="TProperty">The type of the property for which the rule is being built.</typeparam>
public interface IValidationComponentFailureOptionsBuilder<T, TExternalResources, out TProperty> :
    IPropertyRuleBuilder<T, TExternalResources, TProperty>
{
    /// <summary>
    /// Sets the <see cref="ComponentFailureStrategy"/> for the most recently added validation component.
    /// This overrides the default component failure strategy for this specific component.
    /// </summary>
    /// <param name="onFailure">The strategy to apply when this component fails.</param>
    /// <returns>The current validation component failure options builder for fluent chaining.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> OnFailure(
        ComponentFailureStrategy onFailure);

    /// <summary>
    /// Sets a static custom failure message for the most recently added validation component.
    /// This message will be used if the component fails, overriding any default message.
    /// </summary>
    /// <param name="failureMessage">The static string message to use.</param>
    /// <returns>The current validation component failure options builder for fluent chaining.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> WithMessage(
        string failureMessage);

    /// <summary>
    /// Sets a dynamic custom failure message factory for the most recently added validation component.
    /// The factory function will be invoked if the component fails to generate the message.
    /// </summary>
    /// <param name="failureMessageFactory">A function that generates the failure message, providing context and the property value.</param>
    /// <returns>The current validation component failure options builder for fluent chaining.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> WithMessage(
        FailureMessageFactory<T, TExternalResources, TProperty> failureMessageFactory);

    /// <summary>
    /// Sets the <see cref="FailureSeverity"/> for the most recently added validation component.
    /// This overrides the default severity for this specific component.
    /// </summary>
    /// <param name="failureSeverity">The severity level to assign to the failure.</param>
    /// <returns>The current validation component failure options builder for fluent chaining.</returns>
    IValidationComponentFailureOptionsBuilder<T, TExternalResources, TProperty> Severity(
        FailureSeverity failureSeverity);

}
