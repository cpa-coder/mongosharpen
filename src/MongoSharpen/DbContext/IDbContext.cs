using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoSharpen.Builders;

namespace MongoSharpen;

public interface IDbContext
{
    /// <summary>
    ///     Returns whether the database of this context exist in the database server.
    /// </summary>
    /// <returns></returns>
    Task<bool> ExistAsync();

    /// <summary>
    ///     Starts a new transaction.
    /// </summary>
    /// <param name="options">A session options</param>
    /// <TIP>
    ///     1. Make sure that replica set is enabled in the database server. <br />
    ///     2. As much as possible, run only one transaction per request to avoid concurrency issues.
    /// </TIP>
    ITransaction Transaction(ClientSessionOptions? options = null);

    /// <summary>
    ///     Saves the entity to the database.
    /// </summary>
    /// <param name="entity">The entity to save</param>
    /// <param name="token">A cancellation token</param>
    /// <typeparam name="T">The type of entity</typeparam>
    Task SaveAsync<T>(T entity, CancellationToken token = default)
        where T : IEntity;

    /// <summary>
    ///     Saves the entities to the database.
    /// </summary>
    /// <param name="entities">The entity to save</param>
    /// <param name="token">A cancellation token</param>
    /// <typeparam name="T">The type of entity</typeparam>
    Task SaveAsync<T>(IEnumerable<T> entities, CancellationToken token = default)
        where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's find operation.
    /// </summary>
    IFind<T> Find<T>() where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's find operation.
    /// </summary>
    IFind<T> Find<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression) where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's find operation.
    /// </summary>
    public IFind<T> Find<T>(FilterDefinition<T> definition) where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's find operation.
    /// </summary>
    IFind<T, TProjection> Find<T, TProjection>() where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's find operation.
    /// </summary>
    IFind<T, TProjection> Find<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression) where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's find operation.
    /// </summary>
    public IFind<T, TProjection> Find<T, TProjection>(FilterDefinition<T> definition) where T : IEntity;

    /// <summary>
    ///     Returns an estimate of the number of documents in the collection ignoring global filter.
    /// </summary>
    /// <param name="token">The cancellation token</param>
    /// <typeparam name="T">The entity that inherits from <see cref="IEntity" /></typeparam>
    /// <returns>An estimate of the number of documents in the collection</returns>
    Task<long> CountEstimatedAsync<T>(CancellationToken token = default) where T : IEntity;

    /// <summary>
    ///     Counts the number of documents in the collection ignoring global filter.
    /// </summary>
    /// <TIP>
    ///     For faster count, you may want to consider using <see cref="CountEstimatedAsync{T}" />.
    /// </TIP>
    /// <param name="token">The cancellation token</param>
    /// <typeparam name="T">The entity that inherits from <see cref="IEntity" /></typeparam>
    /// <returns>The number of documents in the collection</returns>
    Task<long> CountAsync<T>(CancellationToken token = default) where T : IEntity;

    /// <summary>
    ///     Counts the number of documents in the collection.
    /// </summary>
    /// <param name="expression">Filter expression</param>
    /// <param name="options">Options for count operation</param>
    /// <param name="token">The cancellation token</param>
    /// <typeparam name="T">The entity that inherits from <see cref="IEntity" /></typeparam>
    /// <returns>The number of documents in the collection</returns>
    Task<long> CountAsync<T>(Expression<Func<T, bool>> expression, CountOptions? options = null,
        CancellationToken token = default) where T : IEntity;

    /// <summary>
    ///     Counts the number of documents in the collection.
    /// </summary>
    /// <param name="filter">Filter definition</param>
    /// <param name="options">Options for count operation</param>
    /// <param name="token">The cancellation token</param>
    /// <typeparam name="T">The entity that inherits from <see cref="IEntity" /></typeparam>
    /// <returns>The number of documents in the collection</returns>
    Task<long> CountAsync<T>(FilterDefinition<T> filter, CountOptions? options = null,
        CancellationToken token = default) where T : IEntity;

    /// <summary>
    ///     Counts the number of documents in the collection.
    /// </summary>
    /// <param name="filter">Filter definition function</param>
    /// <param name="options">Options for count operation</param>
    /// <param name="token">The cancellation token</param>
    /// <typeparam name="T">The entity that inherits from <see cref="IEntity" /></typeparam>
    /// <returns>The number of documents in the collection</returns>
    Task<long> CountAsync<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter,
        CountOptions? options = null, CancellationToken token = default) where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's distinct operation.
    /// </summary>
    public IDistinct<T, TProperty> Distinct<T, TProperty>() where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's distinct operation.
    /// </summary>
    public IDistinct<T, TProperty> Distinct<T, TProperty>(FilterDefinition<T> definition) where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's distinct operation.
    /// </summary>
    public IDistinct<T, TProperty> Distinct<T, TProperty>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
        where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's update operation.
    /// </summary>
    /// <warning>
    ///     This is a destructive operation. Use with caution.
    /// </warning>
    /// <TIP>
    ///     Consider logging the old records first.
    /// </TIP>
    IUpdate<T> Update<T>(FilterDefinition<T> definition) where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's update operation.
    /// </summary>
    /// <warning>
    ///     This is a destructive operation. Use with caution.
    /// </warning>
    /// <TIP>
    ///     Consider logging the old records first.
    /// </TIP>
    IUpdate<T> Update<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's update operation.
    /// </summary>
    /// <warning>
    ///     This is a destructive operation. Use with caution.
    /// </warning>
    /// <TIP>
    ///     Consider logging the old records first.
    /// </TIP>
    IUpdate<T, TProjection> Update<T, TProjection>(FilterDefinition<T> definition) where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's update operation.
    /// </summary>
    /// <warning>
    ///     This is a destructive operation. Use with caution.
    /// </warning>
    /// <TIP>
    ///     Consider logging the old records first.
    /// </TIP>
    IUpdate<T, TProjection> Update<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's delete operation.
    /// </summary>
    /// <warning>
    ///     This is a destructive operation. Use with caution.
    /// </warning>
    /// <TIP>
    ///     Consider logging the old records first or using the soft delete operation.
    /// </TIP>
    public IDelete<T> Delete<T>(FilterDefinition<T> definition) where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's delete operation.
    /// </summary>
    /// <warning>
    ///     This is a destructive operation. Use with caution.
    /// </warning>
    /// <TIP>
    ///     Consider logging the old records first or using the soft delete operation.
    /// </TIP>
    IDelete<T> Delete<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's delete operation.
    /// </summary>
    /// ///
    /// <warning>
    ///     This is a destructive operation. Use with caution.
    /// </warning>
    /// <TIP>
    ///     Consider logging the old records first or using the soft delete operation.
    /// </TIP>
    IDelete<T, TProjection> Delete<T, TProjection>(FilterDefinition<T> definition)
        where T : IEntity;

    /// <summary>
    ///     Implements MongoDB's delete operation.
    /// </summary>
    /// ///
    /// <warning>
    ///     This is a destructive operation. Use with caution.
    /// </warning>
    /// <TIP>
    ///     Consider logging the old records first or using the soft delete operation.
    /// </TIP>
    IDelete<T, TProjection> Delete<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity;

    /// <summary>
    ///     Implements soft delete operation.
    /// </summary>
    ISoftDelete<T> SoftDelete<T>(FilterDefinition<T> definition)
        where T : IEntity, IDeleteOn;

    /// <summary>
    ///     Implements soft delete operation.
    /// </summary>
    ISoftDelete<T> SoftDelete<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity, IDeleteOn;

    /// <summary>
    ///     Implements soft delete operation.
    /// </summary>
    ISoftDelete<T, TProjection> SoftDelete<T, TProjection>(FilterDefinition<T> definition)
        where T : IEntity, IDeleteOn;

    /// <summary>
    ///     Implements soft delete operation.
    /// </summary>
    ISoftDelete<T, TProjection> SoftDelete<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity, IDeleteOn;

    /// <summary>
    ///     Implements document logging operation.
    /// </summary>
    Task LogAsync<T>(string id, CancellationToken token = default)
        where T : IEntity;

    /// <summary>
    ///     Implements document logging operation.
    /// </summary>
    Task LogAsync<T>(T entity, CancellationToken token = default)
        where T : IEntity;

    /// <summary>
    ///     Implements document logging operation.
    /// </summary>
    Task LogAsync<T>(FilterDefinition<T> definition, CancellationToken token = default) where T : IEntity;

    /// <summary>
    ///     Implements document logging operation.
    /// </summary>
    Task LogAsync<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression, CancellationToken token = default)
        where T : IEntity;

    /// <summary>
    ///     Implements document logging operation.
    /// </summary>
    Task LogAsync<T>(IEnumerable<T> entities, CancellationToken token = default)
        where T : IEntity;

    /// <summary>
    ///     Deletes the database for this context.
    /// </summary>
    /// <warning>
    ///     This is a destructive operation. DON'T USE this unless required.
    /// </warning>
    Task DropDataBaseAsync(CancellationToken token = default);

    /// <summary>
    ///     Exposes the MongoDB's collection.
    /// </summary>
    IMongoCollection<T> Collection<T>() where T : IEntity;

    /// <summary>
    ///     Exposes the MongoDB's collection with a <see cref="BsonDocument" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IMongoCollection<BsonDocument> CollectionLog<T>() where T : IEntity;

    /// <summary>
    ///     Exposes the MongoDB's <see cref="IMongoQueryable" />.
    /// </summary>
    /// <remarks>
    ///     Global filter is applied unless ignored.
    /// </remarks>
    IMongoQueryable<T> Queryable<T>(AggregateOptions? options = null) where T : IEntity;
}