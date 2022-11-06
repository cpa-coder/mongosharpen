using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public Delete<T> Delete<T>(FilterDefinition<T> definition)
        where T : IEntity => new(this, _ => definition);

    public Delete<T> Delete<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new(this, expression);

    public Delete<T, TProjection> Delete<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new(this, expression);

    public Delete<T, TProjection> Delete<T, TProjection>(FilterDefinition<T> definition)
        where T : IEntity => new(this, _ => definition);
}