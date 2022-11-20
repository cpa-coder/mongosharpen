namespace MongoSharpen;

public interface ISoftDelete : ISystemGenerated
{
    bool Deleted { get; set; }
    DateTime? DeletedOn { get; set; }
    DeletedBy? DeletedBy { get; set; }
}