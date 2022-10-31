using System.Linq.Expressions;
using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen.Builders;

public sealed class Delete<T> where T : IEntity
{
    internal IDbContext Context { get; set; } = null!;

    private readonly FilterDefinition<T> _filters;

    public Delete(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _filters = filter.Invoke(Builders<T>.Filter);
    }

    public Task<DeleteResult> ExecuteManyAsync(CancellationToken token) =>
        Context.Session == null
            ? Cache<T>.GetCollection(Context).DeleteManyAsync(_filters, token)
            : Cache<T>.GetCollection(Context).DeleteManyAsync(Context.Session, _filters, cancellationToken: token);

    public Task<DeleteResult> ExecuteOneAsync(CancellationToken token) =>
        Context.Session == null
            ? Cache<T>.GetCollection(Context).DeleteOneAsync(_filters, token)
            : Cache<T>.GetCollection(Context).DeleteOneAsync(Context.Session, _filters, cancellationToken: token);

    public Task<T> GetAndExecuteAsync(CancellationToken token) =>
        Context.Session == null
            ? Cache<T>.GetCollection(Context).FindOneAndDeleteAsync(_filters, cancellationToken: token)
            : Cache<T>.GetCollection(Context).FindOneAndDeleteAsync(Context.Session, _filters, cancellationToken: token);
}

public sealed class Delete<T, TProjection> where T : IEntity
{
    internal IDbContext Context { get; set; } = null!;

    private FilterDefinition<T> _filters;
    private readonly FindOneAndDeleteOptions<T, TProjection> _options;

    public Delete(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _filters = filter.Invoke(Builders<T>.Filter);
        _options = new FindOneAndDeleteOptions<T, TProjection>();
    }

    public Delete<T, TProjection> Project(Expression<Func<T, TProjection>> expression)
    {
        if (_options.Projection != null) throw new InvalidOperationException("Projection already set");

        ProjectionDefinition<T, TProjection> Projection(ProjectionDefinitionBuilder<T> p)
        {
            return p.Expression(expression);
        }
        _options.Projection = Projection(Builders<T>.Projection);

        return this;
    }

    public Delete<T, TProjection> Project(Func<ProjectionDefinitionBuilder<T>, ProjectionDefinition<T, TProjection>> projection)
    {
        if (_options.Projection != null) throw new InvalidOperationException("Projection already set");

        _options.Projection = projection(Builders<T>.Projection);
        return this;
    }

    public Task<TProjection> GetAndExecuteAsync(CancellationToken token)
    {
        if (_options.Projection == null) throw new InvalidOperationException("Projection not set");

        if (Cache<T>.Get().ForSystemGeneration)
            _filters &= Builders<T>.Filter.Eq(x => ((ISystemGenerated) x).SystemGenerated, false);

        return Context.Session == null
            ? Cache<T>.GetCollection(Context).FindOneAndDeleteAsync(_filters, _options, token)
            : Cache<T>.GetCollection(Context).FindOneAndDeleteAsync(Context.Session, _filters, _options, token);
    }
}