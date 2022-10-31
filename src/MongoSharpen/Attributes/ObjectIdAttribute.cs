using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoSharpen;

/// <summary>
///     Use this attribute to mark a property in order to save it in MongoDB server as ObjectId
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ObjectIdAttribute : BsonRepresentationAttribute
{
    public ObjectIdAttribute()
        : base(BsonType.ObjectId)
    {
    }
}