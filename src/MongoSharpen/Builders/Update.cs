using System.Linq.Expressions;
using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen.Builders;

public sealed class Update<T> where T : IEntity
{
    private readonly IDbContext _context;
    private readonly FilterDefinition<T> _filters;
    private readonly List<UpdateDefinition<T>> _updates = new();
    private readonly FindOneAndUpdateOptions<T, T> _options;

    public Update(IDbContext context, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _context = context;
        _filters = filter.Invoke(Builders<T>.Filter);
        _options = new FindOneAndUpdateOptions<T, T> { ReturnDocument = ReturnDocument.After };
    }

    public Update<T> Modify(Action<List<UpdateDefinition<T>>> updateAction)
    {
        updateAction.Invoke(_updates);
        return this;
    }

    public Task<UpdateResult> ExecuteAsync(CancellationToken token = default)
    {
        if (Cache<T>.Get().HasModifiedOn) _updates.Set(b => b.CurrentDate(Cache<T>.Get().ModifiedOnPropName));

        var session = _context.Session;
        var collection = Cache<T>.GetCollection(_context);
        var definition = Builders<T>.Update.Combine(_updates);

        return session == null
            ? collection.UpdateManyAsync(_filters, definition, cancellationToken: token)
            : collection.UpdateManyAsync(session, _filters, definition, cancellationToken: token);
    }

    public Task<T> ExecuteAndGetAsync(CancellationToken token = default)
    {
        if (Cache<T>.Get().HasModifiedOn) _updates.Set(b => b.CurrentDate(Cache<T>.Get().ModifiedOnPropName));

        var session = _context.Session;
        var collection = Cache<T>.GetCollection(_context);
        var update = Builders<T>.Update.Combine(_updates);

        return session == null
            ? collection.FindOneAndUpdateAsync(_filters, update, _options, token)
            : collection.FindOneAndUpdateAsync(session, _filters, update, _options, token);
    }
}

public sealed class Update<T, TProjection> where T : IEntity
{
    private readonly IDbContext _context;
    private readonly FilterDefinition<T> _filters;
    private readonly List<UpdateDefinition<T>> _updates = new();
    private readonly FindOneAndUpdateOptions<T, TProjection> _options;

    public Update(IDbContext context, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _context = context;
        _filters = filter.Invoke(Builders<T>.Filter);
        _options = new FindOneAndUpdateOptions<T, TProjection> { ReturnDocument = ReturnDocument.After };
    }

    public Update<T, TProjection> Modify(Action<List<UpdateDefinition<T>>> updateAction)
    {
        updateAction.Invoke(_updates);
        return this;
    }

    public Update<T, TProjection> Project(Expression<Func<T, TProjection>> expression)
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

        return session == null
            ? collection.FindOneAndUpdateAsync(_filters, update, _options, token)
            : collection.FindOneAndUpdateAsync(session, _filters, update, _options, token);
    }
}