using System.Linq.Expressions;
using MongoDB.Driver;

namespace MongoSharpen;

public static class FilterExtension
{
    public static FilterDefinition<T> MatchId<T>(this FilterDefinition<T> filter, string id) where T : IEntity
    {
        filter &= Builders<T>.Filter.Eq(x => x.Id, id);
        return filter;
    }

    public static FilterDefinition<T> MatchId<T>(this FilterDefinitionBuilder<T> builder, string id)
        where T : IEntity => builder.Empty.MatchId(id);

    public static FilterDefinition<T> Match<T>(this FilterDefinition<T> filter, FilterDefinition<T> expression) where T : IEntity
    {
        filter &= expression;
        return filter;
    }

    public static FilterDefinition<T> Match<T>(this FilterDefinitionBuilder<T> builder, FilterDefinition<T> expression)
        where T : IEntity => builder.Empty.Match(expression);

    public static FilterDefinition<T> Match<T>(this FilterDefinition<T> filter,
        Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression) where T : IEntity
    {
        filter &= expression(Builders<T>.Filter);
        return filter;
    }

    public static FilterDefinition<T> Match<T>(this FilterDefinitionBuilder<T> builder,
        Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => builder.Empty.Match(expression);

    public static FilterDefinition<T> Match<T>(this FilterDefinition<T> filter, Expression<Func<T, bool>> expression)
        where T : IEntity
    {
        FilterDefinition<T> Filter(FilterDefinitionBuilder<T> f)
        {
            return f.Where(expression);
        }

        filter &= Filter(Builders<T>.Filter);
        return filter;
    }

    public static FilterDefinition<T> Match<T>(this FilterDefinitionBuilder<T> builder, Expression<Func<T, bool>> expression)
        where T : IEntity => builder.Empty.Match(expression);

    public static FilterDefinition<T> Match<T>(this FilterDefinition<T> filter, string expression) where T : IEntity
    {
        filter &= expression;
        return filter;
    }

    public static FilterDefinition<T> Match<T>(this FilterDefinitionBuilder<T> builder, string expression)
        where T : IEntity => builder.Empty.Match(expression);
}