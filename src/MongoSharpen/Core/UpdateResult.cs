namespace MongoSharpen;

public struct UpdateResult
{
    public bool Acknowledge { get; set; }
    public long ModifiedCount { get; set; }
    public long MatchedCount { get; set; }
}