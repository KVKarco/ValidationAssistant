using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

public interface IConditionalFlowRuleBuilder<T, TExternalResources>
{
    IOtherwiseConditionalFlowRuleBuilder<T, TExternalResources> UseWhen(
        ValidationCondition<T, TExternalResources> condition,
        Action rulesToUseWhenConditionIsMet,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IOtherwiseConditionalAsyncFlowRuleBuilder<T, TExternalResources> UseWhenAsync(
        AsyncValidationCondition<T, TExternalResources> condition,
        Action rulesToUseWhenConditionIsMet,
        [CallerLineNumber] int callingFileLineNumber = 0);
}

public interface IOtherwiseConditionalFlowRuleBuilder<T, TExternalResources>
{
    void OtherwiseUse(
        Action rulesToUseWhenConditionIsNotMet,
        [CallerLineNumber] int callingFileLineNumber = 0);
}

public interface IOtherwiseConditionalAsyncFlowRuleBuilder<T, TExternalResources>
{
    void OtherwiseUseAsync(
        Action rulesToUseWhenConditionIsNotMet,
        [CallerLineNumber] int callingFileLineNumber = 0);
}