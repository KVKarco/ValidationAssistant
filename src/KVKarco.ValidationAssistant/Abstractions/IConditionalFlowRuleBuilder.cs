using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// Defines the fluent API for initiating a **conditional validation block**.
/// Rules defined within these blocks are applied only when a specified condition is met.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <remarks>
/// Rules inside conditional blocks default to a "Continue" failure strategy. Conditional flow rules can be **nested**.
/// </remarks>
public interface IConditionalFlowRuleBuilder<T, TExternalResources>
{
    /// <summary>
    /// Initiates a synchronous **conditional block**. Rules within the action are applied only if <paramref name="condition"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="condition">A synchronous predicate.</param>
    /// <param name="rulesToUseWhenConditionIsMet">Action to define rules when condition is met. Supports **nesting**.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number.</param>
    /// <returns>An <see cref="IOtherwiseConditionalFlowRuleBuilder{T, TExternalResources}"/> for an optional "otherwise" block.</returns>
    /// <remarks>
    /// Use this for conditions affecting **multiple rules or a block of rules**, especially with an <c>OtherwiseUse</c> clause.
    /// For conditions affecting **only a single property rule** without an <c>OtherwiseUse</c>, consider
    /// <see cref="IPropertyRuleConditionComponentBuilder{T, TExternalResources, TProperty}.ContinueWhen(ValidationCondition{T, TExternalResources}, string?, int)"/> or its variants.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: Apply rules if user is active, with nested condition
    /// builder.UseWhen(ctx => ctx.ValidationInstance.User.IsActive, () =>
    /// {
    ///     builder.UseFor(x => x.User.Email).NotNull();
    ///     builder.UseFor(x => x.User.PhoneNumber).MatchesRegex(@"^\d{10}$");
    ///
    ///     // Nested conditional block
    ///     builder.UseWhen(ctx => ctx.ValidationInstance.User.HasPremiumAccount, () =>
    ///     {
    ///         builder.UseFor(x => x.User.PremiumFeatures).NotEmpty();
    ///     });
    /// });
    /// </code>
    /// </example>
    IOtherwiseConditionalFlowRuleBuilder<T, TExternalResources> UseWhen(
        ValidationCondition<T, TExternalResources> condition,
        Action rulesToUseWhenConditionIsMet,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Initiates an asynchronous **conditional block**. Rules within the action are applied only if <paramref name="condition"/> is <see langword="true"/> asynchronously.
    /// </summary>
    /// <param name="condition">An asynchronous predicate.</param>
    /// <param name="rulesToUseWhenConditionIsMet">Action to define rules when condition is met. Supports **nesting**.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number.</param>
    /// <returns>An <see cref="IOtherwiseConditionalAsyncFlowRuleBuilder{T, TExternalResources}"/> for an optional "otherwise" block.</returns>
    /// <remarks>
    /// Use this for conditions affecting **multiple rules or a block of rules**, especially with an <c>OtherwiseUseAsync</c> clause.
    /// For conditions affecting **only a single property rule** without an <c>OtherwiseUseAsync</c>, consider
    /// <see cref="IPropertyRuleConditionComponentBuilder{T, TExternalResources, TProperty}.ContinueWhenAsync(AsyncValidationCondition{T, TExternalResources}, string?, int)"/> or its variants.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: Apply rules asynchronously if data is available
    /// builder.UseWhenAsync(async (ctx, ct) => await CheckDataAvailabilityAsync(ctx.ValidationInstance, ct), () =>
    /// {
    ///     builder.UseFor(x => x.DataPayload).NotNull();
    ///     builder.UseFor(x => x.DataPayload).Length(10, 100);
    /// });
    /// </code>
    /// </example>
    IOtherwiseConditionalAsyncFlowRuleBuilder<T, TExternalResources> UseWhenAsync(
        AsyncValidationCondition<T, TExternalResources> condition,
        Action rulesToUseWhenConditionIsMet,
        [CallerLineNumber] int callingFileLineNumber = 0);
}

/// <summary>
/// Defines the fluent API for specifying an **"otherwise" block** in a synchronous conditional flow.
/// Rules here are applied only when the condition from the preceding <c>UseWhen</c> evaluates to <see langword="false"/>.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <remarks>
/// This interface enables mutually exclusive rule sets. Rules within this block also follow a "continue-by-default" strategy.
/// The action receives the same builder, allowing **nesting** of further conditional blocks.
/// </remarks>
public interface IOtherwiseConditionalFlowRuleBuilder<T, TExternalResources>
{
    /// <summary>
    /// Defines a synchronous block of validation rules to be applied when the condition
    /// specified in the preceding <c>UseWhen</c> method evaluates to <see langword="false"/>.
    /// </summary>
    /// <param name="rulesToUseWhenConditionIsNotMet">Action to define rules when condition is not met. Supports **nesting**.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number.</param>
    /// <example>
    /// <code>
    /// // Example: Validate different fields based on user type
    /// builder.UseWhen(ctx => ctx.ValidationInstance.User.IsAdmin, () =>
    /// {
    ///     builder.UseFor(x => x.AdminSpecificField).NotNull();
    /// })
    /// .OtherwiseUse(() =>
    /// {
    ///     builder.UseFor(x => x.RegularUserField).NotEmpty();
    ///
    ///     // Nested conditional within OtherwiseUse
    ///     builder.UseWhen(ctx => ctx.ValidationInstance.User.IsGuest, () =>
    ///     {
    ///         builder.UseFor(x => x.GuestId).NotNull();
    ///     });
    /// });
    /// </code>
    /// </example>
    void OtherwiseUse(
        Action rulesToUseWhenConditionIsNotMet,
        [CallerLineNumber] int callingFileLineNumber = 0);
}

/// <summary>
/// Defines the fluent API for specifying an **"otherwise" block** in an asynchronous conditional flow.
/// Rules here are applied only when the condition from the preceding <c>UseWhenAsync</c> evaluates to <see langword="false"/> asynchronously.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
/// <remarks>
/// This interface enables mutually exclusive asynchronous rule sets. Rules within this block also follow a "continue-by-default" strategy.
/// The action receives the same builder, allowing **nesting** of further conditional blocks.
/// </remarks>
public interface IOtherwiseConditionalAsyncFlowRuleBuilder<T, TExternalResources>
{
    /// <summary>
    /// Defines an asynchronous block of validation rules to be applied when the condition
    /// specified in the preceding <c>UseWhenAsync</c> method evaluates to <see langword="false"/> asynchronously.
    /// </summary>
    /// <param name="rulesToUseWhenConditionIsNotMet">Action to define rules when condition is not met. Supports **nesting**.</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number.</param>
    /// <example>
    /// <code>
    /// // Example: Async validation based on API response
    /// builder.UseWhenAsync(async (ctx, ct) => await IsApiResponseSuccessfulAsync(ct), () =>
    /// {
    ///     builder.UseFor(x => x.ApiResultData).NotNull();
    /// })
    /// .OtherwiseUseAsync(() =>
    /// {
    ///     builder.UseFor(x => x.ApiErrorMessage).NotEmpty();
    ///
    ///     // Nested async conditional within OtherwiseUseAsync
    ///     builder.UseWhenAsync(async (ctx, ct) => await IsFallbackRequiredAsync(ct), () =>
    ///     {
    ///         builder.UseFor(x => x.FallbackData).NotNull();
    ///     });
    /// });
    /// </code>
    /// </example>
    void OtherwiseUseAsync(
        Action rulesToUseWhenConditionIsNotMet,
        [CallerLineNumber] int callingFileLineNumber = 0);
}