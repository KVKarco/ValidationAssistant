using System.Runtime.CompilerServices;

namespace KVKarco.ValidationAssistant.Abstractions;

public interface IConditionalBlockBuilder<T, TExternalResources>
{
    void OtherwiseUse(
        Action componentsToUse,
        [CallerLineNumber] int callingFileLineNumber = 0);
}
