using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace MongoSharpen;

/// <summary>
///     Access to <see cref="IDbContext" /> for common database operations.
/// </summary>
public static class DbFactory
{
    static DbFactory()
    {
    }

    private static DbFactoryInternal Instance { get; } = new(new ConventionRegistryWrapper());

    /// <summary>
    ///     Adds a convention to the default <see cref="ConventionRegistry" />.
    /// </summary>
    /// <TIP>Add only before getting any <see cref="IDbContext" /> instance.</TIP>
    /// <param name="name">Register pack name</param>
    /// <param name="pack">Actual convention pack</param>
    public static void AddConvention(string name, ConventionPack pack)
    {
        Instance.AddConvention(name, pack);
    }

    /// <summary>
    ///     Removes a convention from the default <see cref="ConventionRegistry" />.
    /// </summary>
    /// <TIP>Remove only before getting any <see cref="IDbContext" /> instance.</TIP>
    /// <param name="name">Registered pack name to remove</param>
    public static void RemoveConvention(string name)
    {
        Instance.RemoveConvention(name);
    }

    /// <summary>
    ///     List of all conventions registered in the default <see cref="ConventionRegistry" />.
    /// </summary>
    public static List<string> ConventionNames => Instance.ConventionNames;

    /// <summary>
    ///     Get or set the default <see cref="IDbContext" /> connection.
    /// </summary>
    public static string DefaultConnection
    {
        get => Instance.DefaultConnection;
        set => Instance.DefaultConnection = value;
    }

    /// <summary>
    ///     Get <see cref="IDbContext" /> instance with default database connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws when no default connection has been setup</exception>
    public static IDbContext Get(string database) => Instance.Get(database);

    /// <summary>
    ///     Get <see cref="IDbContext" /> instance with custom connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="connection">The connection other than default connection</param>
    /// <returns></returns>
    public static IDbContext Get(string database, string connection) => Instance.Get(database, connection);

    public static List<IDbContext> DbContexts => Instance.DbContexts;

    /// <summary>
    ///     Set global filter for all <see cref="IDbContext" /> instances using JSON string.
    /// </summary>
    /// <param name="jsonString">Must be in a JSON format</param>
    /// <param name="prepend">Set if global filter is merge before or after any other filter in any database execution</param>
    /// <typeparam name="T">Must be an interface</typeparam>
    public static void SetGlobalFilter<T>(string jsonString, bool prepend = false) =>
        Instance.SetGlobalFilter<T>(jsonString, prepend);

    /// <summary>
    ///     Set global filter for all <see cref="IDbContext" /> instances using filter definition.
    /// </summary>
    /// <param name="filter">A filter definition</param>
    /// <param name="prepend">Set if global filter is merge before or after any other filter in any database execution</param>
    /// <typeparam name="T">Must be of type <see cref="IEntity" /></typeparam>
    public static void SetGlobalFilter<T>(FilterDefinition<T> filter, bool prepend = false) where T : IEntity =>
        Instance.SetGlobalFilter(filter, prepend);

    /// <summary>
    ///     Set global filter for all <see cref="IDbContext" /> instances using <see cref="BsonDocument" />.
    /// </summary>
    /// <param name="document">A <see cref="BsonDocument" /></param>
    /// <param name="prepend">Set if global filter is merge before or after any other filter in any database execution</param>
    /// <typeparam name="T">Must be of type <see cref="IEntity" /></typeparam>
    public static void SetGlobalFilter<T>(BsonDocument document, bool prepend = false) where T : IEntity =>
        Instance.SetGlobalFilter<T>(document, prepend);
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