namespace MongoSharpen;

public struct DeleteResult
{
    public bool Acknowledge { get; set; }
    public long DeletedCount { get; set; }
}