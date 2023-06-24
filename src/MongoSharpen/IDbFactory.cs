using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace MongoSharpen;

public interface IDbFactory
{
    /// <summary>
    ///     Adds a convention to the default <see cref="ConventionRegistry" />.
    /// </summary>
    /// <TIP>Add only before getting any <see cref="IDbContext" /> instance.</TIP>
    /// <param name="name">Register convention name</param>
    /// <param name="convention">A convention</param>
    void AddConvention(string name, IConvention convention);

    /// <summary>
    ///     Removes a convention from the default <see cref="ConventionRegistry" />.
    /// </summary>
    /// <TIP>Remove only before getting any <see cref="IDbContext" /> instance.</TIP>
    /// <param name="name">Registered convention name to remove</param>
    void RemoveConvention(string name);

    /// <summary>
    ///     List of all conventions registered in the default <see cref="ConventionRegistry" />.
    /// </summary>
    List<string> ConventionNames { get; }

    /// <summary>
    ///     Check if any convention has been registered.
    /// </summary>
    public bool HasRegisteredConventions { get; }

    /// <summary>
    ///     Get or set the default <see cref="IDbContext" /> connection.
    /// </summary>
    string DefaultConnection { get; set; }

    /// <summary>
    ///     Get or set the default <see cref="IDbContext" /> database name.
    /// </summary>
    string DefaultDatabase { get; set; }

    /// <summary>
    ///     Get the list of all <see cref="IDbContext" /> instances
    /// </summary>
    List<IDbContext> DbContexts { get; }

    /// <summary>
    ///     Get <see cref="DbContext" /> object with default database and connection
    /// </summary>
    /// <param name="ignoreGlobalFilter">Indicate whether to ignore global filter</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Throws when no default database has been setup</exception>
    /// <exception cref="InvalidOperationException">Throws when no default connection has been setup</exception>
    IDbContext Get(bool ignoreGlobalFilter = false);

    /// <summary>
    ///     Get <see cref="DbContext" /> object with default database connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="ignoreGlobalFilter">Indicate whether to ignore global filter</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws when no default connection has been setup</exception>
    IDbContext Get(string database, bool ignoreGlobalFilter = false);

    /// <summary>
    ///     Get <see cref="DbContext" /> object with custom connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="connection">The connection other than default connection</param>
    /// <param name="ignoreGlobalFilter">Indicate whether to ignore global filter</param>
    /// <returns></returns>
    IDbContext Get(string database, string connection, bool ignoreGlobalFilter = false);

    /// <summary>
    ///     Set global filter for all <see cref="IDbContext" /> instances using JSON string.
    /// </summary>
    /// <param name="jsonString">Must be in a JSON format</param>
    /// <param name="assembly">Specific assembly of types to apply filter with</param>
    /// <param name="prepend">Set if global filter is merge before or after any other filter in any database execution</param>
    /// <typeparam name="T">Must be an interface</typeparam>
    void SetGlobalFilter<T>(string jsonString, Assembly assembly, bool prepend = false);

    /// <summary>
    ///     Set global filter for all <see cref="IDbContext" /> instances using filter definition.
    /// </summary>
    /// <param name="filter">A filter definition</param>
    /// <param name="prepend">Set if global filter is merge before or after any other filter in any database execution</param>
    /// <typeparam name="T">Must be of type <see cref="IEntity" /></typeparam>
    void SetGlobalFilter<T>(FilterDefinition<T> filter, bool prepend = false) where T : IEntity;

    /// <summary>
    ///     Set global filter for all <see cref="IDbContext" /> instances using <see cref="BsonDocument" />.
    /// </summary>
    /// <param name="document">A <see cref="BsonDocument" /></param>
    /// <param name="prepend">Set if global filter is merge before or after any other filter in any database execution</param>
    /// <typeparam name="T">Must be of type <see cref="IEntity" /></typeparam>
    void SetGlobalFilter<T>(BsonDocument document, bool prepend = false) where T : IEntity;
}