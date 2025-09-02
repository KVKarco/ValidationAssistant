using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.Abstractions;

public interface ISchemaGuardBuilder<T>
{
    //create part of the Schema Guard for given path 
    //can throw ValidationCompositionException if the selector is considered invalid.
    ISchemaGuardBuilder<T> Contains<TTarget>(Expression<Func<T, TTarget>> selector);
}
