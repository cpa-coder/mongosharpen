using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen;

public sealed partial class DbContext
{
    public Task SaveAsync<T>(T entity, CancellationToken cancellation = default) where T : IEntity
    {
        PrepareProperties(entity);

        return ForInsert(entity)
            ? _session == null
                ? Cache<T>.GetCollection(this).InsertOneAsync(entity, null, cancellation)
                : Cache<T>.GetCollection(this).InsertOneAsync(_session, entity, null, cancellation)
            : _session == null
                ? Cache<T>.GetCollection(this).ReplaceOneAsync(x => x.Id == entity.Id, entity,
                    new ReplaceOptions { IsUpsert = true }, cancellation)
                : Cache<T>.GetCollection(this).ReplaceOneAsync(_session, x => x.Id == entity.Id, entity,
                    new ReplaceOptions { IsUpsert = true }, cancellation);
    }

    public Task SaveAsync<T>(IEnumerable<T> entities, CancellationToken cancellation = default) where T : IEntity
    {
        var list = entities.ToList();
        var models = new List<WriteModel<T>>(list.Count);

        foreach (var i in list)
        {
            PrepareProperties(i);

            if (ForInsert(i))
                models.Add(new InsertOneModel<T>(i));
            else
                models.Add(new ReplaceOneModel<T>(
                        Builders<T>.Filter.Eq(x => x.Id, i.Id),
                        i)
                    { IsUpsert = true });
        }

        var options = new BulkWriteOptions { IsOrdered = false };

        return _session == null
            ? Cache<T>.GetCollection(this).BulkWriteAsync(models, options, cancellation)
            : Cache<T>.GetCollection(this).BulkWriteAsync(_session, models, options, cancellation);
    }

    private static bool ForInsert<T>(T entity) where T : IEntity => string.IsNullOrEmpty(entity.Id);

    private static void PrepareProperties<T>(T entity) where T : IEntity
    {
        if (ForInsert(entity))
        {
            entity.Id = entity.GenerateId();
            if (!Cache<T>.Get().HasCreatedOn) return;

            var createdOn = (ICreatedOn) entity;
            createdOn.CreatedOn = DateTime.UtcNow;
        }

        if (!Cache<T>.Get().HasModifiedOn) return;

        var modifiedOn = (IModifiedOn) entity;
        modifiedOn.ModifiedOn = DateTime.UtcNow;
    }
}