using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public IUpdate<T> Update<T>(FilterDefinition<T> definition)
        where T : IEntity => new Update<T>(this, _ => definition);

    public IUpdate<T> Update<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new Update<T>(this, expression);

    public IUpdate<T, TProjection> Update<T, TProjection>(FilterDefinition<T> definition)
        where T : IEntity => new Update<T, TProjection>(this, _ => definition);

    public IUpdate<T, TProjection> Update<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new Update<T, TProjection>(this, expression);
}