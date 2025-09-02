using KVKarco.ValidationAssistant.Abstractions;

namespace KVKarco.ValidationAssistant.Internal.ValidationLeafAssets;

internal sealed class ValidationLeafOptions<TSubject, TResources, TTarget> :
    IValidationLeafOptions<TSubject, TResources, TTarget>
{
    public ValidationLeafOptions(
        bool isValidationRule,
        string code,
        int declarationLine,
        FlowEffect onFailureFlowEffect,
        Severity failureSeverity,
        string? failureMessage,
        MessageFactory<TSubject, TResources, TTarget>? failureMessageFactory,
        Condition<TSubject, TResources>? condition,
        AsyncCondition<TSubject, TResources>? asyncCondition,
        bool isWhen)
    {
        IsValidationRule = isValidationRule;
        Code = code;
        DeclarationLine = declarationLine;
        OnFailureFlowEffect = onFailureFlowEffect;
        FailureSeverity = failureSeverity;
        FailureMessage = failureMessage;
        FailureMessageFactory = failureMessageFactory;
        Condition = condition;
        AsyncCondition = asyncCondition;
        IsConditionWhen = isWhen;
    }

    public bool IsValidationRule { get; }

    public string Code { get; }

    public int DeclarationLine { get; }

    public FlowEffect OnFailureFlowEffect { get; }

    public Severity FailureSeverity { get; }

    public string? FailureMessage { get; }

    public MessageFactory<TSubject, TResources, TTarget>? FailureMessageFactory { get; }

    public Condition<TSubject, TResources>? Condition { get; }

    public AsyncCondition<TSubject, TResources>? AsyncCondition { get; }

    public bool IsConditionWhen { get; }

    public bool IsConditionAsync => AsyncCondition is not null;
}
