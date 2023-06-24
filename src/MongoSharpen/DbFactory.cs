using System.Reflection;
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

    /// <summary>
    ///     Get an instance of <see cref="IDbFactory" />
    /// </summary>
    public static IDbFactory Instance { get; } = new DbFactoryInternal();

    /// <summary>
    ///     Adds a convention to the default <see cref="ConventionRegistry" />.
    /// </summary>
    /// <TIP>Add only before getting any <see cref="IDbContext" /> instance.</TIP>
    /// <param name="name">Register pack name</param>
    /// <param name="convention">A convention</param>
    public static void AddConvention(string name, IConvention convention)
    {
        Instance.AddConvention(name, convention);
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
    ///     Get or set the default <see cref="IDbContext" /> database name.
    /// </summary>
    public static string DefaultDatabase
    {
        get => Instance.DefaultDatabase;
        set => Instance.DefaultDatabase = value;
    }

    /// <summary>
    ///     Get <see cref="DbContext" /> object with default database and connection
    /// </summary>
    /// <param name="ignoreGlobalFilter">Indicate whether to ignore global filter</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Throws when no default database has been setup</exception>
    /// <exception cref="InvalidOperationException">Throws when no default connection has been setup</exception>
    public static IDbContext Get(bool ignoreGlobalFilter = false) => Instance.Get(ignoreGlobalFilter);

    /// <summary>
    ///     Get <see cref="IDbContext" /> instance with default database connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="ignoreGlobalFilter">Indicate whether to ignore global filter</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws when no default connection has been setup</exception>
    public static IDbContext Get(string database, bool ignoreGlobalFilter = false) => Instance.Get(database, ignoreGlobalFilter);

    /// <summary>
    ///     Get <see cref="IDbContext" /> instance with custom connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="connection">The connection other than default connection</param>
    /// <param name="ignoreGlobalFilter">Indicate whether to ignore global filter</param>
    /// <returns></returns>
    public static IDbContext Get(string database, string connection, bool ignoreGlobalFilter = false) =>
        Instance.Get(database, connection, ignoreGlobalFilter);

    /// <summary>
    ///     Get the list of all <see cref="IDbContext" /> instances
    /// </summary>
    public static List<IDbContext> DbContexts => Instance.DbContexts;

    /// <summary>
    ///     Set global filter for all <see cref="IDbContext" /> instances using JSON string.
    /// </summary>
    /// <param name="jsonString">Must be in a JSON format</param>
    /// <param name="assembly">Specific assembly of types to apply filter with</param>
    /// <param name="prepend">Set if global filter is merge before or after any other filter in any database execution</param>
    /// <typeparam name="T">Must be an interface</typeparam>
    public static void SetGlobalFilter<T>(string jsonString, Assembly assembly, bool prepend = false) =>
        Instance.SetGlobalFilter<T>(jsonString, assembly, prepend);

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