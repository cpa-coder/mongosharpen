using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

public sealed partial class DbContext
{
    public Update<T> Update<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new(expression) { Context = this };

    public Update<T, TProjection> Update<T, TProjection>(
        Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new(expression) { Context = this };
}