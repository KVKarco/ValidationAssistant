using KVKarco.ValidationAssistant.Abstractions.ValidationContexts;
using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

public interface IResourceGuardBuilder<TResources, out TResource>
{
    IResourceGuardBuilder<TResources, TResource> AgainstNull(
        [CallerLineNumber] int callingFileLineNumber = 0);

    IResourceGuardBuilder<TResources, TResource> Against(
        Predicate<TResource> predicate,
        [CallerLineNumber] int callingFileLineNumber = 0);

    IResourceGuardBuilder<TResources, TResource> AgainstAsync(
        AsyncPredicate<IPreValidationCtx<TResource>> predicate,
        [CallerLineNumber] int callingFileLineNumber = 0);
}
