using KVKarco.ValidationAssistant.Abstractions;

namespace KVKarco.ValidationAssistant.ValidationRules;

// note: this interface is not intended to be used directly by consumers of the library, but rather as a contract for custom validation rules
// users of the library should implement the <see cref="CustomAsyncValidationRule{T, TExternalResources, TProperty}"/> abstract class instead
public interface IAsyncValidationRule<T, TExternalResources, in TProperty>
{
    string GetDefaultFailureMessage(IMessageCtx<T, TExternalResources> context, TProperty value);

    Task<bool> IsValidAsync(IValidationCtx<T, TExternalResources> context, TProperty value, CancellationToken ct);
}
