using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

/// <summary>
/// Defines the fluent API for initiating a conditional validation block.
/// This interface allows you to specify a condition, and then define a set of rules
/// that should only be applied when that condition is met. It provides both
/// synchronous and asynchronous options for the condition evaluation.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
public interface IConditionalFlowRuleBuilder<T, TExternalResources>
{
    /// <summary>
    /// Initiates a synchronous conditional validation block. Rules defined within the
    /// <paramref name="rulesToUseWhenConditionIsMet"/> action will only be applied
    /// if the provided <paramref name="condition"/> returns <see langword="true"/>.
    /// </summary>
    /// <param name="condition">A synchronous predicate that determines whether the rules
    /// in <paramref name="rulesToUseWhenConditionIsMet"/> should be executed.</param>
    /// <param name="rulesToUseWhenConditionIsMet">An action that contains the fluent API calls
    /// to define the validation rules to be applied when the condition is met. These rules
    /// will be defined using the <c>builder</c> instance from the outer scope (e.g., the
    /// <c>ICoreValidationDefinitionBuilder</c> passed to <c>ExpressRules</c>).</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file
    /// where this method is called. Used for debugging/reporting.</param>
    /// <returns>An <see cref="IOtherwiseConditionalFlowRuleBuilder{T, TExternalResources}"/>
    /// to optionally define rules for when the condition is not met.</returns>
    /// <example>
    /// <code>
    /// // Example usage within an ExpressRules method:
    /// builder.UseWhen(ctx => ctx.ValidationInstance.User.Name is not null, () =>
    /// {
    ///     builder.UseFor(x => x.User.Name).Length(1, 50);
    /// });
    /// </code>
    /// </example>
    IOtherwiseConditionalFlowRuleBuilder<T, TExternalResources> UseWhen(
        ValidationCondition<T, TExternalResources> condition,
        Action rulesToUseWhenConditionIsMet,
        [CallerLineNumber] int callingFileLineNumber = 0);

    /// <summary>
    /// Initiates an asynchronous conditional validation block. Rules defined within the
    /// <paramref name="rulesToUseWhenConditionIsMet"/> action will only be applied
    /// if the provided <paramref name="condition"/> returns <see langword="true"/> asynchronously.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether the rules
    /// in <paramref name="rulesToUseWhenConditionIsMet"/> should be executed.</param>
    /// <param name="rulesToUseWhenConditionIsMet">An action that contains the fluent API calls
    /// to define the validation rules to be applied when the condition is met. These rules
    /// will be defined using the <c>builder</c> instance from the outer scope (e.g., the
    /// <c>ICoreValidationDefinitionBuilder</c> passed to <c>ExpressRules</c>).</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file
    /// where this method is called. Used for debugging/reporting.</param>
    /// <returns>An <see cref="IOtherwiseConditionalAsyncFlowRuleBuilder{T, TExternalResources}"/>
    /// to optionally define rules for when the condition is not met.</returns>
    /// <example>
    /// <code>
    /// // Example usage within an ExpressRules method:
    /// builder.UseWhenAsync(async (ctx, ct) => await SomeAsyncCondition(ctx.ValidationInstance, ct), () =>
    /// {
    ///     builder.UseFor(x => x.SomeAsyncProperty).EnsureAsync(async (val, ct) => await SomeAsyncCheck(val, ct));
    /// });
    /// </code>
    /// </example>
    IOtherwiseConditionalAsyncFlowRuleBuilder<T, TExternalResources> UseWhenAsync(
        AsyncValidationCondition<T, TExternalResources> condition,
        Action rulesToUseWhenConditionIsMet,
        [CallerLineNumber] int callingFileLineNumber = 0);
}

/// <summary>
/// Defines the fluent API for specifying an "otherwise" block in a synchronous conditional validation flow.
/// This interface allows you to define a set of rules that should be applied only when the
/// condition specified in the preceding <see cref="IConditionalFlowRuleBuilder{T, TExternalResources}.UseWhen"/>
/// method evaluates to <see langword="false"/>.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
public interface IOtherwiseConditionalFlowRuleBuilder<T, TExternalResources>
{
    /// <summary>
    /// Defines a synchronous block of validation rules to be applied when the condition
    /// specified in the preceding <c>UseWhen</c> method evaluates to <see langword="false"/>.
    /// </summary>
    /// <param name="rulesToUseWhenConditionIsNotMet">An action that contains the fluent API calls
    /// to define the validation rules to be applied when the condition is not met. These rules
    /// will be defined using the <c>builder</c> instance from the outer scope (e.g., the
    /// <c>ICoreValidationDefinitionBuilder</c> passed to <c>ExpressRules</c>).</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file
    /// where this method is called. Used for debugging/reporting.</param>
    /// <example>
    /// <code>
    /// // Example usage within an ExpressRules method:
    /// builder.UseWhen(ctx => ctx.ValidationInstance.User.Name is not null, () =>
    /// {
    ///     builder.UseFor(x => x.User.Name).Length(1, 50);
    /// })
    /// .OtherwiseUse(() =>
    /// {
    ///     builder.UseFor(x => x.User.Email).NotNull();
    /// });
    /// </code>
    /// </example>
    void OtherwiseUse(
        Action rulesToUseWhenConditionIsNotMet,
        [CallerLineNumber] int callingFileLineNumber = 0);
}

/// <summary>
/// Defines the fluent API for specifying an "otherwise" block in an asynchronous conditional validation flow.
/// This interface allows you to define a set of rules that should be applied only when the
/// condition specified in the preceding <see cref="IConditionalFlowRuleBuilder{T, TExternalResources}.UseWhenAsync"/>
/// method evaluates to <see langword="false"/> asynchronously.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type providing external resources or dependencies.</typeparam>
public interface IOtherwiseConditionalAsyncFlowRuleBuilder<T, TExternalResources>
{
    /// <summary>
    /// Defines an asynchronous block of validation rules to be applied when the condition
    /// specified in the preceding <c>UseWhenAsync</c> method evaluates to <see langword="false"/> asynchronously.
    /// </summary>
    /// <param name="rulesToUseWhenConditionIsNotMet">An action that contains the fluent API calls
    /// to define the validation rules to be applied when the condition is not met. These rules
    /// may themselves be synchronous or asynchronous. These rules will be defined using the
    /// <c>builder</c> instance from the outer scope (e.g., the <c>ICoreValidationDefinitionBuilder</c>
    /// passed to <c>ExpressRules</c>).</param>
    /// <param name="callingFileLineNumber">Automatically captures the line number in the source file
    /// where this method is called. Used for debugging/reporting.</param>
    /// <example>
    /// <code>
    /// // Example usage within an ExpressRules method:
    /// builder.UseWhenAsync(async (ctx, ct) => await SomeAsyncCondition(ctx.ValidationInstance, ct), () =>
    /// {
    ///     builder.UseFor(x => x.SomeAsyncProperty).EnsureAsync(async (val, ct) => await SomeAsyncCheck(val, ct));
    /// })
    /// .OtherwiseUseAsync(() =>
    /// {
    ///     builder.UseFor(x => x.AnotherProperty).NotNull();
    /// });
    /// </code>
    /// </example>
    void OtherwiseUseAsync(
        Action rulesToUseWhenConditionIsNotMet,
        [CallerLineNumber] int callingFileLineNumber = 0);
}