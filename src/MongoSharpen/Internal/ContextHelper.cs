using MongoDB.Driver;

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

        var client = new MongoClient(connection);
        Instance._clients.TryAdd(connection, client);

        return client;
    }
}