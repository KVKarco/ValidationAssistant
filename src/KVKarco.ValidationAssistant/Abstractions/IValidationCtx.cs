using System.Globalization;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// Represents the fundamental validation context for a single object instance.
/// Provides access to the object being validated and external resources needed for validation rules.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of the object containing external resources (e.g., repositories, services, configuration, validators)
/// injected into the validator for use by validation rules.</typeparam>
public interface IValidationCtx<T, TExternalResources>
{
    /// <summary>
    /// Gets the instance of the object currently being validated.
    /// </summary>
    T ValidationInstance { get; }

    /// <summary>
    /// Gets the external resources available to the validation rules.
    /// These resources are typically injected via the validator's constructor.
    /// </summary>
    TExternalResources Resources { get; }
}

/// <summary>
/// Represents the context for conditional validation logic, used in <see cref="ValidationCondition{T, TExternalResources}"/> delegates.
/// Provides access to the object instance, external resources, and the ability to query validation snapshots.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of the object containing external resources.</typeparam>
public interface IConditionCtx<T, TExternalResources> :
    IValidationCtx<T, TExternalResources>
{
    /// <summary>
    /// Checks the validity state of a previously captured validation snapshot.
    /// This is typically used to enable/disable rule sets based on prior validation results.
    /// </summary>
    /// <param name="snapShotIdentifier">The unique identifier of the snapshot whose validity is to be retrieved.</param>
    /// <returns><see langword="true"/> if the snapshot was valid (no property rule failures at its capture point); otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ValidationRunException">
    /// Thrown if the validator does not support snapshots, if the <paramref name="snapShotIdentifier"/> does not exist,
    /// or if the snapshot's value has not yet been set.
    /// </exception>
    bool IsSnapShotValid(string snapShotIdentifier);
}

/// <summary>
/// Represents a basic context for generating validation failure messages.
/// Provides property-specific information and cultural settings.
/// </summary>
public interface IMessageCtx
{
    /// <summary>
    /// Gets the name of the property that failed validation.
    /// </summary>
    ReadOnlySpan<char> PropertyName { get; }

    /// <summary>
    /// Gets the culture information to be used for localizing the message.
    /// </summary>
    CultureInfo Culture { get; }
}

/// <summary>
/// Represents a comprehensive context for generating validation failure messages,
/// combining object instance details, external resources, and property-specific information.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of the object containing external resources.</typeparam>
public interface IMessageCtx<T, TExternalResources> :
    IValidationCtx<T, TExternalResources>,
    IMessageCtx
{
    // No additional members, just combining interfaces
}