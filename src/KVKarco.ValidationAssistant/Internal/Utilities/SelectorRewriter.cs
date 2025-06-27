using System.Linq.Expressions;


namespace KVKarco.ValidationAssistant.Internal.Utilities;

/// <summary>
/// Provides a static extension method for rewriting <see cref="Expression{TDelegate}"/> instances,
/// specifically designed to rebind the parameter expression of a lambda. This is useful
/// for scenarios where expressions need to be composed or reused with a consistent
/// parameter instance, ensuring proper referencing within the new expression tree.
/// </summary>
internal static class SelectorRewriter
{
    /// <summary>
    /// Rewrites the given <paramref name="propertySelector"/> expression by replacing its
    /// original parameter with a new <see cref="ParameterExpression"/> that has the same type
    /// but a standardized name ("x"). This is typically used to normalize lambda expressions
    /// before further processing or combining them.
    /// </summary>
    /// <typeparam name="T">The type of the input instance for the expression (the root object).</typeparam>
    /// <typeparam name="TProperty">The type of the property or return value of the expression.</typeparam>
    /// <param name="propertySelector">The original lambda expression whose parameter is to be rewritten.</param>
    /// <returns>
    /// A new <see cref="Expression{TDelegate}"/> where the original parameter of the
    /// <paramref name="propertySelector"/> has been replaced by a new <see cref="ParameterExpression"/>
    /// named "x" of the same type. The body of the expression is adjusted accordingly.
    /// </returns>
    /// <example>
    /// Original: `(user => user.Name)`
    /// Rewritten: `(x => x.Name)`
    /// </example>
    public static Expression<Func<T, TProperty>> Rewrite<T, TProperty>(this Expression<Func<T, TProperty>> propertySelector)
    {
        // Create a new ParameterExpression with a standardized name ("x") but the same type
        // as the original parameter of the input propertySelector.
        ParameterExpression newParameterExpression = Expression.Parameter(propertySelector.Parameters[0].Type, "x");

        // Use the custom ExpressionVisitor to traverse the expression tree and replace
        // all occurrences of the original parameter with the new, standardized one.
        Expression<Func<T, TProperty>> newExpression =
            new PredicateRewriterVisitor(newParameterExpression).VisitAndConvert(propertySelector, null);

        return newExpression;
    }

    /// <summary>
    /// An internal, sealed <see cref="ExpressionVisitor"/> implementation specifically designed
    /// to replace all occurrences of an <see cref="Expression"/>'s original parameter(s)
    /// with a predefined <see cref="ParameterExpression"/>.
    /// </summary>
    private sealed class PredicateRewriterVisitor : ExpressionVisitor
    {
        /// <summary>
        /// The <see cref="ParameterExpression"/> that will replace all visited parameter nodes.
        /// </summary>
        private readonly ParameterExpression _parameterExpression;

        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateRewriterVisitor"/> class
        /// with the parameter expression that should be used for replacement.
        /// </summary>
        /// <param name="parameterExpression">The new <see cref="ParameterExpression"/> to substitute for original parameters.</param>
        public PredicateRewriterVisitor(ParameterExpression parameterExpression)
        {
            _parameterExpression = parameterExpression;
        }

        /// <summary>
        /// Visits a <see cref="ParameterExpression"/> node.
        /// This overridden method returns the predefined <see cref="_parameterExpression"/>,
        /// effectively replacing the original parameter in the expression tree.
        /// </summary>
        /// <param name="node">The <see cref="ParameterExpression"/> to visit.</param>
        /// <returns>The replacement <see cref="ParameterExpression"/>.</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameterExpression;
        }
    }
}
