using System.Linq.Expressions;
using MongoDB.Driver;

namespace MongoSharpen;

public static class UpdateExtension
{
    public static List<UpdateDefinition<T>> Set<T>(this List<UpdateDefinition<T>> updates, UpdateDefinition<T> operation)
        where T : IEntity
    {
        updates.Add(operation);
        return updates;
    }

    public static List<UpdateDefinition<T>> Set<T, TProp>(this List<UpdateDefinition<T>> updates,
        Expression<Func<T, TProp>> property, TProp value) where T : IEntity
    {
        updates.Add(Builders<T>.Update.Set(property, value));
        return updates;
    }

    internal static List<UpdateDefinition<T>> Set<T>(this List<UpdateDefinition<T>> updates,
        Func<UpdateDefinitionBuilder<T>, UpdateDefinition<T>> operation) where T : IEntity
    {
        updates.Add(operation(Builders<T>.Update));
        return updates;
    }
}