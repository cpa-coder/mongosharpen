using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public Find<T> Find<T>() where T : IEntity => new(this);

    public Find<T> Find<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new(this, expression);

    public Find<T> Find<T>(FilterDefinition<T> definition)
        where T : IEntity => new(this, _ => definition);

    public Find<T, TProjection> Find<T, TProjection>() where T : IEntity => new(this);

    public Find<T, TProjection> Find<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new(this, expression);
    
    public Find<T, TProjection> Find<T, TProjection>(FilterDefinition<T> definition)
        where T : IEntity => new(this, _ => definition);
}