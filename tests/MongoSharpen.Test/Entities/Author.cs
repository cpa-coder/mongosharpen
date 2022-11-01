namespace MongoSharpen.Test.Entities;

[Collection("authors")]
public class Author : Entity
{
    public string Name { get; set; }
}