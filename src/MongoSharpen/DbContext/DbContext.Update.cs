using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public Update<T> Update<T>(FilterDefinition<T> definition)
        where T : IEntity => new(this, _ => definition);

    public Update<T> Update<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new(this, expression);

    public Update<T, TProjection> Update<T, TProjection>(FilterDefinition<T> definition)
        where T : IEntity => new(this, _ => definition);

    public Update<T, TProjection> Update<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new(this, expression);
}