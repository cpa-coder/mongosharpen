using System.Linq.Expressions;

namespace MongoSharpen;

public static class ExpressionExtensions
{
    /// <summary>
    /// Merge two expression with AND operator
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>The combined expression</returns>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
    {
        var p = a.Parameters[0];

        var visitor = new SubstExpressionVisitor
        {
            Subs = { [b.Parameters[0]] = p }
        };

        Expression body = Expression.AndAlso(a.Body, visitor.Visit(b.Body)!);
        return Expression.Lambda<Func<T, bool>>(body, p);
    }

    
    /// <summary>
    /// Merge two expression with OR operator
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>The combined expression</returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
    {
        var p = a.Parameters[0];

        var visitor = new SubstExpressionVisitor
        {
            Subs = { [b.Parameters[0]] = p }
        };

        Expression body = Expression.OrElse(a.Body, visitor.Visit(b.Body)!);
        return Expression.Lambda<Func<T, bool>>(body, p);
    }

    private class SubstExpressionVisitor : ExpressionVisitor
    {
        public readonly Dictionary<Expression, Expression> Subs = new();

        protected override Expression VisitParameter(ParameterExpression node) => Subs.TryGetValue(node, out var newValue) ? newValue : node;
    }
}