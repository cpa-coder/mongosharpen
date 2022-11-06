using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public SoftDelete<T> SoftDelete<T>(FilterDefinition<T> definition)
        where T : IEntity, ISoftDelete => new(this, _ => definition);

    public SoftDelete<T> SoftDelete<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity, ISoftDelete => new(this, expression);

    public SoftDelete<T, TProjection> SoftDelete<T, TProjection>(FilterDefinition<T> definition)
        where T : IEntity, ISoftDelete => new(this, _ => definition);

    public SoftDelete<T, TProjection> SoftDelete<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity, ISoftDelete => new(this, expression);
}