using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public SoftDelete<T> SoftDelete<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity, ISoftDelete => new(expression) { Context = this };

    public SoftDelete<T, TProjection> SoftDelete<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity, ISoftDelete => new(expression) { Context = this };
}