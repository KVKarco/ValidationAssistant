using KVKarco.ValidationAssistant.Abstractions;
using KVKarco.ValidationAssistant.Internal.Utilities;

namespace KVKarco.ValidationAssistant.Internal.ValidationLeafAssets;

internal sealed class ValidationLeafOptionsBuilder<TSubject, TResources, TTarget> :
    IValidationRuleOptionsBuilder<TSubject, TResources, TTarget>,
    IValidationLeafOptionsBuilder<TSubject, TResources, TTarget>
{
    private readonly List<string>? _snapShots;
    private readonly FlowEffect _defaultOnFailureFlowEffect;
    private readonly Severity _defaultFailureSeverity;

    private bool _isValidationRule;
    private string _code;
    private int _declarationLine;
    private FlowEffect _onFailureFlowEffect;
    private Severity _failureSeverity;
    private string? _failureMessage;
    private MessageFactory<TSubject, TResources, TTarget>? _failureMessageFactory;
    private Condition<TSubject, TResources>? _condition;
    private AsyncCondition<TSubject, TResources>? _asyncCondition;
    private bool _isWhen;

    public ValidationLeafOptionsBuilder(FlowEffect defaultOnFailureFlowEffect, Severity defaultFailureSeverity, List<string>? snapShots)
    {
        _code = string.Empty;
        _defaultFailureSeverity = defaultFailureSeverity;
        _defaultOnFailureFlowEffect = defaultOnFailureFlowEffect;
        _snapShots = snapShots;
    }

    public void NextRule(int declarationLine, string code, bool isValidationRule)
    {
        _isValidationRule = isValidationRule;
        _declarationLine = declarationLine;
        _code = code;
        _onFailureFlowEffect = _defaultOnFailureFlowEffect;
        _failureSeverity = _defaultFailureSeverity;
        _failureMessage = null;
        _failureMessageFactory = null;
        _condition = null;
        _asyncCondition = null;
    }

    public IValidationLeafOptions<TSubject, TResources, TTarget> Create()
    {
        return new ValidationLeafOptions<TSubject, TResources, TTarget>(
            _isValidationRule,
            _code,
            _declarationLine,
            _onFailureFlowEffect,
            _failureSeverity,
            _failureMessage,
            _failureMessageFactory,
            _condition,
            _asyncCondition,
            _isWhen);
    }

    public IValidationRuleOptionsBuilder<TSubject, TResources, TTarget> UseUnless(string snapShot)
    {
        Ensure.SnapShotNotEmptyAndIsCaptured(snapShot, _snapShots);
        _asyncCondition = null;
        string localSnapShot = snapShot;
        _condition = ctx => ctx.IsSnapShotValid(localSnapShot);
        _isWhen = false;
        return this;
    }

    public IValidationRuleOptionsBuilder<TSubject, TResources, TTarget> UseUnless(Condition<TSubject, TResources> condition)
    {
        Ensure.IsValidCondition(condition);
        _asyncCondition = null;
        _condition = condition;
        _isWhen = false;
        return this;
    }

    public IValidationRuleOptionsBuilder<TSubject, TResources, TTarget> UseUnlessAsync(AsyncCondition<TSubject, TResources> condition)
    {
        Ensure.IsValidCondition(condition);
        _condition = null;
        _asyncCondition = condition;
        _isWhen = false;
        return this;
    }

    public IValidationRuleOptionsBuilder<TSubject, TResources, TTarget> UseWhen(string snapShot)
    {
        Ensure.SnapShotNotEmptyAndIsCaptured(snapShot, _snapShots);
        _asyncCondition = null;
        string localSnapShot = snapShot;
        _condition = ctx => ctx.IsSnapShotValid(localSnapShot);
        _isWhen = true;
        return this;
    }

    public IValidationRuleOptionsBuilder<TSubject, TResources, TTarget> UseWhen(Condition<TSubject, TResources> condition)
    {
        Ensure.IsValidCondition(condition);
        _asyncCondition = null;
        _condition = condition;
        _isWhen = true;
        return this;
    }

    public IValidationRuleOptionsBuilder<TSubject, TResources, TTarget> UseWhenAsync(AsyncCondition<TSubject, TResources> condition)
    {
        Ensure.IsValidCondition(condition);
        _condition = null;
        _asyncCondition = condition;
        _isWhen = true;
        return this;
    }

    public IValidationRuleOptionsBuilder<TSubject, TResources, TTarget> WithCode(string code)
    {
        Ensure.IsValidCode(code);
        _code = code;
        return this;
    }

    public IValidationRuleOptionsBuilder<TSubject, TResources, TTarget> WithFlowImpact(FlowEffect effect)
    {
        _onFailureFlowEffect = effect;
        return this;
    }

    public IValidationRuleOptionsBuilder<TSubject, TResources, TTarget> WithMessage(string message)
    {
        Ensure.MessageNotEmpty(message);
        _failureMessage = message;
        _failureMessageFactory = null;
        return this;
    }

    public IValidationRuleOptionsBuilder<TSubject, TResources, TTarget> WithMessage(MessageFactory<TSubject, TResources, TTarget> messageFactory)
    {
        Ensure.IsValidMessage(messageFactory);
        _failureMessage = null;
        _failureMessageFactory = messageFactory;
        return this;
    }

    public IValidationRuleOptionsBuilder<TSubject, TResources, TTarget> WithSeverity(Severity severity)
    {
        _failureSeverity = severity;
        return this;
    }
}