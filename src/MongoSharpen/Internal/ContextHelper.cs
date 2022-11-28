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
        if (Instance._clients.ContainsKey(connection)) return Instance._clients[connection];

        var settings = MongoClientSettings.FromConnectionString(connection);
        settings.LinqProvider = LinqProvider.V3;

        var client = new MongoClient(settings);
        Instance._clients.TryAdd(connection, client);

        return client;
    }
}