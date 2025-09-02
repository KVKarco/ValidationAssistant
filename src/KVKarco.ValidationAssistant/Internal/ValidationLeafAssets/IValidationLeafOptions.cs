using KVKarco.ValidationAssistant.Abstractions;

namespace KVKarco.ValidationAssistant.Internal.ValidationLeafAssets;

internal interface IValidationLeafOptions
{
    public string Code { get; }

    public int DeclarationLine { get; }

    public FlowEffect OnFailureFlowEffect { get; }

    public Severity FailureSeverity { get; }

    public bool IsConditionWhen { get; }

    public bool IsValidationRule { get; }

    public string? FailureMessage { get; }

    public bool HasAsyncCondition { get; }
}

internal interface IValidationLeafOptions<TSubject, TResources, TTarget> :
    IValidationLeafOptions
{
    public MessageFactory<TSubject, TResources, TTarget>? FailureMessageFactory { get; }

    public Condition<TSubject, TResources>? Condition { get; }

    public AsyncCondition<TSubject, TResources>? AsyncCondition { get; }

    public bool HasAlternateFailureMessage => FailureMessage is not null || FailureMessageFactory is not null;

    public bool HasConditionAttached => Condition is not null || AsyncCondition is not null;
}
