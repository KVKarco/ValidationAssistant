using KVKarco.ValidationAssistant.Abstractions;

namespace KVKarco.ValidationAssistant.ValidationRules;

// note: this interface is not intended to be used directly by consumers of the library, but rather as a contract for custom validation rules
// users of the library should implement the <see cref="CustomValidationRule{T, TExternalResources, TProperty}"/> abstract class instead
public interface IValidationRule<T, TExternalResources, in TProperty>
{
    string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value);

    bool IsValid(IValidationCtx<T, TExternalResources> context, TProperty value);
}
