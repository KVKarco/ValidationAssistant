using KVKarco.ValidationAssistant.Abstractions;

namespace KVKarco.ValidationAssistant.Internal.GenericValidationRules;

/// <summary>
/// Represents a synchronous validation rule designed to control the flow of execution within a
/// <c>PropertyRule ValidationRulesSet</c> based on a predefined condition. If the condition
/// evaluates to <see langword="false"/>, it typically indicates that further validation
/// for the current property rule set should cease, often implying a logical stop.
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources or dependencies available to the rule.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated by this rule (though its value might not be directly used by the condition).</typeparam>
internal sealed class ConditionalFlowValidationRule<T, TExternalResources, TProperty> :
    IValidationRule<T, TExternalResources, TProperty>
{
    /// <summary>
    /// The synchronous validation condition that determines whether to allow further execution of validation rules.
    /// </summary>
    private readonly ValidationCondition<T, TExternalResources> _condition;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalFlowValidationRule{T, TExternalResources, TProperty}"/> class.
    /// </summary>
    /// <param name="condition">The synchronous condition delegate that dictates the flow control.
    /// If it returns <see langword="false"/>, the validation flow for the current rule set is typically stopped.</param>
    public ConditionalFlowValidationRule(ValidationCondition<T, TExternalResources> condition)
        => _condition = condition;

    /// <summary>
    /// Gets the unique name of this conditional flow validation rule. This is typically a predefined constant
    /// from <see cref="DefaultNaming"/>.
    /// </summary>
    public ReadOnlySpan<char> RuleName => DefaultNaming.ConditionalValRule;

    /// <summary>
    /// Provides the default failure message for this validation rule, specifically indicating a logical stop.
    /// It retrieves the message from the global default messages configuration.
    /// </summary>
    /// <param name="context">The message context, providing access to the validated object, external resources, and culture.</param>
    /// <param name="value">The actual value of the property associated with the component (can be ignored if not applicable).</param>
    /// <returns>A string representing the logical component stop explanation for this rule.</returns>
    public string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value)
        => ValidatorsConfig.GlobalDefaults.Messages.LogicalComponentStopExplanation(context, value);

    /// <summary>
    /// Determines whether the validation flow should continue by executing the encapsulated condition.
    /// </summary>
    /// <param name="context">The validation run context, providing access to the main object and external dependencies, which are used by the condition.</param>
    /// <param name="value">The value of the property to validate (might not be directly used by this flow control rule).</param>
    /// <returns><see langword="true"/> if the condition evaluates to <see langword="true"/> (meaning flow can continue); otherwise, <see langword="false"/>.</returns>
    public bool IsValid(ValidatorRunCtx<T, TExternalResources> context, TProperty value)
        => _condition(context);
}

/// <summary>
/// Represents an asynchronous validation rule designed to control the flow of execution within a
/// <c>PropertyRule ValidationRulesSet</c> based on a predefined asynchronous condition. If the condition
/// evaluates to <see langword="false"/>, it typically indicates that further validation
/// for the current property rule set should cease, often implying a logical stop.
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources or dependencies available to the rule.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated by this rule (though its value might not be directly used by the condition).</typeparam>
internal sealed class ConditionalFlowAsyncValidationRule<T, TExternalResources, TProperty> :
    IAsyncValidationRule<T, TExternalResources, TProperty>
{
    /// <summary>
    /// The asynchronous validation condition that determines whether to allow further execution of validation rules.
    /// </summary>
    private readonly AsyncValidationCondition<T, TExternalResources> _condition;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalFlowAsyncValidationRule{T, TExternalResources, TProperty}"/> class.
    /// </summary>
    /// <param name="condition">The asynchronous condition delegate that dictates the flow control.
    /// If it returns <see langword="false"/>, the validation flow for the current rule set is typically stopped.</param>
    public ConditionalFlowAsyncValidationRule(AsyncValidationCondition<T, TExternalResources> condition)
        => _condition = condition;

    /// <summary>
    /// Gets the unique name of this asynchronous conditional flow validation rule. This is typically a predefined constant
    /// from <see cref="DefaultNaming"/>.
    /// </summary>
    public ReadOnlySpan<char> RuleName => DefaultNaming.ConditionalAsyncValRule;

    /// <summary>
    /// Provides the default failure message for this asynchronous validation rule, specifically indicating a logical stop.
    /// It retrieves the message from the global default messages configuration.
    /// </summary>
    /// <param name="context">The message context, providing access to the validated object, external resources, and culture.</param>
    /// <param name="value">The actual value of the property associated with the component (can be ignored if not applicable).</param>
    /// <returns>A string representing the logical component stop explanation for this rule.</returns>
    public string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value)
        => ValidatorsConfig.GlobalDefaults.Messages.LogicalComponentStopExplanation(context, value);

    /// <summary>
    /// Asynchronously determines whether the validation flow should continue by executing the encapsulated asynchronous condition.
    /// </summary>
    /// <param name="context">The validation run context, providing access to the main object and external dependencies, which are used by the condition.</param>
    /// <param name="value">The value of the property to validate (might not be directly used by this flow control rule).</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous validation operation. The task result is <see langword="true"/>
    /// if the condition evaluates to <see langword="true"/> (meaning flow can continue); otherwise, <see langword="false"/>.</returns>
    public Task<bool> IsValidAsync(ValidatorRunCtx<T, TExternalResources> context, TProperty value, CancellationToken ct)
        => _condition(context, ct);
}
