using System.Linq.Expressions;
using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen.Builders;

public interface IUpdate<T> where T : IEntity
{
    IUpdate<T> Modify(Action<List<UpdateDefinition<T>>> updateAction);
    IUpdate<T> Modify(IEnumerable<UpdateDefinition<T>> updateDefinitions);
    Task<UpdateResult> ExecuteAsync(CancellationToken token = default);
    Task<T> ExecuteAndGetAsync(CancellationToken token = default);
}

internal sealed class Update<T> : IUpdate<T> where T : IEntity
{
    private readonly IDbContext _context;
    private FilterDefinition<T> _filters;
    private readonly List<UpdateDefinition<T>> _updates = new();
    private readonly FindOneAndUpdateOptions<T, T> _options;

    public Update(IDbContext context, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _context = context;
        _filters = filter.Invoke(Builders<T>.Filter);
        _options = new FindOneAndUpdateOptions<T, T> { ReturnDocument = ReturnDocument.After };
    }

    public IUpdate<T> Modify(Action<List<UpdateDefinition<T>>> updateAction)
    {
        updateAction.Invoke(_updates);
        return this;
    }

    public IUpdate<T> Modify(IEnumerable<UpdateDefinition<T>> updateDefinitions)
    {
        _updates.AddRange(updateDefinitions);
        return this;
    }

    public async Task<UpdateResult> ExecuteAsync(CancellationToken token = default)
    {
        if (Cache<T>.Get().HasModifiedOn) _updates.Set(b => b.CurrentDate(Cache<T>.Get().ModifiedOnPropName));

        var session = _context.Session;
        var collection = Cache<T>.GetCollection(_context);
        var definition = Builders<T>.Update.Combine(_updates);

        _filters = _context.MergeWithGlobalFilter(_filters);

        var result = session == null
            ? await collection.UpdateManyAsync(_filters, definition, cancellationToken: token)
            : await collection.UpdateManyAsync(session, _filters, definition, cancellationToken: token);

        return new UpdateResult
        {
            MatchedCount = result.MatchedCount,
            ModifiedCount = result.ModifiedCount,
            Acknowledge = result.IsAcknowledged
        };
    }

    public Task<T> ExecuteAndGetAsync(CancellationToken token = default)
    {
        if (Cache<T>.Get().HasModifiedOn) _updates.Set(b => b.CurrentDate(Cache<T>.Get().ModifiedOnPropName));

        var session = _context.Session;
        var collection = Cache<T>.GetCollection(_context);
        var update = Builders<T>.Update.Combine(_updates);

        _filters = _context.MergeWithGlobalFilter(_filters);

        return session == null
            ? collection.FindOneAndUpdateAsync(_filters, update, _options, token)
            : collection.FindOneAndUpdateAsync(session, _filters, update, _options, token);
    }
}

public interface IUpdate<T, TProjection> where T : IEntity
{
    IUpdate<T, TProjection> Modify(Action<List<UpdateDefinition<T>>> updateAction);
    IUpdate<T, TProjection> Project(Expression<Func<T, TProjection>> expression);
    Task<TProjection> ExecuteAndGetAsync(CancellationToken token = default);
}

internal sealed class Update<T, TProjection> : IUpdate<T, TProjection> where T : IEntity
{
    private readonly IDbContext _context;
    private FilterDefinition<T> _filters;
    private readonly List<UpdateDefinition<T>> _updates = new();
    private readonly FindOneAndUpdateOptions<T, TProjection> _options;

    public Update(IDbContext context, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _context = context;
        _filters = filter.Invoke(Builders<T>.Filter);
        _options = new FindOneAndUpdateOptions<T, TProjection> { ReturnDocument = ReturnDocument.After };
    }

    public IUpdate<T, TProjection> Modify(Action<List<UpdateDefinition<T>>> updateAction)
    {
        updateAction.Invoke(_updates);
        return this;
    }

    public IUpdate<T, TProjection> Project(Expression<Func<T, TProjection>> expression)
    {
        if (_options.Projection != null) throw new InvalidOperationException("Projection already set");

        ProjectionDefinition<T, TProjection> Projection(ProjectionDefinitionBuilder<T> p)
        {
            return p.Expression(expression);
        }
        _options.Projection = Projection(Builders<T>.Projection);

        return this;
    }

    public Task<TProjection> ExecuteAndGetAsync(CancellationToken token = default)
    {
        if (_options.Projection == null) throw new InvalidOperationException("Projection not set");

        if (Cache<T>.Get().HasModifiedOn) _updates.Set(b => b.CurrentDate(Cache<T>.Get().ModifiedOnPropName));

        var session = _context.Session;
        var collection = Cache<T>.GetCollection(_context);
        var update = Builders<T>.Update.Combine(_updates);

        _filters = _context.MergeWithGlobalFilter(_filters);

        return session == null
            ? collection.FindOneAndUpdateAsync(_filters, update, _options, token)
            : collection.FindOneAndUpdateAsync(session, _filters, update, _options, token);
    }
}