namespace MongoSharpen;

public interface IDeleteOn : ISystemGenerated
{
    bool Deleted { get; set; }
    DateTime? DeletedOn { get; set; }
    DeletedBy? DeletedBy { get; set; }
}