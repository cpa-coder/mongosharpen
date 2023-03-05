using System.Linq.Expressions;
using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen.Builders;

public interface IDelete<T> where T : IEntity
{
    Task<DeleteResult> ExecuteManyAsync(bool forceDelete = false, CancellationToken token = default);
    Task<DeleteResult> ExecuteOneAsync(bool forceDelete = false, CancellationToken token = default);
    Task<T?> GetAndExecuteAsync(bool forceDelete = false, CancellationToken token = default);
}

internal sealed class Delete<T> : IDelete<T> where T : IEntity
{
    private readonly IDbContext _context;
    private FilterDefinition<T> _filters;

    public Delete(IDbContext context, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _context = context;
        _filters = filter.Invoke(Builders<T>.Filter);
    }

    public async Task<DeleteResult> ExecuteManyAsync(bool forceDelete = false, CancellationToken token = default)
    {
        if (!forceDelete && Cache<T>.Get().ForSystemGeneration)
            _filters &= Builders<T>.Filter.Eq(x => ((ISystemGenerated) x).SystemGenerated, false);

        _filters = _context.MergeWithGlobalFilter(_filters);

        var result = _context.Session == null
            ? await Cache<T>.GetCollection(_context).DeleteManyAsync(_filters, token)
            : await Cache<T>.GetCollection(_context).DeleteManyAsync(_context.Session, _filters, cancellationToken: token);

        return new DeleteResult
        {
            DeletedCount = result.DeletedCount,
            Acknowledge = result.IsAcknowledged
        };
    }

    public async Task<DeleteResult> ExecuteOneAsync(bool forceDelete = false, CancellationToken token = default)
    {
        if (!forceDelete && Cache<T>.Get().ForSystemGeneration)
            _filters &= Builders<T>.Filter.Eq(x => ((ISystemGenerated) x).SystemGenerated, false);

        _filters = _context.MergeWithGlobalFilter(_filters);

        var result = _context.Session == null
            ? await Cache<T>.GetCollection(_context).DeleteOneAsync(_filters, token)
            : await Cache<T>.GetCollection(_context).DeleteOneAsync(_context.Session, _filters, cancellationToken: token);

        return new DeleteResult
        {
            DeletedCount = result.DeletedCount,
            Acknowledge = result.IsAcknowledged
        };
    }

    public async Task<T?> GetAndExecuteAsync(bool forceDelete = false, CancellationToken token = default)
    {
        if (!forceDelete && Cache<T>.Get().ForSystemGeneration)
            _filters &= Builders<T>.Filter.Eq(x => ((ISystemGenerated) x).SystemGenerated, false);

        _filters = _context.MergeWithGlobalFilter(_filters);

        T result;

        if (_context.Session == null)
            result = await Cache<T>.GetCollection(_context).FindOneAndDeleteAsync(_filters, null, token);
        else
            result = await Cache<T>.GetCollection(_context).FindOneAndDeleteAsync(_context.Session, _filters, null, token);

        if (result == null)
            throw new InvalidOperationException("No item deleted");

        return result;
    }
}

public interface IDelete<T, TProjection> where T : IEntity
{
    IDelete<T, TProjection> Project(Expression<Func<T, TProjection>> expression);
    Task<TProjection?> GetAndExecuteAsync(bool forceDelete = false, CancellationToken token = default);
}

internal sealed class Delete<T, TProjection> : IDelete<T, TProjection> where T : IEntity
{
    private readonly IDbContext _context;
    private FilterDefinition<T> _filters;
    private readonly FindOneAndDeleteOptions<T, TProjection> _options;

    public Delete(IDbContext context, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _context = context;
        _filters = filter.Invoke(Builders<T>.Filter);
        _options = new FindOneAndDeleteOptions<T, TProjection>();
    }

    public IDelete<T, TProjection> Project(Expression<Func<T, TProjection>> expression)
    {
        if (_options.Projection != null) throw new InvalidOperationException("Projection already set");

        ProjectionDefinition<T, TProjection> Projection(ProjectionDefinitionBuilder<T> p)
        {
            return p.Expression(expression);
        }
        _options.Projection = Projection(Builders<T>.Projection);

        return this;
    }

    public async Task<TProjection?> GetAndExecuteAsync(bool forceDelete = false, CancellationToken token = default)
    {
        if (_options.Projection == null) throw new InvalidOperationException("Projection not set");

        if (!forceDelete && Cache<T>.Get().ForSystemGeneration)
            _filters &= Builders<T>.Filter.Eq(x => ((ISystemGenerated) x).SystemGenerated, false);

        _filters = _context.MergeWithGlobalFilter(_filters);

        TProjection result;

        if (_context.Session == null)
            result = await Cache<T>.GetCollection(_context).FindOneAndDeleteAsync(_filters, _options, token);
        else
            result = await Cache<T>.GetCollection(_context).FindOneAndDeleteAsync(_context.Session, _filters, _options, token);

        if (result == null)
            throw new InvalidOperationException("No item deleted");

        return result;
    }
}