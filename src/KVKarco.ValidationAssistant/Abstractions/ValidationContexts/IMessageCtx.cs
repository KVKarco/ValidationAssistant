namespace KVKarco.ValidationAssistant.Abstractions.ValidationContexts;

using KVKarco.ValidationAssistant.Abstractions.MessageTemplate;
using System;
using System.Globalization;

/// <summary>
/// Represents the basic context for generating validation failure messages.
/// Provides access to the target property and cultural settings used for localization.
/// </summary>
/// <remarks>
/// This context is designed to support message templating and localization.
/// </remarks>
public interface IMessageCtx : IDisposable
{
    /// <summary>
    /// Gets the name of the property that failed validation.
    /// </summary>
    /// <remarks>
    /// This is typically the property name, but may also represent an alternate identifier
    /// for rules that apply to multiple properties.
    /// </remarks>
    string PropertyName { get; }

    /// <summary>
    /// Gets the culture information used to localize the message.
    /// </summary>
    CultureInfo Culture { get; }

    /// <summary>
    /// Retrieves a message template resolver by name base on the current culture used in validation or from default configuration.
    /// </summary>
    /// <param name="templateName">The name of the template to resolve.</param>
    /// <returns>An <see cref="IMessageResolver"/> capable of resolving the specified template.</returns>
    IMessageResolver GetTemplate(string templateName);
}

/// <summary>
/// Represents a comprehensive context for generating validation failure messages,
/// combining object instance details, external resources, and property-specific information.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TResources">The type of the external resources available to validation rules.</typeparam>
/// <remarks>
/// This context extends <see cref="IMessageCtx"/> to also include subject and resource access,
/// making it suitable for advanced scenarios where message generation depends on both
/// the data being validated and external dependencies.
/// </remarks>
public interface IMessageCtx<T, TResources> : IMessageCtx
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