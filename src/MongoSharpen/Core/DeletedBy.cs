namespace MongoSharpen;

public sealed class DeletedBy : Entity
{
    public DeletedBy()
    {
    }

    private DeletedBy(string value)
    {
        Id = value;
    }

    public static implicit operator DeletedBy(string value) => new(value);
    public static implicit operator string(DeletedBy value) => value.Id;
}