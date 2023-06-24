using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen.Builders;

public interface IDistinct<T, TProperty> where T : IEntity
{
    IDistinct<T, TProperty> Property(string property);
    Task<List<TProperty>> ExecuteAsync(CancellationToken cancellation = default);
}

internal sealed class Distinct<T, TProperty> : IDistinct<T, TProperty> where T : IEntity
{
    private readonly DbContext _context;
    private FieldDefinition<T, TProperty>? _field;
    private FilterDefinition<T> _filters;

    public Distinct(DbContext context, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter)
    {
        _context = context;
        _filters = filter.Invoke(Builders<T>.Filter);
    }

    public Distinct(DbContext context)
    {
        _context = context;
        _filters = FilterDefinition<T>.Empty;
    }

    public IDistinct<T, TProperty> Property(string property)
    {
        if (_field != null) throw new InvalidOperationException("Property already set");

        _field = property;
        return this;
    }

    private Task<IAsyncCursor<TProperty>> ExecuteCursorAsync(CancellationToken token = default)
    {
        if (_field == null)
            throw new InvalidOperationException("Please specify what property to use for obtaining unique values");

        _filters = _context.MergeWithGlobalFilter(_filters);

        return _context.Session == null
            ? Cache<T>.GetCollection(_context).DistinctAsync(_field, _filters, null, token)
            : Cache<T>.GetCollection(_context).DistinctAsync(_context.Session, _field, _filters, null, token);
    }

    public async Task<List<TProperty>> ExecuteAsync(CancellationToken cancellation = default)
    {
        var list = new List<TProperty>();
        using var csr = await ExecuteCursorAsync(cancellation).ConfigureAwait(false);

        while (await csr.MoveNextAsync(cancellation).ConfigureAwait(false)) list.AddRange(csr.Current);
        return list;
    }
}