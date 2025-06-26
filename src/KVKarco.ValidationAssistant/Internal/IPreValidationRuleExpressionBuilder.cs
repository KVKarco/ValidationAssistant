using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Internal;

// all method can throw RuleCreationException if the predicates are null
public interface IPreValidationRuleExpressionBuilder<T, TExternalResources>
{
    void Ensure(
        PreValidationPredicate<T> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    void EnsureAsync(
        AsyncPreValidationPredicate<T> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    void EnsureResources(
        PreValidationPredicate<TExternalResources> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);

    void EnsureResourcesAsync(
        AsyncPreValidationPredicate<TExternalResources> predicate,
        string? explanationMessage = null,
        [CallerLineNumber] int callingFileLineNumber = 0);
}
