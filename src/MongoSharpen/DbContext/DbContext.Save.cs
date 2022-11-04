using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public Task SaveAsync<T>(T entity, CancellationToken token = default) where T : IEntity
    {
        return PrepareAndForInsert(entity)
            ? _session == null
                ? Cache<T>.GetCollection(this).InsertOneAsync(entity, null, token)
                : Cache<T>.GetCollection(this).InsertOneAsync(_session, entity, null, token)
            : _session == null
                ? Cache<T>.GetCollection(this).ReplaceOneAsync(x => x.Id == entity.Id, entity,
                    new ReplaceOptions { IsUpsert = true }, token)
                : Cache<T>.GetCollection(this).ReplaceOneAsync(_session, x => x.Id == entity.Id, entity,
                    new ReplaceOptions { IsUpsert = true }, token);
    }

    public Task SaveAsync<T>(IEnumerable<T> entities, CancellationToken token = default) where T : IEntity
    {
        var list = entities.ToList();
        var models = new List<WriteModel<T>>(list.Count);

        foreach (var i in list)
            if (PrepareAndForInsert(i))
                models.Add(new InsertOneModel<T>(i));
            else
                models.Add(new ReplaceOneModel<T>(Builders<T>.Filter.Eq(x => x.Id, i.Id), i) { IsUpsert = true });

        var options = new BulkWriteOptions { IsOrdered = false };

        return _session == null
            ? Cache<T>.GetCollection(this).BulkWriteAsync(models, options, token)
            : Cache<T>.GetCollection(this).BulkWriteAsync(_session, models, options, token);
    }

    private static bool PrepareAndForInsert<T>(T entity) where T : IEntity
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = entity.GenerateId();

            if (Cache<T>.Get().HasCreatedOn)
            {
                var createdOn = (ICreatedOn) entity;
                createdOn.CreatedOn = DateTime.UtcNow;
            }

            return true;
        }

        if (Cache<T>.Get().HasModifiedOn)
        {
            var modifiedOn = (IModifiedOn) entity;
            modifiedOn.ModifiedOn = DateTime.UtcNow;
        }
        return false;
    }
}