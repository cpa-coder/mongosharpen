using System.Linq.Expressions;
using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

public static class SortExtension
{
    public static List<SortDefinition<T>> By<T>(this List<SortDefinition<T>> definitions,
        Func<SortDefinitionBuilder<T>, SortDefinition<T>> sortFunction) where T : IEntity
    {
        definitions.Add(sortFunction(Builders<T>.Sort));
        return definitions;
    }

    public static List<SortDefinition<T>> By<T>(this List<SortDefinition<T>> definitions,
        Expression<Func<T, object>> propertyToSortBy, Order sortOrder = Order.Ascending) where T : IEntity
    {
        if (sortOrder == Order.Ascending)
            definitions.By(s => s.Ascending(propertyToSortBy));
        else
            definitions.By(s => s.Descending(propertyToSortBy));

        return definitions;
    }
}