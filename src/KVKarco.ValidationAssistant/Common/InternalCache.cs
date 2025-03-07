using Ardalis.GuardClauses;
using KVKarco.ValidationAssistant.Common.Helpers;
using KVKarco.ValidationAssistant.Common.Helpers.CustomGuards;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.Common;

internal static class InternalCache
{
    private static readonly ConcurrentDictionary<string, Lazy<PropertyKey>> _keysKache = [];
    private static readonly ConcurrentDictionary<PropertyKey, Lazy<ValueEvaluator>> _evaluatorsCache = [];

    public static PropertyKey GetOrAddKey<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> propertySelector)
    {
        PropertyKey key = _keysKache.GetOrAdd(
            propertySelector.ToString(),
            x => new Lazy<PropertyKey>(() => ExpressionFactory.ExtractPropertyKey(Guard.Against.EntityOrValueSelection(propertySelector))))
            .Value;

        _evaluatorsCache.TryAdd(key, new Lazy<ValueEvaluator>(() => ValueEvaluator.Create(key, propertySelector)));

        return key;
    }
}
