using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

public sealed partial class DbContext
{
    public Find<T> Find<T>() where T : IEntity => new() { Context = this };

    public Find<T> Find<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new() { Context = this };

    public Find<T, TProjection> Find<T, TProjection>() where T : IEntity => new() { Context = this };

    public Find<T, TProjection> Find<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new(expression) { Context = this };
}