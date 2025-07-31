using KVKarco.ValidationAssistant.ValidationRules;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

public interface IPropertyValidatorBuilder<T, TExternalResources, out TProperty> :
    IPropertyValidatorOptionsBuilder<T, TExternalResources, TProperty>
{
    IPropertyValidatorBuilder<T, TExternalResources, TProperty> Ensure(
        Predicate<TProperty> predicate,
        Action<IValidationRuleOptionsBuilder<T, TExternalResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> Ensure(
       Predicate<T, TExternalResources, TProperty> predicate,
       Action<IValidationRuleOptionsBuilder<T, TExternalResources, TProperty>>? options = null,
       [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> EnsureAsync(
        AsyncPredicate<T, TExternalResources, TProperty> predicate,
        Action<IValidationRuleOptionsBuilder<T, TExternalResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> EnsureRule(
        IValidationRule<T, TExternalResources, TProperty> rule,
        Action<IValidationRuleOptionsBuilder<T, TExternalResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> EnsureRuleAsync(
        IAsyncValidationRule<T, TExternalResources, TProperty> rule,
        Action<IValidationRuleOptionsBuilder<T, TExternalResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> EnsureValidator(
        Func<TExternalResources, ICustomValidator<TProperty>> validatorFactory,
        Action<IValidationRuleOptionsBuilder<T, TExternalResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IPropertyValidatorBuilder<T, TExternalResources, TProperty> EnsureValidatorAsync(
        Func<TExternalResources, ICustomValidator<TProperty>> validatorFactory,
        Action<IValidationRuleOptionsBuilder<T, TExternalResources, TProperty>>? options = null,
        [CallerLineNumber] int callingFileLineNumber = 0);
}
