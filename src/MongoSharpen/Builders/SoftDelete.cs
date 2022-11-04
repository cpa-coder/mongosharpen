using System.Linq.Expressions;
using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen.Builders;

public class SoftDelete<T> where T : IEntity, ISoftDelete
{
    private readonly IDbContext _context;
    private FilterDefinition<T> _filters;
    private readonly List<UpdateDefinition<T>> _updates = new();
    private readonly FindOneAndUpdateOptions<T, T> _options = new() { ReturnDocument = ReturnDocument.After };

    public SoftDelete(IDbContext context, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
    {
        _context = context;
        _filters = expression(Builders<T>.Filter);
    }

    public async Task<DeleteResult> ExecuteManyAsync(string userId, bool forceDelete = false, CancellationToken token = default)
    {
        if (!forceDelete) _filters &= Builders<T>.Filter.Eq(t => t.SystemGenerated, false);
        _filters = _context.MergeWithGlobalFilter(_filters);

        if (Cache<T>.Get().HasModifiedOn) _updates.Set(b => b.CurrentDate(Cache<T>.Get().ModifiedOnPropName));

        _updates.Set(t => t.Deleted, true);
        _updates.Set(t => t.DeletedBy, new DeletedBy { Id = userId });
        _updates.Set(t => t.DeletedOn, DateTime.UtcNow);

        var session = _context.Session;
        var collection = Cache<T>.GetCollection(_context);
        var definition = Builders<T>.Update.Combine(_updates);

        var result = session == null
            ? await collection.UpdateManyAsync(_filters, definition, cancellationToken: token)
            : await collection.UpdateManyAsync(session, _filters, definition, cancellationToken: token);

        return new DeleteResult.Acknowledged(result.ModifiedCount);
    }

    public async Task<DeleteResult> ExecuteOneAsync(string userId, bool forceDelete = false, CancellationToken token = default)
    {
        if (!forceDelete) _filters &= Builders<T>.Filter.Eq(t => t.SystemGenerated, false);
        _filters = _context.MergeWithGlobalFilter(_filters);

        if (Cache<T>.Get().HasModifiedOn) _updates.Set(b => b.CurrentDate(Cache<T>.Get().ModifiedOnPropName));

        _updates.Set(t => t.Deleted, true);
        _updates.Set(t => t.DeletedBy, new DeletedBy { Id = userId });
        _updates.Set(t => t.DeletedOn, DateTime.UtcNow);

        var session = _context.Session;
        var collection = Cache<T>.GetCollection(_context);
        var definition = Builders<T>.Update.Combine(_updates);

        var result = session == null
            ? await collection.UpdateOneAsync(_filters, definition, cancellationToken: token)
            : await collection.UpdateOneAsync(session, _filters, definition, cancellationToken: token);

        return new DeleteResult.Acknowledged(result.ModifiedCount);
    }

    public async Task<T> ExecuteAndGetAsync(string userId, bool forceDelete = false, CancellationToken token = default)
    {
        if (!forceDelete) _filters &= Builders<T>.Filter.Eq(t => t.SystemGenerated, false);
        _filters = _context.MergeWithGlobalFilter(_filters);

        if (Cache<T>.Get().HasModifiedOn) _updates.Set(b => b.CurrentDate(Cache<T>.Get().ModifiedOnPropName));

        _updates.Set(t => t.Deleted, true);
        _updates.Set(t => t.DeletedBy, new DeletedBy { Id = userId });
        _updates.Set(t => t.DeletedOn, DateTime.UtcNow);

        var session = _context.Session;
        var collection = Cache<T>.GetCollection(_context);
        var definition = Builders<T>.Update.Combine(_updates);

        return session == null
            ? await collection.FindOneAndUpdateAsync(_filters, definition, _options, token)
            : await collection.FindOneAndUpdateAsync(session, _filters, definition, _options, token);
    }
}

public class SoftDelete<T, TProjection> where T : IEntity, ISoftDelete
{
    private readonly IDbContext _context;
    private FilterDefinition<T> _filters;
    private readonly List<UpdateDefinition<T>> _updates = new();
    private readonly FindOneAndUpdateOptions<T, TProjection> _options = new() { ReturnDocument = ReturnDocument.After };

    public SoftDelete(IDbContext context, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
    {
        _context = context;
        _filters = expression(Builders<T>.Filter);
    }

    public SoftDelete<T, TProjection> Project(Expression<Func<T, TProjection>> expression)
    {
        if (_options.Projection != null) throw new InvalidOperationException("Projection already set");

        ProjectionDefinition<T, TProjection> Projection(ProjectionDefinitionBuilder<T> p)
        {
            return p.Expression(expression);
        }
        _options.Projection = Projection(Builders<T>.Projection);

        return this;
    }

    public async Task<TProjection> ExecuteAndGetAsync(string userId, bool forceDelete = false, CancellationToken token = default)
    {
        if (!forceDelete) _filters &= Builders<T>.Filter.Eq(t => t.SystemGenerated, false);
        _filters = _context.MergeWithGlobalFilter(_filters);

        if (_options.Projection == null) throw new InvalidOperationException("Projection not set");

        if (Cache<T>.Get().HasModifiedOn) _updates.Set(b => b.CurrentDate(Cache<T>.Get().ModifiedOnPropName));

        _updates.Set(t => t.Deleted, true);
        _updates.Set(t => t.DeletedBy, new DeletedBy { Id = userId });
        _updates.Set(t => t.DeletedOn, DateTime.UtcNow);

        var session = _context.Session;
        var collection = Cache<T>.GetCollection(_context);
        var definition = Builders<T>.Update.Combine(_updates);

        return session == null
            ? await collection.FindOneAndUpdateAsync(_filters, definition, _options, token)
            : await collection.FindOneAndUpdateAsync(session, _filters, definition, _options, token);
    }
}