namespace KVKarco.ValidationAssistant.Abstractions.ValidationContexts;

/// <summary>
/// Represents the fundamental validation context for a single object instance.
/// Provides access to the object being validated and external resources needed for validation rules.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TResources">
/// The type of the external resources (e.g., repositories, services, configuration, or other validators)
/// injected into the validator for use by validation rules.
/// </typeparam>
/// <remarks>
/// This context is passed to validation rules to provide both the subject under validation
/// and the supporting resources required to perform rule checks.
/// </remarks>
public interface IValidationCtx<T, TResources> : ICleanUpCtx
{
    /// <summary>
    /// Gets the object instance being validated.
    /// </summary>
    T Subject { get; }

    /// <summary>
    /// Gets the external resources available to the validation rules.
    /// </summary>
    TResources Resources { get; }
}
