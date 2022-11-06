namespace MongoSharpen;

public sealed class CreatedBy : Entity
{
    public CreatedBy()
    {
    }

    private CreatedBy(string value)
    {
        Id = value;
    }

    public static implicit operator CreatedBy(string value) => new(value);
    public static implicit operator string(CreatedBy value) => value.Id;
}