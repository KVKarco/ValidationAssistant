using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.Common.Helpers;

internal static class SelectorRewriter
{
    public static Expression<Func<TEntity, TProperty>> Rewrite<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> propertySelector)
    {
        ParameterExpression param = Expression.Parameter(propertySelector.Parameters[0].Type, "x");
        Expression<Func<TEntity, TProperty>> newExpression = new PredicateRewriterVisitor(param).VisitAndConvert(propertySelector, null);

        return newExpression;
    }

    private class PredicateRewriterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _parameterExpression;

        public PredicateRewriterVisitor(ParameterExpression parameterExpression)
        {
            _parameterExpression = parameterExpression;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameterExpression;
        }
    }
}
