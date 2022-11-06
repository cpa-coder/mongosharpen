using System.Linq.Expressions;
using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen.Builders;

public sealed class Find<T> where T : IEntity
{
    private readonly IDbContext _context;
    private FilterDefinition<T> _filters;
    private readonly List<SortDefinition<T>> _sorts = new();
    private readonly FindOptions<T, T> _options = new();

    public Find(IDbContext context)
    {
        _context = context;
        _filters = Builders<T>.Filter.Empty;
    }

    public Find(IDbContext context, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _context = context;
        _filters = filter.Invoke(Builders<T>.Filter);
    }

    public Find<T> Sort(Action<List<SortDefinition<T>>> sortAction)
    {
        sortAction.Invoke(_sorts);
        return this;
    }

    public Find<T> Skip(int skip)
    {
        _options.Skip = skip;
        return this;
    }

    public Find<T> Limit(int take)
    {
        _options.Limit = take;
        return this;
    }

    public Task<IAsyncCursor<T>> ExecuteCursorAsync(CancellationToken cancellation = default)
    {
        if (_sorts.Count > 0)
            _options.Sort = Builders<T>.Sort.Combine(_sorts);

        var collection = Cache<T>.GetCollection(_context);
        var session = _context.Session;

        _filters = _context.MergeWithGlobalFilter(_filters);

        return session == null
            ? collection.FindAsync(_filters, _options, cancellation)
            : collection.FindAsync(session, _filters, _options, cancellation);
    }

    public async Task<List<T>> ExecuteAsync(CancellationToken cancellation = default)
    {
        var list = new List<T>();
        using var cursor = await ExecuteCursorAsync(cancellation).ConfigureAwait(false);

        while (await cursor.MoveNextAsync(cancellation).ConfigureAwait(false)) list.AddRange(cursor.Current);
        return list;
    }

    public async Task<T> ExecuteSingleAsync(CancellationToken cancellation = default)
    {
        Limit(2); //use take 2 to check if there is more than 1 document

        using var cursor = await ExecuteCursorAsync(cancellation).ConfigureAwait(false);
        await cursor.MoveNextAsync(cancellation).ConfigureAwait(false);
        return cursor.Current.Single();
    }

    public async Task<T> ExecuteFirstAsync(CancellationToken cancellation = default)
    {
        Limit(1); //to prevent fetching more documents than needed

        using var cursor = await ExecuteCursorAsync(cancellation).ConfigureAwait(false);
        await cursor.MoveNextAsync(cancellation).ConfigureAwait(false);
        return cursor.Current.First();
    }

    public Task<T> OneAsync(string id, CancellationToken cancellation = default)
    {
        _filters = Builders<T>.Filter.Empty.MatchId(id);
        return ExecuteSingleAsync(cancellation);
    }

    public Task<List<T>> ManyAsync(Expression<Func<T, bool>> expression, CancellationToken cancellation = default)
    {
        _filters = Builders<T>.Filter.Empty.Match(expression);
        return ExecuteAsync(cancellation);
    }

    public Task<List<T>> ManyAsync(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression,
        CancellationToken cancellation = default)
    {
        _filters = Builders<T>.Filter.Empty.Match(expression);
        return ExecuteAsync(cancellation);
    }

    public async Task<bool> AnyAsync(CancellationToken cancellation = default)
    {
        Limit(1); //to prevent fetching more documents than needed

        using var cursor = await ExecuteCursorAsync(cancellation).ConfigureAwait(false);
        await cursor.MoveNextAsync(cancellation).ConfigureAwait(false);
        return cursor.Current.Any();
    }
}

public sealed class Find<T, TProjection> where T : IEntity
{
    private readonly IDbContext _context;
    private FilterDefinition<T> _filters;
    private readonly List<SortDefinition<T>> _sorts = new();
    private readonly FindOptions<T, TProjection> _options = new();

    public Find(IDbContext context)
    {
        _context = context;
        _filters = Builders<T>.Filter.Empty;
    }

    public Find(IDbContext context, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _context = context;
        _filters = filter.Invoke(Builders<T>.Filter);
    }

    public Find<T, TProjection> Sort(Action<List<SortDefinition<T>>> sortAction)
    {
        sortAction.Invoke(_sorts);
        return this;
    }

    public Find<T, TProjection> Skip(int skip)
    {
        _options.Skip = skip;
        return this;
    }

    public Find<T, TProjection> Limit(int take)
    {
        _options.Limit = take;
        return this;
    }

    public Find<T, TProjection> Project(Expression<Func<T, TProjection>> expression)
    {
        if (_options.Projection != null) throw new InvalidOperationException("Projection already set");

        ProjectionDefinition<T, TProjection> Projection(ProjectionDefinitionBuilder<T> p)
        {
            return p.Expression(expression);
        }
        _options.Projection = Projection(Builders<T>.Projection);

        return this;
    }

    public Task<IAsyncCursor<TProjection>> ExecuteCursorAsync(CancellationToken cancellation = default)
    {
        if (_options.Projection == null) throw new InvalidOperationException("Projection not set");

        if (_sorts.Count > 0)
            _options.Sort = Builders<T>.Sort.Combine(_sorts);

        var collection = Cache<T>.GetCollection(_context);
        var session = _context.Session;

        _filters = _context.MergeWithGlobalFilter(_filters);

        return session == null
            ? collection.FindAsync(_filters, _options, cancellation)
            : collection.FindAsync(session, _filters, _options, cancellation);
    }

    public async Task<List<TProjection>> ExecuteAsync(CancellationToken cancellation = default)
    {
        var list = new List<TProjection>();
        using var cursor = await ExecuteCursorAsync(cancellation).ConfigureAwait(false);

        while (await cursor.MoveNextAsync(cancellation).ConfigureAwait(false)) list.AddRange(cursor.Current);
        return list;
    }

    public async Task<TProjection> ExecuteSingleAsync(CancellationToken cancellation = default)
    {
        Limit(2); //use take 2 to check if there is more than 1 document

        using var cursor = await ExecuteCursorAsync(cancellation).ConfigureAwait(false);
        await cursor.MoveNextAsync(cancellation).ConfigureAwait(false);
        return cursor.Current.SingleOrDefault();
    }

    public async Task<TProjection> ExecuteFirstAsync(CancellationToken cancellation = default)
    {
        Limit(1); //to prevent fetching more documents than needed

        using var cursor = await ExecuteCursorAsync(cancellation).ConfigureAwait(false);
        await cursor.MoveNextAsync(cancellation).ConfigureAwait(false);
        return cursor.Current.FirstOrDefault();
    }

    public Task<TProjection> OneAsync(string id, CancellationToken cancellation = default)
    {
        _filters = Builders<T>.Filter.Empty.MatchId(id);
        return ExecuteSingleAsync(cancellation);
    }

    public Task<List<TProjection>> ManyAsync(Expression<Func<T, bool>> expression, CancellationToken cancellation = default)
    {
        _filters = Builders<T>.Filter.Empty.Match(expression);
        return ExecuteAsync(cancellation);
    }

    public Task<List<TProjection>> ManyAsync(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression,
        CancellationToken cancellation = default)
    {
        _filters = Builders<T>.Filter.Empty.Match(expression);
        return ExecuteAsync(cancellation);
    }
}

public enum Order
{
    Ascending,
    Descending
}