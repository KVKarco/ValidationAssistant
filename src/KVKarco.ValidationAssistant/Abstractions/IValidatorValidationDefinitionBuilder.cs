using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

public interface IValidatorValidationDefinitionBuilder<TSubject, TResources>
{
    ITargetValidatorComponentBuilder<TSubject, TResources, TProperty> ApplyFor<TProperty>(
        Expression<Func<TSubject, TProperty>> selector,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<TSubject, TResources> ApplyWhen(
        string snapShot,
        Action<IValidatorValidationDefinitionBuilder<TSubject, TResources>> componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<TSubject, TResources> ApplyWhen(
        Condition<TSubject, TResources> condition,
        Action<IValidatorValidationDefinitionBuilder<TSubject, TResources>> componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<TSubject, TResources> ApplyWhenAsync(
        AsyncCondition<TSubject, TResources> condition,
        Action<IValidatorValidationDefinitionBuilder<TSubject, TResources>> componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<TSubject, TResources> ApplyUnless(
        string snapShot,
        Action<IValidatorValidationDefinitionBuilder<TSubject, TResources>> componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<TSubject, TResources> ApplyUnless(
        Condition<TSubject, TResources> condition,
        Action<IValidatorValidationDefinitionBuilder<TSubject, TResources>> componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IConditionalBlockBuilder<TSubject, TResources> ApplyUnlessAsync(
        AsyncCondition<TSubject, TResources> condition,
        Action<IValidatorValidationDefinitionBuilder<TSubject, TResources>> componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);
}
