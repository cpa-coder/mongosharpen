using MongoDB.Bson.Serialization.Conventions;

namespace MongoSharpen;

public sealed class DbFactory
{
    static DbFactory()
    {
    }

    private DbFactory()
    {
    }

    private static DbFactoryInternal Instance { get; } = new(new ConventionRegistryWrapper());

    public static void AddConvention(string name, ConventionPack pack)
    {
        Instance.AddConvention(name, pack);
    }

    public static void RemoveConvention(string name)
    {
        Instance.RemoveConvention(name);
    }

    public static List<string> ConventionNames => Instance.ConventionNames;

    public static string DefaultConnection
    {
        get => Instance.DefaultConnection;
        set => Instance.DefaultConnection = value;
    }

    /// <summary>
    ///     Get <see cref="DbContext" /> object with default database connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws when no default connection has been setup</exception>
    public static IDbContext Get(string database) => Instance.Get(database);

    /// <summary>
    ///     Get <see cref="DbContext" /> object with custom connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="connection">The connection other than default connection</param>
    /// <returns></returns>
    public static IDbContext Get(string database, string connection) => Instance.Get(database, connection);

    public static List<IDbContext> DbContexts => Instance.DbContexts;
}

internal interface IConventionRegistryWrapper
{
    void Register(string name, IConventionPack conventions);
}

internal class ConventionRegistryWrapper : IConventionRegistryWrapper
{
    public void Register(string name, IConventionPack conventions)
        => ConventionRegistry.Register(name, conventions, _ => true);
}