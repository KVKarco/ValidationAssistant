namespace KVKarco.ValidationAssistant.Abstractions.ValidationContexts;

/// <summary>
/// Represents the context for conditional validation logic, used in <see cref="ValidationCondition{T, TExternalResources}"/> delegates.
/// Provides access to the object instance, external resources, and previously captured validation snapshots.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TResources">The type of the external resources provided to validation rules.</typeparam>
/// <remarks>
/// This context is typically used when enabling or disabling rule sets based on the results of earlier validation passes.
/// </remarks>
/// <seealso cref="IValidationCtx{T,TResources}"/>
public interface IConditionCtx<T, TResources> :
    IValidationCtx<T, TResources>
{
    /// <summary>
    /// Determines whether a previously captured validation snapshot was valid.
    /// </summary>
    /// <param name="snapShotIdentifier">The unique identifier of the snapshot to check.</param>
    /// <returns>
    /// <see langword="true"/> if the snapshot contained no property rule failures at the time it was captured;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ValidationRunException">
    /// Thrown if snapshots are not supported, if the identifier does not exist,
    /// or if the snapshot has not yet been initialized.
    /// </exception>
    bool IsSnapShotValid(string snapShotIdentifier);
}
