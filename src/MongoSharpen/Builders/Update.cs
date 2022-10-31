using System.Linq.Expressions;
using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen.Builders;

public sealed class Update<T> where T : IEntity
{
    internal IDbContext Context { get; set; } = null!;

    private readonly FilterDefinition<T> _filters;
    private readonly List<UpdateDefinition<T>> _updates = new();
    private readonly FindOneAndUpdateOptions<T, T> _options;

    public Update(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
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

        var session = Context.Session;
        var collection = Cache<T>.GetCollection(Context);
        var definition = Builders<T>.Update.Combine(_updates);

        return session == null
            ? collection.UpdateManyAsync(_filters, definition, cancellationToken: token)
            : collection.UpdateManyAsync(session, _filters, definition, cancellationToken: token);
    }

    public Task<T> ExecuteAndGetAsync(CancellationToken token = default)
    {
        if (Cache<T>.Get().HasModifiedOn) _updates.Set(b => b.CurrentDate(Cache<T>.Get().ModifiedOnPropName));

        var session = Context.Session;
        var collection = Cache<T>.GetCollection(Context);
        var update = Builders<T>.Update.Combine(_updates);

        return session == null
            ? collection.FindOneAndUpdateAsync(_filters, update, _options, token)
            : collection.FindOneAndUpdateAsync(session, _filters, update, _options, token);
    }
}

public sealed class Update<T, TProjection> where T : IEntity
{
    internal IDbContext Context { get; set; } = null!;

    private readonly FilterDefinition<T> _filters;
    private readonly List<UpdateDefinition<T>> _updates = new();
    private readonly FindOneAndUpdateOptions<T, TProjection> _options;

    public Update(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
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

    public Update<T, TProjection> Project(
        Func<ProjectionDefinitionBuilder<T>, ProjectionDefinition<T, TProjection>> projection)
    {
        if (_options.Projection != null) throw new InvalidOperationException("Projection already set");
        _options.Projection = projection(Builders<T>.Projection);

        return this;
    }

    public Task<TProjection> ExecuteAndGetAsync(CancellationToken token = default)
    {
        if (_options.Projection == null) throw new InvalidOperationException("Make sure to set a projection before executing");

        if (Cache<T>.Get().HasModifiedOn) _updates.Set(b => b.CurrentDate(Cache<T>.Get().ModifiedOnPropName));

        var session = Context.Session;
        var collection = Cache<T>.GetCollection(Context);
        var update = Builders<T>.Update.Combine(_updates);

        return session == null
            ? collection.FindOneAndUpdateAsync(_filters, update, _options, token)
            : collection.FindOneAndUpdateAsync(session, _filters, update, _options, token);
    }
}