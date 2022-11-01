using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoSharpen.Builders;

namespace MongoSharpen;

public interface IDbContext : IDisposable
{
    internal IMongoDatabase Database { get; }
    internal IClientSessionHandle? Session { get; }
    void StartTransaction(ClientSessionOptions? options = null);
    Task CommitAsync(CancellationToken cancellation = default);

    Task SaveAsync<T>(T entity, CancellationToken cancellation = default)
        where T : IEntity;

    Task SaveAsync<T>(IEnumerable<T> entities, CancellationToken cancellation = default)
        where T : IEntity;

    Find<T> Find<T>() where T : IEntity;

    Find<T> Find<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity;
    
    Find<T, TProjection> Find<T, TProjection>()
        where T : IEntity;

    Find<T, TProjection> Find<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity;

    Update<T> Update<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity;

    Update<T, TProjection> Update<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity;

    Delete<T> Delete<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity;

    Delete<T, TProjection> Delete<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity;

    Task DropDataBaseAsync(CancellationToken token = default);
    IMongoQueryable<T> Queryable<T>() where T : IEntity;
    IMongoCollection<T> Collection<T>() where T : IEntity;
}