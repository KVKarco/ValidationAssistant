namespace KVKarco.ValidationAssistant.Abstractions;

public interface IValidationRuleOptionsBuilder<T, TExternalResources, out TProperty>
{
    IValidationRuleOptionsBuilder<T, TExternalResources, TProperty> WithMessage(
        string message);

    IValidationRuleOptionsBuilder<T, TExternalResources, TProperty> WithMessage(
        MessageFactory<T, TExternalResources, TProperty> messageFactory);

    IValidationRuleOptionsBuilder<T, TExternalResources, TProperty> WithSeverity(
        Severity severity);

    IValidationRuleOptionsBuilder<T, TExternalResources, TProperty> WithFlowImpact(
        FlowEffect effect);

    IValidationRuleOptionsBuilder<T, TExternalResources, TProperty> WithCode(
        string code);

    IValidationRuleOptionsBuilder<T, TExternalResources, TProperty> UseWhen(
        string snapShot);

    IValidationRuleOptionsBuilder<T, TExternalResources, TProperty> UseUnless(
        string snapShot);

    IValidationRuleOptionsBuilder<T, TExternalResources, TProperty> UseWhen(
        Condition<T, TExternalResources, TProperty> condition);

    IValidationRuleOptionsBuilder<T, TExternalResources, TProperty> UseUnless(
        Condition<T, TExternalResources, TProperty> condition);

    IValidationRuleOptionsBuilder<T, TExternalResources, TProperty> UseWhenAsync(
        AsyncCondition<T, TExternalResources, TProperty> condition);

    IValidationRuleOptionsBuilder<T, TExternalResources, TProperty> UseUnlessAsync(
        AsyncCondition<T, TExternalResources, TProperty> condition);
}
