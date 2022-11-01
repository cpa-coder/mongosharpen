using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoSharpen.Internal;

namespace MongoSharpen;

internal sealed partial class DbContext : IDbContext
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private IClientSessionHandle? _session;

    IClientSessionHandle? IDbContext.Session => _session;
    IMongoDatabase IDbContext.Database => _database;

    internal DbContext(string database, string connectionString)
    {
        _client = ContextHelper.GetClient(connectionString);
        _database = _client.GetDatabase(database);
    }

    public void StartTransaction(ClientSessionOptions? options = null)
    {
        if (_session != null)
            throw new InvalidOperationException("Transaction already started");

        _session = _client.StartSession(options);
        _session.StartTransaction();
    }

    public async Task CommitAsync(CancellationToken cancellation = default)
    {
        if (_session == null)
            throw new InvalidOperationException("No transaction started");

        await _session.CommitTransactionAsync(cancellation);

        // to be able to start a new transaction with the same db context
        _session.Dispose();
        _session = null;
    }

    public Task DropDataBaseAsync(CancellationToken token = default)
    {
        var database = _database.DatabaseNamespace;
        return _client.DropDatabaseAsync(database.DatabaseName, token);
    }

    public IMongoQueryable<T> Queryable<T>() where T : IEntity => Cache<T>.GetCollection(this).AsQueryable(_session);
    public IMongoCollection<T> Collection<T>() where T : IEntity => Cache<T>.GetCollection(this);

    public void Dispose()
    {
        _session?.Dispose();
    }
}