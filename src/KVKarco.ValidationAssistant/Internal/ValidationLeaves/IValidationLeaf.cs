using KVKarco.ValidationAssistant.Internal;

using KVKarco.ValidationAssistant.Internal.Utilities.TargetAssets;

/// <summary>
/// Defines a single, atomic validation rule(leaf) that can be composed into a validation tree,
/// contains the validation logic and the options how it executed.
/// </summary>
/// <remarks>
/// This interface represents a 'leaf' in a validation node chain, encapsulating a specific validation check for a property.
/// It supports both synchronous and asynchronous execution and is designed for high performance and allocation efficiency.
/// </remarks>
/// <typeparam name="T">The type of the root object being validated.</typeparam>
/// <typeparam name="TResources">The type containing shared resources for the validation process.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
internal interface IValidationLeaf<T, TResources, TProperty>
    where TResources : class
{
    /// <summary>
    /// Gets a value indicating whether the rule can be executed synchronously.
    /// </summary>
    /// <remarks>
    /// This property is a passthrough to the underlying rule options, indicating if the
    /// rule's core logic and conditions are all synchronous.
    /// </remarks>
    bool CanExceedSynchronously { get; }

    /// <summary>
    /// Executes the validation rule synchronously.
    /// </summary>
    /// <exception cref="Exception.ValidationAssistantException">
    /// Thrown when the property's path cannot be resolved (e.g., a member in the path is null),
    /// or if the validation rule is asynchronous but was called synchronously.
    /// </exception>
    /// <param name="context">The validation context.</param>
    /// <param name="undefined">A reference to the undefined property value.</param>
    void Execute(ValidationCtx<T, TResources> context, in Undefined<TProperty> undefined);

    /// <summary>
    /// Executes the validation rule asynchronously.
    /// </summary>
    /// <remarks>
    /// This method uses <see cref="ValueTask"/> to provide an allocation-free path for synchronous completions,
    /// improving performance in hot paths.
    /// </remarks>
    /// <exception cref="Exception.ValidationAssistantException">
    /// Thrown when the property's path cannot be resolved (e.g., a member in the path is null).
    /// </exception>
    /// <param name="context">The validation context.</param>
    /// <param name="undefined">The undefined property value.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask ExecuteAsync(ValidationCtx<T, TResources> context, Undefined<TProperty> undefined, CancellationToken ct);
}