namespace KVKarco.ValidationAssistant.Abstractions;

public interface IValidationRuleOptionsBuilder<TSubject, TResources, out TProperty>
{
    IValidationRuleOptionsBuilder<TSubject, TResources, TProperty> WithMessage(
        string message);

    IValidationRuleOptionsBuilder<TSubject, TResources, TProperty> WithMessage(
        MessageFactory<TSubject, TResources, TProperty> messageFactory);

    IValidationRuleOptionsBuilder<TSubject, TResources, TProperty> WithSeverity(
        Severity severity);

    IValidationRuleOptionsBuilder<TSubject, TResources, TProperty> WithFlowImpact(
        FlowEffect effect);

    IValidationRuleOptionsBuilder<TSubject, TResources, TProperty> WithCode(
        string code);

    IValidationRuleOptionsBuilder<TSubject, TResources, TProperty> ApplyWhen(
        string snapShot);

    IValidationRuleOptionsBuilder<TSubject, TResources, TProperty> ApplyUnless(
        string snapShot);

    IValidationRuleOptionsBuilder<TSubject, TResources, TProperty> ApplyWhen(
        Condition<TSubject, TResources> condition);

    IValidationRuleOptionsBuilder<TSubject, TResources, TProperty> ApplyUnless(
        Condition<TSubject, TResources> condition);

    IValidationRuleOptionsBuilder<TSubject, TResources, TProperty> ApplyWhenAsync(
        AsyncCondition<TSubject, TResources> condition);

    IValidationRuleOptionsBuilder<TSubject, TResources, TProperty> ApplyUnlessAsync(
        AsyncCondition<TSubject, TResources> condition);
}
