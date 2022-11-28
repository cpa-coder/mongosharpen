using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public IDelete<T> Delete<T>(FilterDefinition<T> definition)
        where T : IEntity => new Delete<T>(this, _ => definition);

    public IDelete<T> Delete<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new Delete<T>(this, expression);

    public IDelete<T, TProjection> Delete<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new Delete<T, TProjection>(this, expression);

    public IDelete<T, TProjection> Delete<T, TProjection>(FilterDefinition<T> definition)
        where T : IEntity => new Delete<T, TProjection>(this, _ => definition);
}