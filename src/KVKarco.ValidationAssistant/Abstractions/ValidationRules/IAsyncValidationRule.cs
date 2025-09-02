using KVKarco.ValidationAssistant.Abstractions.ValidationContexts;

namespace KVKarco.ValidationAssistant.Abstractions.ValidationRules;

/// <summary>
/// Defines the context-aware asynchronous contract for a validation rule that operates on a specific property.
/// </summary>
/// <typeparam name="T">The type of the root entity being validated.</typeparam>
/// <typeparam name="TResources">The type representing any external resources required during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated. The 'in' keyword indicates that this type parameter is contravariant.</typeparam>
public interface IAsyncValidationRule<T, TResources, in TProperty> : IValidationRule
{
    /// <summary>
    /// Provides the default failure message for this asynchronous validation rule if it fails.
    /// </summary>
    /// <param name="context">The message context (<see cref="IMessageCtx{T,TResources}"/>) that can be used to localize or template the message.</param>
    /// <param name="value">The value of the property being validated.</param>
    /// <returns>A string representing the failure message.</returns>
    /// <remarks>
    /// Use simple interpolated strings for quick messages, or fluent templates for advanced usage:
    /// <code>
    /// context.GetTemplate("NotEmpty")
    ///        .Replace("PropertyName", context.TargetName)
    ///        .Replace("AttemptedValue", value)
    ///        .GetMessage(toPool: true);
    /// </code>
    /// <para>
    /// Use <c>toPool: true</c> if all placeholders are static for this rule instance.  
    /// Use <c>toPool: false</c> if placeholders depend on dynamic per-value data.
    /// </para>
    /// </remarks>
    string GetDefaultFailureMessage(IMessageCtx<T, TResources> context, TProperty value);

    /// <summary>
    /// Determines whether the property value is valid asynchronously according to this rule's logic.
    /// </summary>
    /// <param name="context">The validation context (<see cref="IValidationCtx{T,TResources}"/> <seealso cref="ICleanUpCtx"/>).</param>
    /// <param name="value">The value of the property being validated.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a result of <c>true</c> if the property value is valid; otherwise, <c>false</c>.</returns>
    Task<bool> IsValidAsync(IValidationCtx<T, TResources> context, TProperty value, CancellationToken ct);
}
