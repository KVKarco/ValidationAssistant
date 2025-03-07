using KVKarco.ValidationAssistant.Common.Helpers;
using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.Common;

internal abstract class ValueEvaluator
{
    public PropertyKey PropertyKey { get; }

    protected ValueEvaluator(PropertyKey propertyKey)
    {
        PropertyKey = propertyKey;
    }

    public static ValueEvaluator<TEntity, TProperty> Create<TEntity, TProperty>(
        PropertyKey propertyKey,
        Expression<Func<TEntity, TProperty>> propertySelector)
    {
        return new ValueEvaluator<TEntity, TProperty>(
            propertyKey,
            ExpressionFactory.CreateValueNaNEvaluator(propertySelector),
            propertySelector.Compile());
    }
}

internal sealed class ValueEvaluator<TEntity, TProperty> : ValueEvaluator
{
    private readonly Predicate<TEntity> _valueNaNEvaluator;
    private readonly Func<TEntity, TProperty> _valueExtractor;

    public ValueEvaluator(PropertyKey propertyKey, Predicate<TEntity> valueNaNEvaluator, Func<TEntity, TProperty> valueExtractor)
        : base(propertyKey)
    {
        _valueNaNEvaluator = valueNaNEvaluator;
        _valueExtractor = valueExtractor;
    }

    public bool HasValue(TEntity entity) => _valueNaNEvaluator(entity);

    public TProperty GetValue(TEntity entity) => _valueExtractor(entity);
}
