using KVKarco.ValidationAssistant.Abstractions.ValidationContexts;
using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// A delegate for an asynchronous predicate that evaluates a value.
/// </summary>
/// <typeparam name="T">The type of the value to be evaluated.</typeparam>
/// <param name="value">The value to evaluate.</param>
/// <param name="ct">The cancellation token.</param>
/// <returns><c>true</c> if the predicate is met, otherwise <c>false</c>.</returns>
public delegate Task<bool> AsyncPredicate<in T>(T value, CancellationToken ct);

/// <summary>
/// A delegate for an asynchronous action on a value.
/// </summary>
/// <typeparam name="T">The type of the value to be acted on.</typeparam>
/// <param name="value">The value to be acted on.</param>
/// <param name="ct">The cancellation token.</param>
/// <returns>A <see cref="Task"/> that represents the asynchronous action.</returns>
public delegate Task AsyncAction<in T>(T value, CancellationToken ct);

/// <summary>
/// A delegate for an asynchronous action that takes no parameters.
/// </summary>
/// <returns>A <see cref="Task"/> that represents the asynchronous action.</returns>
public delegate Task AsyncAction();

/// <summary>
/// Used in Ensure to create validity logic(IValidationRule) for a property.
/// This delegate is context-aware and can access external resources, the main validation instance, and the property value being validated.
/// </summary>
/// <typeparam name="TContext">The type of validation context.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated (contravariant).</typeparam>
/// <param name="context">The validation context, providing access to the subject and resources.</param>
/// <param name="value">The value of the property being validated.</param>
/// <returns><c>true</c> if the validation rule passes, <c>false</c> otherwise.</returns>
public delegate bool Predicate<TContext, in TProperty>(
    TContext context,
    TProperty value);

/// <summary>
/// Used in EnsureAsync to create asynchronous validity logic(IAsyncValidationRule) for a property.
/// This delegate is context-aware and can access external resources, the main validation instance, and the property value being validated.
/// </summary>
/// <typeparam name="TContext">The type of validation context.</typeparam>
/// <typeparam name="TResources">The type of external resources available to the validation process.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated (contravariant).</typeparam>
/// <param name="context">The validation context, providing access to the subject and resources.</param>
/// <param name="value">The value of the property being validated.</param>
/// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
/// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous validation, containing <c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
public delegate Task<bool> AsyncPredicate<TContext, in TProperty>(
    TContext context,
    TProperty value,
    CancellationToken ct);


/// <summary>
/// Used in conditionally executed logic
/// (e.g., <c>UseWhen</c>, <c>UseUnless</c>) to determine if a block of components or a rule/rules should be executed.
/// </summary>
/// <typeparam name="T">The type of the main validation instance.</typeparam>
/// <typeparam name="TResources">The type of external resources available to the validation process.</typeparam>
/// <param name="context">The condition context, providing access to the subject and resources.</param>
/// <returns><c>true</c> if the condition is met, otherwise <c>false</c>.</returns>
public delegate bool Condition<T, TResources>(
    IConditionCtx<T, TResources> context);

public delegate bool Condition<T, TResources, TProperty>(
    IConditionCtx<T, TResources> context, TProperty value);

/// <summary>
/// Used in asynchronous  conditionally executed logic
/// (e.g., <c>UseWhenAsync</c>, <c>UseUnlessAsync</c>) to determine if a block of components or a rule/rules should be executed.
/// </summary>
/// <typeparam name="T">The type of the main validation instance.</typeparam>
/// <typeparam name="TResources">The type of external resources available to the validation process.</typeparam>
/// <param name="context">The condition context, providing access to the subject and resources.</param>
/// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
/// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous condition, containing <c>true</c> if the condition is met, otherwise <c>false</c>.</returns>
public delegate Task<bool> AsyncCondition<T, TResources>(
    IConditionCtx<T, TResources> context,
    CancellationToken ct);

public delegate Task<bool> AsyncCondition<T, TResources, TProperty>(
    IConditionCtx<T, TResources> context,
    TProperty value,
    CancellationToken ct);

/// <summary>
/// A delegate used to create dynamic validation failure messages for property rules.
/// The message can be generated based on the validation context and the property's value.
/// </summary>
/// <typeparam name="T">The type of the main validation instance.</typeparam>
/// <typeparam name="TResources">The type of external resources available to the validation process.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated (contravariant).</typeparam>
/// <param name="context">The message context, providing access to the subject and resources.</param>
/// <param name="value">The value of the property being validated.</param>
/// <returns>The validation failure message as a string.</returns>
public delegate string MessageFactory<T, TResources, in TProperty>(
    IMessageCtx<T, TResources> context,
    TProperty value);

/// <summary>
/// Validator factory for creating nested reusable <see cref="CustomValidator{TTarget}"/> in side validators.
/// </summary>
/// <typeparam name="TResources">The type of external resources available to the validation process in the main validator.</typeparam>
/// <typeparam name="TProperty">The type of the property this factory will create validator for. </typeparam>
/// <param name="context">The external recourses available in the parent validator.</param>
/// <returns></returns>
public delegate CustomValidator<TProperty> ValidatorFactory<T, TResources, TProperty>(
    ValidationCtx<T, TResources> context);