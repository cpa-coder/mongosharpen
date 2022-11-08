using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoSharpen;

public class EntitySerializer : SerializerBase<Entity?>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Entity? value)
    {
        if (value == null)
        {
            context.Writer.WriteString(string.Empty);
            return;
        }

        var id = value.Id;
        if (id.Length == 24 && ObjectId.TryParse(value.Id, out var result))
            context.Writer.WriteObjectId(result);
        else
            context.Writer.WriteString(string.Empty);
    }

    public override Entity? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        switch (context.Reader.CurrentBsonType)
        {
            case BsonType.String:
                return new Entity { Id = context.Reader.ReadString() };

            case BsonType.ObjectId:
                return new Entity { Id = context.Reader.ReadObjectId().ToString()};

            case BsonType.Null:
                context.Reader.ReadNull();
                return null;

            default:
                throw new BsonSerializationException(
                    $"'{context.Reader.CurrentBsonType}' values are not valid " +
                    "on properties decorated with an [AsObjectId] attribute!");
        }
    }
}