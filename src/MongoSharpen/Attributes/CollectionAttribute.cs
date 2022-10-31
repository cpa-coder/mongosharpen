namespace MongoSharpen;

/// <summary>
///     Specifies a custom MongoDB collection name for an entity type.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class CollectionAttribute : Attribute
{
    public string Name { get; }

    public CollectionAttribute(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        Name = name;
    }
}