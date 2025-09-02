using KVKarco.ValidationAssistant.Internal.Utilities.TargetAssets;

namespace KVKarco.ValidationAssistant.Internal.CustomValidatorAssets.Abstractions;

/// <summary>
/// Provides a contract for validation components that are parts of Parent components.
/// </summary>
/// <typeparam name="TSubject">The type of the entity or object being validated.</typeparam>
/// <typeparam name="TResources">The type of external resources used during validation.</typeparam>
/// <typeparam name="TContext">The type of the validation run context.</typeparam>
internal interface IValidatorLeaf<TSubject, TResources, TTarget>
{
    bool CanExecuteSynchronously { get; }

    /// <summary>
    /// Runs the validation logic for the component synchronously.
    /// </summary>
    /// <param name="context">Context associated with the current validator run.</param>
    void Execute(CustomValidatorRunCtx<TSubject, TResources> context, Undefined<TTarget> value);

    /// <summary>
    /// Runs the validation logic for the component asynchronously.
    /// </summary>
    /// <param name="context">Context associated with the current validator run.</param>
    ValueTask ExecuteAsync(CustomValidatorRunCtx<TSubject, TResources> context, Undefined<TTarget> value, CancellationToken ct);
}
