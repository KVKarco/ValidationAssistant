using Ardalis.GuardClauses;
using KVKarco.ValidationAssistant.Common;
using KVKarco.ValidationAssistant.Common.Helpers.CustomGuards;
using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant;

public abstract class ValidatorBase<TEntity>
{
    public void ValidationFor<TProeprty>(Expression<Func<TEntity, TProeprty>> propertySelector)
    {
        PropertyKey key = Guard.Against.InvalidPropertySelector(propertySelector);
    }
}
