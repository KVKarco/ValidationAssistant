using KVKarco.ValidationAssistant.Abstractions.ValidationRules;
using KVKarco.ValidationAssistant.Internal.Utilities;
using KVKarco.ValidationAssistant.Internal.Utilities.TargetAssets;
using KVKarco.ValidationAssistant.Internal.ValidationLeafAssets;

namespace KVKarco.ValidationAssistant.Internal.CustomValidatorAssets;

internal sealed class CustomValidatorValidationComponent<TSubject, TResources, TTarget>
    : IValidatorLeaf<TSubject, TResources, TTarget, CustomValidatorRunCtx<TSubject, TResources>>
{
    private readonly IValidationLeafOptions<TSubject, TResources, TTarget> _options;
    private readonly IValidationRule<TSubject, TResources, TTarget>? _rule;
    private readonly IAsyncValidationRule<TSubject, TResources, TTarget>? _asyncRule;

    public CustomValidatorValidationComponent(
        IValidationRule<TSubject, TResources, TTarget> rule,
        IValidationLeafOptions<TSubject, TResources, TTarget> options)
    {
        _rule = rule;
        _asyncRule = null; // Initialize to null, will be set in the async constructor
        _options = options;
        CanExecuteSynchronously = _options.AsyncCondition is null;
    }

    public CustomValidatorValidationComponent(
        IAsyncValidationRule<TSubject, TResources, TTarget> asyncRule,
        IValidationLeafOptions<TSubject, TResources, TTarget> options)
    {
        _rule = null; // Initialize to null, will be set in the sync constructor
        _asyncRule = asyncRule;
        _options = options;
        CanExecuteSynchronously = false;
    }

    public bool CanExecuteSynchronously { get; }

    public void Execute(CustomValidatorRunCtx<TSubject, TResources> context, Undefined<TTarget> value)
    {
        //all must guard in sync mode if there is a async condition or the rule its self is async.
        Ensure.NotAsyncComponent(context, _options, CanExecuteSynchronously);

        if (_options.IsValidationRule)
        {
            Ensure.TargetValueIsDefined(context, _options, value);
        }

        if (_options.HasConditionAttached)
        {
            bool isValidCondition = _options.Condition!(context);
            bool toSkipRule = _options.IsConditionWhen ? !isValidCondition : isValidCondition;

            if (toSkipRule)
            {
                context.AddRuleSkipped(_options);
                return;
            }
        }

        bool isValid = _rule!.IsValid(context, value.Value);

        if (isValid)
        {
            context.AddRuleSuccess(_options);
        }
        else
        {
            context.AddRuleFailure(_options, value, _options.HasAlternateFailureMessage ? null : _rule.GetDefaultFailureMessage(context, value.Value));
        }
    }

    public async ValueTask ExecuteAsync(CustomValidatorRunCtx<TSubject, TResources> context, Undefined<TTarget> value, CancellationToken ct)
    {
        if (_options.IsValidationRule)
        {
            Ensure.TargetValueIsDefined(context, _options, value);
        }

        if (_options.HasConditionAttached)
        {
            bool isValidCondition = _options.Condition is not null
                ? _options.Condition(context)
                : await _options.AsyncCondition!(context, ct).ConfigureAwait(false);

            bool toSkipRule = _options.IsConditionWhen ? !isValidCondition : isValidCondition;

            if (toSkipRule)
            {
                context.AddRuleSkipped(_options);
                return;
            }
        }

        bool isValid = _rule is not null
            ? _rule.IsValid(context, value.Value)
            : await _asyncRule!.IsValidAsync(context, value.Value, ct).ConfigureAwait(false);

        if (isValid)
        {
            context.AddRuleSuccess(_options);
        }
        else
        {
            context.AddRuleFailure(
                _options,
                value,
                _options.HasAlternateFailureMessage
                ? null
                : (_rule is not null ? _rule.GetDefaultFailureMessage(context, value.Value) : _asyncRule!.GetDefaultFailureMessage(context, value.Value)));
        }
    }
}
