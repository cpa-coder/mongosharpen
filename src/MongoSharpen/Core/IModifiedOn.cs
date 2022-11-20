namespace MongoSharpen;

public interface IModifiedOn
{
    ModifiedBy? ModifiedBy { get; set; }

    DateTime? ModifiedOn { get; set; }
}