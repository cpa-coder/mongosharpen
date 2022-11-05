using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoSharpen.Internal;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public IMongoQueryable<T> Queryable<T>(AggregateOptions? options = null) where T : IEntity
    {
        var globalFilter = MergeFilterInternal(Builders<T>.Filter.Empty);

        _session = _client.StartSession();
        var queryable = Cache<T>.GetCollection(this).AsQueryable(_session, options);

        return globalFilter != Builders<T>.Filter.Empty
            ? queryable.Where(_ => globalFilter.Inject())
            : queryable;
    }
}