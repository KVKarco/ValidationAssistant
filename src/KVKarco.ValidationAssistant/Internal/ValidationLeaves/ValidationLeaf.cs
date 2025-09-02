using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Abstractions.ValidationRules;
using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.Utilities;
using KVKarco.ValidationAssistant.Internal.Utilities.TargetAssets;

namespace KVKarco.ValidationAssistant.Internal.ValidationRules;

/// <summary>
/// An abstract base class for a validation rule's leaf node.
/// </summary>
/// <remarks>
/// This class encapsulates the common, non-generic state for a validation leaf, including
/// the rule's message and its core options. It is designed to be immutable.
/// </remarks>
internal abstract class ValidationLeaf
{
    protected readonly string? _message;
    protected readonly IRuleOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationLeaf"/> class.
    /// </summary>
    /// <param name="message">The default failure message for the rule.</param>
    /// <param name="options">The non-generic options for the rule.</param>
    protected ValidationLeaf(
        string? message,
        IRuleOptions options)
    {
        _message = message;
        _options = options;
    }
}

/// <summary>
/// A sealed, immutable validation rule that represents a leaf node in the validation rule tree.
/// </summary>
/// <remarks>
/// This class is the final, read-only representation of a validation rule. It encapsulates all
/// the necessary logic and state for a rule to be executed, ensuring it is thread-safe and can
/// be safely cached for high-performance scenarios.
/// </remarks>
/// <typeparam name="T">The type of the object being validated.</typeparam>
/// <typeparam name="TResources">The type of the resources available during validation.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
internal sealed class ValidationLeaf<T, TResources, TProperty> :
    ValidationLeaf,
    IValidationLeaf<T, TResources, TProperty>
    where TResources : class
{
    private readonly IValidationRule<T, TResources, TProperty>? _rule;
    private readonly IAsyncValidationRule<T, TResources, TProperty>? _asyncRule;
    private readonly MessageFactory<T, TResources, TProperty>? _messageFactory;
    private readonly Condition<T, TResources, TProperty>? _condition;
    private readonly AsyncCondition<T, TResources, TProperty>? _asyncCondition;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationLeaf{T, TResources, TProperty}"/> class.
    /// </summary>
    /// <param name="options">The non-generic configuration options for the rule.</param>
    /// <param name="rule">The synchronous or asynchronous rule implementation.</param>
    /// <param name="condition">An optional synchronous or asynchronous condition delegate.</param>
    /// <param name="message">An optional default failure message.</param>
    /// <param name="messageFactory">An optional delegate for creating a dynamic failure message.</param>
    public ValidationLeaf(
        IRuleOptions options,
        IValidationRule rule,
        Delegate condition,
        string? message,
        MessageFactory<T, TResources, TProperty>? messageFactory) : base(message, options)
    {
        if (message is not null && messageFactory is not null)
        {
            throw new ValidationAssistantInternalException("debug how message and messageFactory are both present when creating ValidationLeaf.");
        }

        _messageFactory = messageFactory;

        if (condition is Condition<T, TResources, TProperty> c)
        {
            _condition = c;
            _asyncCondition = null;
        }
        else if (condition is AsyncCondition<T, TResources, TProperty> asyncC)
        {
            _condition = null;
            _asyncCondition = asyncC;
        }

        //handle rule
        if (rule is IValidationRule<T, TResources, TProperty> r)
        {
            _rule = r;
            _asyncRule = null;
        }
        else if (rule is IAsyncValidationRule<T, TResources, TProperty> asyncC)
        {
            _rule = null;
            _asyncRule = asyncC;
        }
        else
        {
            throw new ValidationAssistantInternalException("debug how sync and async rules are both missing when creating ValidationLeaf.");
        }
    }

    ///<inheritdoc/>
    public bool CanExceedSynchronously => _options.CanExceedSynchronously;

    ///<inheritdoc/>
    public void Execute(ValidationCtx<T, TResources> context, in Undefined<TProperty> undefined)
    {
        // Hint: First, validate that the property value is defined. This is a crucial early exit.
        if (_options.IsValidationRule)
            Ensure.PropertyValueIsDefined(context, undefined.MissingMember);

        // Hint: Check if the rule is designed to be executed synchronously.
        Ensure.RuleCanExecuteSynchronously(_options);

        // Hint: If a synchronous condition is attached, execute it and skip the rule if it fails.
        if (_condition is not null && !_condition(context, undefined.Value))
        {
            context.RuleSkipped(_options);
            return;
        }

        bool isValid = _rule!.IsValid(context, undefined.Value);
        Resolve(isValid, context, undefined);
    }

    ///<inheritdoc/>
    public async ValueTask ExecuteAsync(ValidationCtx<T, TResources> context, Undefined<TProperty> undefined, CancellationToken ct)
    {
        // Hint: First, validate that the property value is defined. This is a crucial early exit.
        if (_options.IsValidationRule)
            Ensure.PropertyValueIsDefined(context, undefined.MissingMember);

        // Hint: Execute either the sync or async condition based on which is present.
        if (_condition is not null || _asyncCondition is not null)
        {
            // Hint: This ternary operator is an elegant way to handle both cases.
            bool toContinue = _condition is null ?
                await _asyncCondition!(context, undefined.Value, ct).ConfigureAwait(false) :
                _condition(context, undefined.Value);
            if (!toContinue)
            {
                context.RuleSkipped(_options);
                return;
            }
        }

        bool isValid = _rule is not null ?
            _rule.IsValid(context, undefined.Value) :
            await _asyncRule!.IsValidAsync(context, undefined.Value, ct).ConfigureAwait(false);

        Resolve(isValid, context, undefined);
    }

    /// <summary>
    /// Resolves the outcome of the rule execution, notifying the context of success or failure.
    /// </summary>
    /// <param name="isValid">A value indicating whether the rule passed or failed.</param>
    /// <param name="context">The validation context.</param>
    /// <param name="undefined">The property value that was validated.</param>
    private void Resolve(bool isValid, ValidationCtx<T, TResources> context, in Undefined<TProperty> undefined)
    {
        if (isValid)
        {
            context.RulePassed(_options);
            return;
        }

        context.RuleFailed(
        _options,
        _message ??
            (_messageFactory is not null ?
                _messageFactory(context, undefined.Value) :
                (_rule is not null ?
                    _rule.GetDefaultFailureMessage(context, undefined.Value) :
                    _asyncRule!.GetDefaultFailureMessage(context, undefined.Value))));
    }
}