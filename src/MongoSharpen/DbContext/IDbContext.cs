using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoSharpen.Builders;

namespace MongoSharpen;

public interface IDbContext
{
    internal IMongoClient Client { get; }
    internal IMongoDatabase Database { get; }
    internal IClientSessionHandle? Session { get; set; }

    Task<bool> ExistAsync();
    Transaction Transaction(ClientSessionOptions? options = null);

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

    SoftDelete<T> SoftDelete<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity, ISoftDelete;

    SoftDelete<T, TProjection> SoftDelete<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity, ISoftDelete;

    Task LogAsync<T>(string id, CancellationToken token = default)
        where T : IEntity;

    Task LogAsync<T>(T entity, CancellationToken token = default)
        where T : IEntity;

    Task LogAsync<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression, CancellationToken token = default)
        where T : IEntity;

    Task LogAsync<T>(IEnumerable<T> entities, CancellationToken token = default)
        where T : IEntity;

    Task DropDataBaseAsync(CancellationToken token = default);
    IMongoCollection<T> Collection<T>() where T : IEntity;
    IMongoQueryable<T> Queryable<T>(ClientSessionOptions? options = null) where T : IEntity;
}