using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoSharpen.Builders;
using MongoSharpen.Internal;

namespace MongoSharpen;

internal sealed partial class DbContext : IDbContext
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private IClientSessionHandle? _session;

    IMongoClient IDbContext.Client => _client;

    IClientSessionHandle? IDbContext.Session
    {
        get => _session;
        set => _session = value;
    }

    IMongoDatabase IDbContext.Database => _database;

    internal DbContext(string database, string connectionString)
    {
        _client = ContextHelper.GetClient(connectionString);
        _database = _client.GetDatabase(database);
    }

    public async Task<bool> ExistAsync()
    {
        var databases = await _client.ListDatabaseNamesAsync();
        var exist = false;

        var currentDb = _database.DatabaseNamespace;

        while (await databases.MoveNextAsync())
        {
            var found = databases.Current.Any(s => s == currentDb.DatabaseName);
            if (!found) continue;

            exist = true;
        }

        return exist;
    }

    public Transaction Transaction(ClientSessionOptions? options = null) => new(this, options);

    public Task DropDataBaseAsync(CancellationToken token = default)
    {
        var database = _database.DatabaseNamespace;
        return _client.DropDatabaseAsync(database.DatabaseName, token);
    }

    public IMongoCollection<T> Collection<T>() where T : IEntity => Cache<T>.GetCollection(this);

    public IMongoQueryable<T> Queryable<T>(ClientSessionOptions? options = null) where T : IEntity
    {
        _session = _client.StartSession(options);
        return Cache<T>.GetCollection(this).AsQueryable(_session);
    }
}