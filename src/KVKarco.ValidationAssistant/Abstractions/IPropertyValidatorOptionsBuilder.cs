using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

public interface IPropertyValidatorOptionsBuilder<T, TExternalResources, out TProperty>
{
    IPropertyValidatorBuilder<T, TExternalResources, TProperty> WithFlowImpact(
    FlowEffect flowEffect,
    [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> DefaultRuleFlowEffect(
        FlowEffect flowEffect,
        [CallerLineNumber] int callingFileLineNumber = 0);

    void DefaultRuleSeverity(
        Severity severity,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> Capture(
        string snapShot,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> ProceedWhen(
        string snapShot,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> ProceedWhen(
        Condition<T, TExternalResources, TProperty> condition,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> ProceedWhenAsync(
        AsyncCondition<T, TExternalResources, TProperty> asyncCondition,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> ProceedUnless(
        string snapShot,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> ProceedUnless(
        Condition<T, TExternalResources, TProperty> condition,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> ProceedUnlessAsync(
        AsyncCondition<T, TExternalResources, TProperty> asyncCondition,
        [CallerLineNumber] int callingFileLineNumber = 0);
}
