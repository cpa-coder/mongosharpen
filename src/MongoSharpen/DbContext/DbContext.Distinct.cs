using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public IDistinct<T, TProperty> Distinct<T, TProperty>()
        where T : IEntity => new Distinct<T, TProperty>(this);
    
    public IDistinct<T, TProperty> Distinct<T, TProperty>(FilterDefinition<T> definition)
        where T : IEntity => new Distinct<T, TProperty>(this, _ => definition);

    public IDistinct<T, TProperty> Distinct<T, TProperty>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
        where T : IEntity => new Distinct<T, TProperty>(this, filter);
}