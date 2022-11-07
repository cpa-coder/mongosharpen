using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public Distinct<T, TProperty> Distinct<T, TProperty>()
        where T : IEntity => new(this);
    
    public Distinct<T, TProperty> Distinct<T, TProperty>(FilterDefinition<T> definition)
        where T : IEntity => new(this, _ => definition);

    public Distinct<T, TProperty> Distinct<T, TProperty>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
        where T : IEntity => new(this, filter);
}