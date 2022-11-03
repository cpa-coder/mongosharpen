using System.Linq.Expressions;
using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen.Builders;

public sealed class Delete<T> where T : IEntity
{
    internal IDbContext Context { get; set; } = null!;

    private FilterDefinition<T> _filters;

    public Delete(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _filters = filter.Invoke(Builders<T>.Filter);
    }

    public Task<DeleteResult> ExecuteManyAsync(bool forceDelete = false, CancellationToken token = default)
    {
        if (!forceDelete && Cache<T>.Get().ForSystemGeneration)
            _filters &= Builders<T>.Filter.Eq(x => ((ISystemGenerated) x).SystemGenerated, false);

        return Context.Session == null
            ? Cache<T>.GetCollection(Context).DeleteManyAsync(_filters, token)
            : Cache<T>.GetCollection(Context).DeleteManyAsync(Context.Session, _filters, cancellationToken: token);
    }

    public Task<DeleteResult> ExecuteOneAsync(bool forceDelete = false, CancellationToken token = default)
    {
        if (!forceDelete && Cache<T>.Get().ForSystemGeneration)
            _filters &= Builders<T>.Filter.Eq(x => ((ISystemGenerated) x).SystemGenerated, false);

        return Context.Session == null
            ? Cache<T>.GetCollection(Context).DeleteOneAsync(_filters, token)
            : Cache<T>.GetCollection(Context).DeleteOneAsync(Context.Session, _filters, cancellationToken: token);
    }

    public async Task<T> GetAndExecuteAsync(bool forceDelete = false, CancellationToken token = default)
    {
        if (!forceDelete && Cache<T>.Get().ForSystemGeneration)
            _filters &= Builders<T>.Filter.Eq(x => ((ISystemGenerated) x).SystemGenerated, false);

        T result;

        if (Context.Session == null)
            result = await Cache<T>.GetCollection(Context).FindOneAndDeleteAsync(_filters, null, token);
        else
            result = await Cache<T>.GetCollection(Context).FindOneAndDeleteAsync(Context.Session, _filters, null, token);

        if (result == null)
            throw new InvalidOperationException("No item deleted");

        return result;
    }
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

    public async Task<TProjection> GetAndExecuteAsync(bool forceDelete = false, CancellationToken token = default)
    {
        if (_options.Projection == null) throw new InvalidOperationException("Projection not set");

        if (!forceDelete && Cache<T>.Get().ForSystemGeneration)
            _filters &= Builders<T>.Filter.Eq(x => ((ISystemGenerated) x).SystemGenerated, false);

        TProjection result;

        if (Context.Session == null)
            result = await Cache<T>.GetCollection(Context).FindOneAndDeleteAsync(_filters, _options, token);
        else
            result = await Cache<T>.GetCollection(Context).FindOneAndDeleteAsync(Context.Session, _filters, _options, token);

        if (result == null)
            throw new InvalidOperationException("No item deleted");

        return result;
    }
}