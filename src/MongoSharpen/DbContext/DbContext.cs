using MongoDB.Bson;
using MongoDB.Driver;
using MongoSharpen.Builders;
using MongoSharpen.Internal;

namespace MongoSharpen;

internal sealed partial class DbContext : IDbContext
{
    private readonly bool _ignoreGlobalFilters;
    private readonly GlobalFilter _globalFilter;

    internal IMongoClient Client { get; }

    internal IClientSessionHandle? Session { get; set; }
    internal IMongoDatabase Database { get; }

    internal DbContext(string database,
        string connectionString,
        bool ignoreGlobalFilters,
        GlobalFilter globalFilter)
    {
        _globalFilter = globalFilter;
        _ignoreGlobalFilters = ignoreGlobalFilters;
        Client = ContextHelper.GetClient(connectionString);
        Database = Client.GetDatabase(database);
    }

    public async Task<bool> ExistAsync()
    {
        var databases = await Client.ListDatabaseNamesAsync();
        var exist = false;

        var currentDb = Database.DatabaseNamespace;

        while (await databases.MoveNextAsync())
        {
            var found = databases.Current.Any(s => s == currentDb.DatabaseName);
            if (!found) continue;

            exist = true;
        }

        return exist;
    }

    public ITransaction Transaction(ClientSessionOptions? options = null) => new Transaction(this, options);

    public Task DropDataBaseAsync(CancellationToken token = default)
    {
        var database = Database.DatabaseNamespace;
        return Client.DropDatabaseAsync(database.DatabaseName, token);
    }

    public IMongoCollection<T> Collection<T>() where T : IEntity => Cache<T>.GetCollection(this);
    public IMongoCollection<BsonDocument> CollectionLog<T>() where T : IEntity => Cache<T>.GetCollection<BsonDocument>(this, "log");

    internal FilterDefinition<T> MergeWithGlobalFilter<T>(FilterDefinition<T> filter) =>
        _ignoreGlobalFilters ? filter : _globalFilter.Merge(filter);
}