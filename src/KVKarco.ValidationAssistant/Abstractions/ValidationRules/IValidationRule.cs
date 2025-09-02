using KVKarco.ValidationAssistant.Abstractions.MessageTemplate;
using KVKarco.ValidationAssistant.Abstractions.ValidationContexts;

namespace KVKarco.ValidationAssistant.Abstractions.ValidationRules;

public interface IValidationRule
{
    /// <summary>
    /// Gets the name of the validation rule. This name is used for identification and reporting, but is not guaranteed to be unique.
    /// </summary>
    string RuleName { get; }
}

/// <summary>
/// Defines the synchronous context aware contract for a validation rule that operates on a specific property.
/// </summary>
/// <typeparam name="T">The type of the root entity being validated.</typeparam>
/// <typeparam name="TExternalResources">The type representing any external resources required during validation.</typeparam>
/// <typeparam name="TProperly">The type of the property being validated. The 'in' keyword indicates that this type parameter is contravariant.</typeparam>
public interface IValidationRule<T, TResources, in TProperty> : IValidationRule
{
    /// <summary>
    /// Provides the default failure message when this rule fails.
    /// </summary>
    /// <param name="context">The message context (<see cref="IMessageCtx"/> or <see cref="IMessageCtx{T,TResources}"/>)
    /// that can be used to localize or template the message.</param>
    /// <param name="value">The value of the property being validated.</param>
    /// <returns>A string representing the failure message.</returns>
    /// <remarks>
    /// You have two options when building messages:
    /// <list type="bullet">
    /// <item><description>
    /// <b>Simple usage:</b> Return an interpolated string directly (fast and straightforward).
    /// </description></item>
    /// <item><description>
    /// <b>Advanced usage:</b> Use <see cref="IMessageResolver"/> templates with fluent <c>Replace</c> calls.
    /// Call <c>GetMessage(toPool: ...)</c> at the end to finalize the message.
    /// </description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><b>Option 1 — Simple interpolated string:</b></para>
    /// <code>
    /// public string GetDefaultFailureMessage(IMessageCtx context, string value)
    ///     => $"{context.TargetName} cannot be empty (was '{value}').";
    /// </code>
    /// <para><b>Option 2 — Advanced template with pooling:</b></para>
    /// <code>
    /// public string GetDefaultFailureMessage(IMessageCtx context, string value)
    /// {
    ///     return context.GetTemplate("NotEmpty")
    ///                   .Replace("PropertyName", context.TargetName)
    ///                   .Replace("AttemptedValue", value)
    ///                   .GetMessage(toPool: true);
    /// }
    /// </code>
    /// <para>
    /// Use <c>toPool: true</c> if all placeholders are static per rule instance (e.g., property name).  
    /// Use <c>toPool: false</c> if placeholders depend on dynamic values that change per validation run (e.g., the actual input).
    /// </para>
    /// </example>
    string GetDefaultFailureMessage(IMessageCtx<T, TResources> context, TProperty value);

    /// <summary>
    /// Determines whether the property value is valid synchronously according to this rule's logic.
    /// </summary>
    /// <param name="context">The validation context.<see cref="IValidationCtx{T,TResources}"/>.<seealso cref="ICleanUpCtx"/></param>
    /// <param name="value">The value of the property being validated.</param>
    /// <returns><c>true</c> if the property value is valid; otherwise, <c>false</c>.</returns>
    bool IsValid(IValidationCtx<T, TResources> context, TProperty value);
}