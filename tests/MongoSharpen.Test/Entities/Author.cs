namespace MongoSharpen.Test.Entities;

[Collection("authors")]
public class Author : Entity, IModifiedOn, ISoftDelete
{
    public string Name { get; set; }
    public ModifiedBy ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public bool SystemGenerated { get; set; }
    public bool Deleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public DeletedBy DeletedBy { get; set; }
}