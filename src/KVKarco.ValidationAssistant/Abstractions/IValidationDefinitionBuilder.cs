using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

public interface IValidationDefinitionBuilder<T, TExternalResources>
{
    IPropertyValidatorBuilder<T, TExternalResources, TProperty> UseFor<TProperty>(
        Expression<Func<T, TProperty>> selector,
        [CallerLineNumber] int callingFileLineNumber = 0);

    void UseForEach<TElement>(
    Expression<Func<T, IEnumerable<TElement>>> selector,
    Action<IPropertyValidatorBuilder<T, TExternalResources, TElement>> elementBuilder,
    [CallerLineNumber] int callingFileLineNumber = 0);

    void DefaultRuleFlowEffect(
        FlowEffect effect,
        [CallerLineNumber] int callingFileLineNumber = 0);

    void DefaultComponentFlowEffect(
        FlowEffect effect,
        [CallerLineNumber] int callingFileLineNumber = 0);

    void DefaultRuleSeverity(
        Severity severity,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<T, TExternalResources> UseWhen(
        string snapShot,
        Action componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<T, TExternalResources> UseWhen(
        Condition<T, TExternalResources> condition,
        Action componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<T, TExternalResources> UseWhenAsync(
        AsyncCondition<T, TExternalResources> condition,
        Action componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<T, TExternalResources> UseUnless(
        string snapShot,
        Action componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<T, TExternalResources> UseUnless(
        Condition<T, TExternalResources> condition,
        Action componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<T, TExternalResources> UseUnlessAsync(
        AsyncCondition<T, TExternalResources> condition,
        Action componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);
}
