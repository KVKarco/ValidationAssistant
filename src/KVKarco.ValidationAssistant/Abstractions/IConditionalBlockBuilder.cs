using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

public interface IConditionalBlockBuilder<TSubject, TResources>
{
    void Otherwise(
        Action<IValidatorValidationDefinitionBuilder<TSubject, TResources>> componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);
}
