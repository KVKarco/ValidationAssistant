using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal;

namespace KVKarco.ValidationAssistant;

/// <summary>
/// Represents a simple, synchronous predicate that validates a property's value.
/// This delegate does not have access to the full validation context or external resources.
/// </summary>
/// <typeparam name="TProperty">The type of the property value to be validated.</typeparam>
/// <param name="propertyValue">The value of the property to validate.</param>
/// <returns><c>true</c> if the property value is valid; otherwise, <c>false</c>.</returns>
public delegate bool PropertyPredicate<in TProperty>(TProperty propertyValue);

/// <summary>
/// Represents a simple, asynchronous predicate that validates a property's value.
/// This delegate does not have access to the full validation context or external resources.
/// </summary>
/// <typeparam name="TProperty">The type of the property value to be validated.</typeparam>
/// <param name="propertyValue">The value of the property to validate.</param>
/// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
/// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous validation operation.
/// The task result is <c>true</c> if the property value is valid; otherwise, <c>false</c>.</returns>
public delegate Task<bool> AsyncPropertyPredicate<in TProperty>(
    TProperty propertyValue,
    CancellationToken ct);

/// <summary>
/// Represents a synchronous predicate that validates a property's value,
/// providing access to the full validation context including the object instance and external resources.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of the object containing external resources.</typeparam>
/// <typeparam name="TProperty">The type of the property value to be validated.</typeparam>
/// <param name="context">The <see cref="IValidationCtx{T, TExternalResources}"/> providing access to the object instance and resources.</param>
/// <param name="value">The value of the property to validate.</param>
/// <returns><c>true</c> if the property value is valid; otherwise, <c>false</c>.</returns>
public delegate bool ContextAwarePropertyPredicate<T, TExternalResources, in TProperty>(
    IValidationCtx<T, TExternalResources> context,
    TProperty value);

/// <summary>
/// Represents an asynchronous predicate that validates a property's value,
/// providing access to the full validation context including the object instance and external resources.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of the object containing external resources.</typeparam>
/// <typeparam name="TProperty">The type of the property value to be validated.</typeparam>
/// <param name="context">The <see cref="IValidationCtx{T, TExternalResources}"/> providing access to the object instance and resources.</param>
/// <param name="propertyValue">The value of the property to validate.</param>
/// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
/// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous validation operation.
/// The task result is <c>true</c> if the property value is valid; otherwise, <c>false</c>.</returns>
public delegate Task<bool> AsyncContextAwarePropertyPredicate<T, TExternalResources, in TProperty>(
    IValidationCtx<T, TExternalResources> context,
    TProperty propertyValue,
    CancellationToken ct);

/// <summary>
/// Represents a factory delegate for generating dynamic validation failure messages.
/// Provides comprehensive context including object instance, external resources, property name, and culture.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of the object containing external resources.</typeparam>
/// <typeparam name="TProperty">The type of the property value that failed validation.</typeparam>
/// <param name="context">The <see cref="IMessageCtx{T, TExternalResources}"/> providing contextual information for message generation.</param>
/// <param name="propertyValue">The value of the property that failed validation.</param>
/// <returns>The generated validation error message string.</returns>
public delegate string FailureMessageFactory<T, TExternalResources, in TProperty>(
    IMessageCtx<T, TExternalResources> context,
    TProperty propertyValue);

/// <summary>
/// Represents a synchronous condition predicate used to determine whether a set of validation rules should execute.
/// Provides access to validation snapshots and external resources.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of the object containing external resources.</typeparam>
/// <param name="context">The <see cref="IConditionCtx{T, TExternalResources}"/> providing conditional evaluation context.</param>
/// <returns><c>true</c> if the associated validation rules should execute; otherwise, <c>false</c>.</returns>
public delegate bool ValidationCondition<T, TExternalResources>(
    IConditionCtx<T, TExternalResources> context);

/// <summary>
/// Represents an asynchronous condition predicate used to determine whether a set of validation rules should execute.
/// Provides access to validation snapshots and external resources.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of the object containing external resources.</typeparam>
/// <param name="context">The <see cref="IConditionCtx{T, TExternalResources}"/> providing conditional evaluation context.</param>
/// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
/// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation.
/// The task result is <c>true</c> if the associated validation rules should execute; otherwise, <c>false</c>.</returns>
public delegate Task<bool> AsyncValidationCondition<T, TExternalResources>(
    IConditionCtx<T, TExternalResources> context,
    CancellationToken ct);

/// <summary>
/// Represents a synchronous predicate function used for pre-validation rules.
/// This delegate evaluates a given value and returns a boolean indicating whether the pre-validation condition is met.
/// </summary>
/// <typeparam name="T">The type of the value to be validated.</typeparam>
/// <param name="value">The value to evaluate against the pre-validation condition.</param>
/// <returns><see langword="true"/> if the pre-validation condition is met; otherwise, <see langword="false"/>.</returns>
public delegate bool PreValidationPredicate<T>(T value);

/// <summary>
/// Represents an asynchronous predicate function used for pre-validation rules.
/// This delegate evaluates a given value asynchronously and returns a Task that resolves to a boolean,
/// indicating whether the pre-validation condition is met.
/// </summary>
/// <typeparam name="T">The type of the value to be validated.</typeparam>
/// <param name="value">The value to evaluate against the pre-validation condition asynchronously.</param>
/// <returns>A <see cref="Task{TResult}"/> that resolves to <see langword="true"/> if the pre-validation condition is met; otherwise, <see langword="false"/>.</returns>
public delegate Task<bool> AsyncPreValidationPredicate<T>(T value, CancellationToken ct);

/// <summary>
/// Defines an internal delegate responsible for resolving (extracting) the value of a specific property
/// from a given validation instance. This delegate is typically created by compiling an
/// expression (e.g., a property selector like `x => x.SomeProperty`) and is used internally
/// by property rules to efficiently access the values they need to validate.
/// </summary>
/// <typeparam name="T">The type of the main validation instance from which the property value is to be extracted.</typeparam>
/// <typeparam name="TProperty">The type of the property whose value is to be extracted.</typeparam>
/// <param name="validationInstance">The instance of type <typeparamref name="T"/> from which the property value will be resolved.</param>
/// <returns>
/// An <see cref="Undefined{TProperty}"/> instance representing the resolved property value.
/// This wrapper can distinguish between a property that is genuinely <see langword="null"/>
/// and one that is conceptually "undefined" or not present due to, for example, a null intermediate
/// in a property chain (e.g., `x.Address.Street` where `Address` is null).
/// </returns>
internal delegate Undefined<TProperty> PropertyValueResolver<T, TProperty>(T validationInstance);