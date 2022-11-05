namespace MongoSharpen;

public interface ICreatedOn
{
    CreatedBy CreatedBy { get; set; }

    DateTime CreatedOn { get; set; }
}