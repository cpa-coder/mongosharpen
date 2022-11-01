using MongoDB.Bson;
using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    private const string Log = "log";

    private static BsonDocument ToBsonDocument<T>(T x) where T : IEntity
    {
        var doc = x.ToBsonDocument();
        doc.Set("old_id", ObjectId.Parse(x.Id));
        doc.Set("_id", ObjectId.GenerateNewId());
        return doc;
    }

    private async Task LogInternalAsync<T>(BsonDocument doc, CancellationToken token) where T : IEntity
    {
        if (_session == null)
            await Cache<T>.GetCollection<BsonDocument>(this, Log).InsertOneAsync(doc, null, token);
        else
            await Cache<T>.GetCollection<BsonDocument>(this, Log).InsertOneAsync(_session, doc, null, token);
    }

    private async Task LogInternalAsync<T>(IEnumerable<BsonDocument> docs, CancellationToken token) where T : IEntity
    {
        if (_session == null)
            await Cache<T>.GetCollection<BsonDocument>(this, Log).InsertManyAsync(docs, null, token);
        else
            await Cache<T>.GetCollection<BsonDocument>(this, Log).InsertManyAsync(_session, docs, null, token);
    }

    public async Task LogAsync<T>(string id, CancellationToken token = default) where T : IEntity
    {
        var entity = await Find<T>(x => x.MatchId(id)).ExecuteFirstAsync(token);
        var doc = ToBsonDocument(entity);
        await LogInternalAsync<T>(doc, token);
    }

    public async Task LogAsync<T>(T entity, CancellationToken token = default) where T : IEntity
    {
        var doc = ToBsonDocument(entity);
        await LogInternalAsync<T>(doc, token);
    }

    public async Task LogAsync<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression,
        CancellationToken token = default) where T : IEntity
    {
        var entities = await Find(expression).ExecuteAsync(token);
        var docs = entities.Select(ToBsonDocument);
        await LogInternalAsync<T>(docs, token);
    }

    public async Task LogAsync<T>(IEnumerable<T> entities, CancellationToken token = default) where T : IEntity
    {
        var docs = entities.Select(ToBsonDocument);
        await LogInternalAsync<T>(docs, token);
    }
}