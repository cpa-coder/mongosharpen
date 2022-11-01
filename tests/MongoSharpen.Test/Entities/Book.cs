using System.Collections.Immutable;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoSharpen.Test.Entities;

[Collection("books")]
public class Book : Entity, ICreatedOn, IModifiedOn
{
    public string Title { get; set; }

    [BsonElement("ISBN")]
    public string ISBN { get; set; }
    public IEnumerable<Author> Authors { get; set; }
    public CreatedBy CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public ModifiedBy ModifiedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
}