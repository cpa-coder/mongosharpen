using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoSharpen;

public interface IEntity
{
    string Id { get; set; }

    string GenerateId();
}

public class Entity : IEntity
{
    [BsonId]
    [AsObjectId]
    public string Id { get; set; } = string.Empty;

    public virtual string GenerateId() => ObjectId.GenerateNewId().ToString();

    public void SetNewId() => Id = GenerateId();
}