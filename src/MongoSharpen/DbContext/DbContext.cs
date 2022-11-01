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

    public Transaction Transaction(ClientSessionOptions? options = null) => new(this, options);

    public Task DropDataBaseAsync(CancellationToken token = default)
    {
        var database = _database.DatabaseNamespace;
        return _client.DropDatabaseAsync(database.DatabaseName, token);
    }

    public IMongoQueryable<T> Queryable<T>() where T : IEntity => Cache<T>.GetCollection(this).AsQueryable(_session);
    public IMongoCollection<T> Collection<T>() where T : IEntity => Cache<T>.GetCollection(this);
}