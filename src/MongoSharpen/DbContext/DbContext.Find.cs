using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public IFind<T> Find<T>() where T : IEntity => new Find<T>(this);

    public IFind<T> Find<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new Find<T>(this, expression);

    public IFind<T> Find<T>(FilterDefinition<T> definition)
        where T : IEntity => new Find<T>(this, _ => definition);

    public IFind<T, TProjection> Find<T, TProjection>() where T : IEntity => new Find<T, TProjection>(this);

    public IFind<T, TProjection> Find<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new Find<T, TProjection>(this, expression);

    public IFind<T, TProjection> Find<T, TProjection>(FilterDefinition<T> definition)
        where T : IEntity => new Find<T, TProjection>(this, _ => definition);
}