using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoSharpen;

/// <summary>
///     Set as default _id in MongoDB
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class AsObjectIdAttribute : BsonSerializerAttribute
{
    public AsObjectIdAttribute() : base(typeof(ObjectIdSerializer))
    {
    }

    private class ObjectIdSerializer : SerializerBase<string>, IRepresentationConfigurable
    {
        public BsonType Representation { get; set; }

        public override void Serialize(BsonSerializationContext ctx, BsonSerializationArgs args, string? value)
        {
            if (value == null)
            {
                ctx.Writer.WriteNull();
                return;
            }

            if (value.Length == 24 && ObjectId.TryParse(value, out var id))
            {
                ctx.Writer.WriteObjectId(id);
                return;
            }

            ctx.Writer.WriteString(value);
        }

        public override string Deserialize(BsonDeserializationContext ctx, BsonDeserializationArgs args)
        {
            switch (ctx.Reader.CurrentBsonType)
            {
                case BsonType.String:
                    return ctx.Reader.ReadString();

                case BsonType.ObjectId:
                    return ctx.Reader.ReadObjectId().ToString();

                case BsonType.Null:
                    ctx.Reader.ReadNull();
                    return string.Empty;

                default:
                    throw new BsonSerializationException(
                        $"'{ctx.Reader.CurrentBsonType}' values are not valid " +
                        "on properties decorated with an [AsObjectId] attribute!");
            }
        }

        public IBsonSerializer WithRepresentation(BsonType representation) => throw new NotImplementedException();
    }
}