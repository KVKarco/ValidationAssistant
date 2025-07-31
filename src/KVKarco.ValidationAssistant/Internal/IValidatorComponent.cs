namespace KVKarco.ValidationAssistant.Internal;

/// <summary>
/// Provides a contract for validation components that can validate properties, conditions etc.
/// </summary>
/// <typeparam name="T">The type of the entity or object being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources used during validation.</typeparam>
/// <typeparam name="TContext">The type of the validation run context.</typeparam>
internal interface IValidatorComponent<T, TExternalResources, TContext>
    where TContext : ValidatorRunCtx<T, TExternalResources>
{
    /// <summary>
    /// Runs the validation logic for the component synchronously.
    /// </summary>
    /// <param name="context">Contex associated with the current validator run.</param>
    /// <param name="metaData">Information for associated with the current component.</param>
    void Execute(TContext context, ValidatorComponentMetaData metaData);

    /// <summary>
    /// Runs the validation logic for the component synchronously.
    /// </summary>
    /// <param name="context">Contex associated with the current validator run.</param>
    /// <param name="metaData">Information for associated with the current component.</param>
    ValueTask ExecuteAsync(TContext context, ValidatorComponentMetaData metaData, CancellationToken ct);
}