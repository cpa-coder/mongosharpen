namespace MongoSharpen.Test.Entities;

[Collection("authors")]
public class Author : Entity, IModifiedOn
{
    public string Name { get; set; }
    public ModifiedBy ModifiedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
}