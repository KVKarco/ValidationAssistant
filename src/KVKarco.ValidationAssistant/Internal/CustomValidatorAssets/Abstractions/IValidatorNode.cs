using KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;

namespace KVKarco.ValidationAssistant.Internal.CustomValidatorAssets.Abstractions;

/// <summary>
/// Provides a contract for validation components that can validate properties, conditions etc.
/// </summary>
/// <typeparam name="TSubject">The type of the entity or object being validated.</typeparam>
/// <typeparam name="TResources">The type of external resources used during validation.</typeparam>
internal interface IValidatorNode<TSubject, TResources>
{
    /// <summary>
    /// Does the component is asynchronous.
    /// </summary>
    bool CanExecuteSynchronously { get; }

    /// <summary>
    /// Runs the validation logic for the component synchronously.
    /// </summary>
    /// <param name="context">Contex associated with the current validator run.</param>
    void Execute(CustomValidatorRunCtx<TSubject, TResources> context);

    /// <summary>
    /// Runs the validation logic for the component synchronously.
    /// </summary>
    /// <param name="context">Contex associated with the current validator run.</param>
    ValueTask ExecuteAsync(CustomValidatorRunCtx<TSubject, TResources> context, CancellationToken ct);
}
