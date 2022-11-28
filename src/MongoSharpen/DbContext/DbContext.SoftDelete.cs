using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public ISoftDelete<T> SoftDelete<T>(FilterDefinition<T> definition)
        where T : IEntity, IDeleteOn => new SoftDelete<T>(this, _ => definition);

    public ISoftDelete<T> SoftDelete<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity, IDeleteOn => new SoftDelete<T>(this, expression);

    public ISoftDelete<T, TProjection> SoftDelete<T, TProjection>(FilterDefinition<T> definition)
        where T : IEntity, IDeleteOn => new SoftDelete<T, TProjection>(this, _ => definition);

    public ISoftDelete<T, TProjection> SoftDelete<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity, IDeleteOn => new SoftDelete<T, TProjection>(this, expression);
}