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

    public Task<IAsyncCursor<T>> ExecuteCursorAsync(CancellationToken token = default)
    {
        if (_sorts.Count > 0)
            _options.Sort = Builders<T>.Sort.Combine(_sorts);

        var collection = Cache<T>.GetCollection(_context);
        var session = _context.Session;

        _filters = _context.MergeWithGlobalFilter(_filters);

        return session == null
            ? collection.FindAsync(_filters, _options, token)
            : collection.FindAsync(session, _filters, _options, token);
    }

    public async Task<List<T>> ExecuteAsync(CancellationToken token = default)
    {
        var list = new List<T>();
        using var cursor = await ExecuteCursorAsync(token).ConfigureAwait(false);

        while (await cursor.MoveNextAsync(token).ConfigureAwait(false)) list.AddRange(cursor.Current);
        return list;
    }

    public async Task<T> ExecuteSingleAsync(CancellationToken token = default)
    {
        Limit(2); //use take 2 to check if there is more than 1 document

        using var cursor = await ExecuteCursorAsync(token).ConfigureAwait(false);
        await cursor.MoveNextAsync(token).ConfigureAwait(false);
        return cursor.Current.Single();
    }

    public async Task<T?> ExecuteSingleOrDefaultAsync(CancellationToken token = default)
    {
        Limit(2); //use take 2 to check if there is more than 1 document

        using var cursor = await ExecuteCursorAsync(token).ConfigureAwait(false);
        await cursor.MoveNextAsync(token).ConfigureAwait(false);
        return cursor.Current.SingleOrDefault();
    }

    public async Task<T> ExecuteFirstAsync(CancellationToken token = default)
    {
        Limit(1); //to prevent fetching more documents than needed

        using var cursor = await ExecuteCursorAsync(token).ConfigureAwait(false);
        await cursor.MoveNextAsync(token).ConfigureAwait(false);
        return cursor.Current.First();
    }

    public async Task<T?> ExecuteFirstOrDefaultAsync(CancellationToken token = default)
    {
        Limit(1); //to prevent fetching more documents than needed

        using var cursor = await ExecuteCursorAsync(token).ConfigureAwait(false);
        await cursor.MoveNextAsync(token).ConfigureAwait(false);
        return cursor.Current.FirstOrDefault();
    }

    public Task<T> OneAsync(string id, CancellationToken token = default)
    {
        _filters = Builders<T>.Filter.Empty.MatchId(id);
        return ExecuteSingleAsync(token);
    }

    public Task<T?> OneOrDefaultAsync(string id, CancellationToken token = default)
    {
        _filters = Builders<T>.Filter.Empty.MatchId(id);
        return ExecuteSingleOrDefaultAsync(token);
    }

    public Task<List<T>> ManyAsync(Expression<Func<T, bool>> expression, CancellationToken token = default)
    {
        _filters = Builders<T>.Filter.Empty.Match(expression);
        return ExecuteAsync(token);
    }

    public Task<List<T>> ManyAsync(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression,
        CancellationToken token = default)
    {
        _filters = Builders<T>.Filter.Empty.Match(expression);
        return ExecuteAsync(token);
    }

    public async Task<bool> AnyAsync(CancellationToken token = default)
    {
        Limit(1); //to prevent fetching more documents than needed

        using var cursor = await ExecuteCursorAsync(token).ConfigureAwait(false);
        await cursor.MoveNextAsync(token).ConfigureAwait(false);
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

    public Task<IAsyncCursor<TProjection>> ExecuteCursorAsync(CancellationToken token = default)
    {
        if (_options.Projection == null) throw new InvalidOperationException("Projection not set");

        if (_sorts.Count > 0)
            _options.Sort = Builders<T>.Sort.Combine(_sorts);

        var collection = Cache<T>.GetCollection(_context);
        var session = _context.Session;

        _filters = _context.MergeWithGlobalFilter(_filters);

        return session == null
            ? collection.FindAsync(_filters, _options, token)
            : collection.FindAsync(session, _filters, _options, token);
    }

    public async Task<List<TProjection>> ExecuteAsync(CancellationToken token = default)
    {
        var list = new List<TProjection>();
        using var cursor = await ExecuteCursorAsync(token).ConfigureAwait(false);

        while (await cursor.MoveNextAsync(token).ConfigureAwait(false)) list.AddRange(cursor.Current);
        return list;
    }

    public async Task<TProjection> ExecuteSingleAsync(CancellationToken token = default)
    {
        Limit(2); //use take 2 to check if there is more than 1 document

        using var cursor = await ExecuteCursorAsync(token).ConfigureAwait(false);
        await cursor.MoveNextAsync(token).ConfigureAwait(false);
        return cursor.Current.Single();
    }

    public async Task<TProjection?> ExecuteSingleOrDefaultAsync(CancellationToken token = default)
    {
        Limit(2); //use take 2 to check if there is more than 1 document

        using var cursor = await ExecuteCursorAsync(token).ConfigureAwait(false);
        await cursor.MoveNextAsync(token).ConfigureAwait(false);
        return cursor.Current.SingleOrDefault();
    }

    public async Task<TProjection> ExecuteFirstAsync(CancellationToken token = default)
    {
        Limit(1); //to prevent fetching more documents than needed

        using var cursor = await ExecuteCursorAsync(token).ConfigureAwait(false);
        await cursor.MoveNextAsync(token).ConfigureAwait(false);
        return cursor.Current.First();
    }

    public async Task<TProjection?> ExecuteFirstOrDefaultAsync(CancellationToken token = default)
    {
        Limit(1); //to prevent fetching more documents than needed

        using var cursor = await ExecuteCursorAsync(token).ConfigureAwait(false);
        await cursor.MoveNextAsync(token).ConfigureAwait(false);
        return cursor.Current.FirstOrDefault();
    }

    public Task<TProjection> OneAsync(string id, CancellationToken token = default)
    {
        _filters = Builders<T>.Filter.Empty.MatchId(id);
        return ExecuteSingleAsync(token);
    }

    public Task<TProjection?> OneOrDefaultAsync(string id, CancellationToken token = default)
    {
        _filters = Builders<T>.Filter.Empty.MatchId(id);
        return ExecuteSingleOrDefaultAsync(token);
    }

    public Task<List<TProjection>> ManyAsync(Expression<Func<T, bool>> expression, CancellationToken token = default)
    {
        _filters = Builders<T>.Filter.Empty.Match(expression);
        return ExecuteAsync(token);
    }

    public Task<List<TProjection>> ManyAsync(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression,
        CancellationToken token = default)
    {
        _filters = Builders<T>.Filter.Empty.Match(expression);
        return ExecuteAsync(token);
    }
}

public enum Order
{
    Ascending,
    Descending
}