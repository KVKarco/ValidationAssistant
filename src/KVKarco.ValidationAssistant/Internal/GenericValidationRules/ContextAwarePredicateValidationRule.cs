using KVKarco.ValidationAssistant.Abstractions;

namespace KVKarco.ValidationAssistant.Internal.GenericValidationRules;

/// <summary>
/// Represents a synchronous validation rule that uses a context-aware predicate function
/// to determine the validity of a property. This rule provides the predicate with full
/// access to the validation run context, including the main object instance and external resources.
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources or dependencies available to the rule.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated by this rule.</typeparam>
internal sealed class ContextAwarePredicateValidationRule<T, TExternalResources, TProperty> :
    IValidationRule<T, TExternalResources, TProperty>
{
    /// <summary>
    /// The context-aware predicate function used to evaluate the validity of the property.
    /// </summary>
    private readonly ContextAwarePropertyPredicate<T, TExternalResources, TProperty> _predicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextAwarePredicateValidationRule{T, TExternalResources, TProperty}"/> class.
    /// </summary>
    /// <param name="predicate">The synchronous, context-aware predicate function that defines the validation logic for the property.</param>
    public ContextAwarePredicateValidationRule(ContextAwarePropertyPredicate<T, TExternalResources, TProperty> predicate)
        => _predicate = predicate;

    /// <summary>
    /// Gets the unique name of this validation rule. This is typically a predefined constant
    /// from <see cref="DefaultNaming"/>.
    /// </summary>
    public ReadOnlySpan<char> RuleName => DefaultNaming.ContextAwarePropertyValRule;

    /// <summary>
    /// Provides the default failure message for this validation rule when the predicate evaluates to <see langword="false"/>.
    /// It retrieves the message from the global default messages configuration.
    /// </summary>
    /// <param name="context">The message context, providing access to the validated object, external resources, and culture.</param>
    /// <param name="value">The actual value of the property that was being validated.</param>
    /// <returns>A string representing the default failure message for this rule, typically
    /// "The specified condition was not met for [PropertyName]".</returns>
    public string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value)
       => ValidatorsConfig.GlobalDefaults.Messages.ValidationComponentError(context, value);

    /// <summary>
    /// Determines whether the property value is valid by executing the encapsulated context-aware predicate.
    /// </summary>
    /// <param name="context">The validation run context, providing access to the main object, external dependencies, and other validation details.</param>
    /// <param name="value">The value of the property to validate.</param>
    /// <returns><see langword="true"/> if the predicate returns <see langword="true"/> (indicating validity); otherwise, <see langword="false"/>.</returns>
    public bool IsValid(ValidatorRunCtx<T, TExternalResources> context, TProperty value)
        => _predicate(context, value);
}

/// <summary>
/// Represents an asynchronous validation rule that uses a context-aware predicate function
/// to determine the validity of a property. This rule provides the predicate with full
/// access to the validation run context, including the main object instance, external resources,
/// and a cancellation token for asynchronous operations.
/// </summary>
/// <typeparam name="T">The type of the main object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources or dependencies available to the rule.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated by this rule.</typeparam>
internal sealed class ContextAwarePredicateAsyncValidationRule<T, TExternalResources, TProperty> :
    IAsyncValidationRule<T, TExternalResources, TProperty>
{
    /// <summary>
    /// The asynchronous, context-aware predicate function used to evaluate the validity of the property.
    /// </summary>
    private readonly AsyncContextAwarePropertyPredicate<T, TExternalResources, TProperty> _asyncPredicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextAwarePredicateAsyncValidationRule{T, TExternalResources, TProperty}"/> class.
    /// </summary>
    /// <param name="asyncPredicate">The asynchronous, context-aware predicate function that defines the validation logic for the property.</param>
    public ContextAwarePredicateAsyncValidationRule(AsyncContextAwarePropertyPredicate<T, TExternalResources, TProperty> asyncPredicate)
        => _asyncPredicate = asyncPredicate;

    /// <summary>
    /// Gets the unique name of this asynchronous validation rule. This is typically a predefined constant
    /// from <see cref="DefaultNaming"/>.
    /// </summary>
    public ReadOnlySpan<char> RuleName => DefaultNaming.ContextAwarePredicateAsyncValRule;

    /// <summary>
    /// Provides the default failure message for this asynchronous validation rule when the predicate evaluates to <see langword="false"/>.
    /// It retrieves the message from the global default messages configuration.
    /// </summary>
    /// <param name="context">The message context, providing access to the validated object, external resources, and culture.</param>
    /// <param name="value">The actual value of the property that was being validated.</param>
    /// <returns>A string representing the default failure message for this rule, typically
    /// "The specified condition was not met for [PropertyName]".</returns>
    public string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value)
       => ValidatorsConfig.GlobalDefaults.Messages.ValidationComponentError(context, value);

    /// <summary>
    /// Asynchronously determines whether the property value is valid by executing the encapsulated
    /// asynchronous, context-aware predicate.
    /// </summary>
    /// <param name="context">The validation run context, providing access to the main object, external dependencies, and other validation details.</param>
    /// <param name="value">The value of the property to validate.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous validation operation. The task result is <see langword="true"/>
    /// if the predicate returns <see langword="true"/> (indicating validity); otherwise, <see langword="false"/>.</returns>
    public Task<bool> IsValidAsync(ValidatorRunCtx<T, TExternalResources> context, TProperty value, CancellationToken ct)
        => _asyncPredicate(context, value, ct);
}