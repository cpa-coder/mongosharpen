using System.Reflection;
using MongoDB.Driver;

namespace MongoSharpen.Internal;

internal sealed class Cache<T>
{
    private readonly Type _type;

    static Cache()
    {
    }

    private Cache()
    {
        _type = typeof(T);
        var interfaces = _type.GetInterfaces();
        HasCreatedOn = interfaces.Any(i => i == typeof(ICreatedOn));
        HasModifiedOn = interfaces.Any(i => i == typeof(IModifiedOn));
        ForSystemGeneration = interfaces.Any(i => i == typeof(ISystemGenerated));
    }

    public bool HasModifiedOn { get; }
    public bool HasCreatedOn { get; }
    public bool ForSystemGeneration { get; }

    public string ModifiedOnPropName => nameof(IModifiedOn.ModifiedOn);

    private static Lazy<Cache<T>> Instance { get; } = new(() => new Cache<T>());

    public static Cache<T> Get() => Instance.Value;

    public static IMongoCollection<T> GetCollection(DbContext context) =>
        context.Database.GetCollection<T>(GetCollectionName());

    private static string GetCollectionName()
    {
        var attribute = Instance.Value._type.GetCustomAttributes<CollectionAttribute>(false).FirstOrDefault();

        var name = attribute != null ? attribute.Name : Instance.Value._type.Name;
        if (string.IsNullOrWhiteSpace(name) || name.Contains("~"))
            throw new ArgumentException($"{name} is an illegal name for a collection!");

        return name;
    }

    // use for logging using bson document
    public static IMongoCollection<TDocument> GetCollection<TDocument>(DbContext context, string documentSuffix) =>
        context.Database.GetCollection<TDocument>($"{GetCollectionName()}.{documentSuffix}");
}