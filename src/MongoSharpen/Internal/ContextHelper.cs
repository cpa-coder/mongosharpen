using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoSharpen.Internal;

internal sealed class ContextHelper
{
    private readonly Dictionary<string, MongoClient> _clients;

    static ContextHelper()
    {
    }

    private ContextHelper()
    {
        _clients = new Dictionary<string, MongoClient>();
    }

    private static ContextHelper Instance { get; } = new();

    public static IMongoClient GetClient(string connection)
    {
        if (Instance._clients.TryGetValue(connection, out var existing)) return existing;

        var settings = MongoClientSettings.FromConnectionString(connection);
        settings.LinqProvider = LinqProvider.V3;

        var client = new MongoClient(settings);

       if(Instance._clients.TryGetValue(connection, out var existingClient))
            return existingClient;

       Instance._clients.Add(connection, client);
       return client;
    }
}