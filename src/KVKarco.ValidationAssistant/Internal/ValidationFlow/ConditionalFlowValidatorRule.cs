
using KVKarco.ValidationAssistant.Exceptions;

namespace KVKarco.ValidationAssistant.Internal.ValidationFlow;

/// <summary>
/// Represents a special internal validator rule responsible for managing conditional flow within the validation pipeline.
/// This rule evaluates a given condition (synchronously or asynchronously) and, based on its outcome,
/// instructs the <see cref="ValidatorRunCtx{T, TExternalResources}"/> to skip a specified number of subsequent rules.
/// This enables "UseWhen" and "OtherwiseUse" constructs without nested rule execution logic at runtime,
/// maintaining a flat rule collection.
/// </summary>
/// <typeparam name="T">The type of the main instance being validated.</typeparam>
/// <typeparam name="TExternalResources">The type of external resources available during validation.</typeparam>
/// <typeparam name="TContext">The specific type of <see cref="ValidatorRunCtx{T, TExternalResources}"/>
/// used for the validation run.</typeparam>
internal sealed class ConditionalFlowValidatorRule<T, TExternalResources, TContext> :
    ValidatorRule<T, TExternalResources, TContext>
    where TContext : ValidatorRunCtx<T, TExternalResources>
{
    private readonly int _rulesToSkip;
    private readonly bool _isOtherwise;
    private readonly ValidationCondition<T, TExternalResources>? _condition;
    private readonly AsyncValidationCondition<T, TExternalResources>? _asyncCondition;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalFlowValidatorRule{T, TExternalResources, TContext}"/>
    /// for a synchronous condition.
    /// </summary>
    /// <param name="validatorName">The name of the validator this rule belongs to.</param>
    /// <param name="rulesToSkip">The number of subsequent rules to skip if the condition is not met (for 'when' block)
    /// or if the condition IS met (for 'otherwise' block).</param>
    /// <param name="isOtherwise">A flag indicating if this rule represents an 'otherwise' condition block.</param>
    /// <param name="condition">The synchronous condition predicate to evaluate.</param>
    /// <param name="declaredOnLine">The line number in the source code where this rule was declared.</param>
    public ConditionalFlowValidatorRule(
        ReadOnlySpan<char> validatorName,
        int rulesToSkip,
        bool isOtherwise,
        ValidationCondition<T, TExternalResources>? condition,
        int declaredOnLine)
        : base(true) // CanRunSynchronously is true for synchronous condition
    {
        _rulesToSkip = rulesToSkip;
        _condition = condition;
        _isOtherwise = isOtherwise;
        _asyncCondition = null; // Ensure async condition is null for synchronous constructor
        Info = new LogicalRuleFailureInfo<T, TExternalResources>(
            rulesToSkip, // This 'rulesToSkip' is passed to the LogicalRuleFailureInfo
            (ctx) => ValidatorsConfig.GlobalDefaults.Messages.ConditionalFlowBlockSkipExplanation(ctx, rulesToSkip), // Use the passed rulesToSkip
            validatorName, RuleName, declaredOnLine, RuleFailureStrategy.Continue); // Conditional flow rules do not stop the overall validation flow, they just redirect it.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalFlowValidatorRule{T, TExternalResources, TContext}"/>
    /// for an asynchronous condition.
    /// </summary>
    /// <param name="validatorName">The name of the validator this rule belongs to.</param>
    /// <param name="rulesToSkip">The number of subsequent rules to skip if the condition is not met (for 'when' block)
    /// or if the condition IS met (for 'otherwise' block).</param>
    /// <param name="isOtherwise">A flag indicating if this rule represents an 'otherwise' condition block.</param>
    /// <param name="asyncCondition">The asynchronous condition predicate to evaluate.</param>
    /// <param name="declaredOnLine">The line number in the source code where this rule was declared.</param>
    public ConditionalFlowValidatorRule(
        ReadOnlySpan<char> validatorName,
        int rulesToSkip,
        bool isOtherwise,
        AsyncValidationCondition<T, TExternalResources>? asyncCondition,
        int declaredOnLine)
        : base(false) // CanRunSynchronously is false for asynchronous condition
    {
        _rulesToSkip = rulesToSkip;
        _condition = null; // Ensure synchronous condition is null for asynchronous constructor
        _isOtherwise = isOtherwise;
        _asyncCondition = asyncCondition;
        Info = new LogicalRuleFailureInfo<T, TExternalResources>(
            rulesToSkip, // This 'rulesToSkip' is passed to the LogicalRuleFailureInfo
            (ctx) => ValidatorsConfig.GlobalDefaults.Messages.ConditionalFlowBlockSkipExplanation(ctx, rulesToSkip), // Use the passed rulesToSkip
            validatorName, RuleName, declaredOnLine, RuleFailureStrategy.Continue); // Conditional flow rules do not stop the overall validation flow, they just redirect it.
    }

    /// <summary>
    /// Gets the logical rule failure information for this conditional flow rule.
    /// This includes the number of rules skipped and the explanation message.
    /// </summary>
    public sealed override LogicalRuleFailureInfo<T, TExternalResources> Info { get; }

    /// <summary>
    /// Gets the descriptive name of this conditional flow rule, indicating whether it's a 'when' or 'otherwise' rule,
    /// and if it's synchronous or asynchronous.
    /// </summary>
    public sealed override ReadOnlySpan<char> RuleName => _condition is not null
        ? (_isOtherwise ? DefaultNaming.ConditionOtherwiseRule : DefaultNaming.ConditionWhenRule)
        : (_isOtherwise ? DefaultNaming.ConditionOtherwiseAsyncRule : DefaultNaming.ConditionWhenAsyncRule);

    /// <summary>
    /// Synchronously executes the conditional flow logic.
    /// If the condition evaluates to <see langword="false"/> (for a 'when' block)
    /// or <see langword="true"/> (for an 'otherwise' block), it instructs the context
    /// to skip the specified number of rules.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <exception cref="ValidationRunException">Thrown if this asynchronous rule is called synchronously.</exception>
    public sealed override void Validate(TContext context)
    {
        ValidationRunException.ThrowIfAsyncRuleIsCalledSynchronously(CanRunSynchronously, RuleName);
        bool isValidCondition = _condition!(context); // Execute the synchronous condition

        // For an 'otherwise' block, we skip if the condition IS valid (meaning the 'when' block was executed).
        // For a 'when' block, we skip if the condition is NOT valid.
        if (_isOtherwise)
        {
            if (isValidCondition) // If 'otherwise' condition is true, it means the 'when' condition was also true, so skip 'otherwise' block.
            {
                context.AddLogicalRuleFailure(Info); // Add a logical failure to indicate skipping
            }
        }
        else // This is a 'when' block
        {
            if (!isValidCondition) // If 'when' condition is false, skip the 'when' block.
            {
                context.AddLogicalRuleFailure(Info); // Add a logical failure to indicate skipping
            }
        }
    }

    /// <summary>
    /// Asynchronously executes the conditional flow logic.
    /// If the condition evaluates to <see langword="false"/> (for a 'when' block)
    /// or <see langword="true"/> (for an 'otherwise' block), it instructs the context
    /// to skip the specified number of rules.
    /// </summary>
    /// <param name="context">The validation run context.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    public sealed override async ValueTask ValidateAsync(TContext context, CancellationToken ct)
    {
        // Execute the condition, either synchronously or asynchronously based on CanRunSynchronously.
        bool isValidCondition = CanRunSynchronously
            ? _condition!(context)
            : await _asyncCondition!(context, ct).ConfigureAwait(false);

        // Logic for skipping based on _isOtherwise flag and condition result.
        if (_isOtherwise)
        {
            if (isValidCondition) // If 'otherwise' condition is true, it means the 'when' condition was also true, so skip 'otherwise' block.
            {
                context.AddLogicalRuleFailure(Info); // Add a logical failure to indicate skipping
            }
        }
        else // This is a 'when' block
        {
            if (!isValidCondition) // If 'when' condition is false, skip the 'when' block.
            {
                context.AddLogicalRuleFailure(Info); // Add a logical failure to indicate skipping
            }
        }
    }
}
