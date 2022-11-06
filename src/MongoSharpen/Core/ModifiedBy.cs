namespace MongoSharpen;

public sealed class ModifiedBy : Entity
{
    public ModifiedBy()
    {
    }

    private ModifiedBy(string value)
    {
        Id = value;
    }

    public static implicit operator ModifiedBy(string value) => new(value);
    public static implicit operator string(ModifiedBy value) => value.Id;
}