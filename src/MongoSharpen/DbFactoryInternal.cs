using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace MongoSharpen;

internal sealed class DbFactoryInternal : IDbFactory
{
    private readonly GlobalFilter _globalFilter;

    /// <summary>
    ///     Instantiate DbFactory for internal testing
    /// </summary>
    public DbFactoryInternal()
    {
        DbContexts = new List<IDbContext>();
        _conventions = new Dictionary<string, IConvention>
        {
            { "camelCase", new CamelCaseElementNameConvention() },
        };
        _globalFilter = new GlobalFilter();
    }

    public bool HasRegisteredConventions { get; private set; }

    private readonly Dictionary<string, IConvention> _conventions;

    public void AddConvention(string name, IConvention convention)
    {
        if (HasRegisteredConventions)
            throw new InvalidOperationException(
                "All conventions are already registered. Make sure to add or remove convention pack " +
                $"before getting any {nameof(DbContext)} in the {nameof(DbFactory)}.");

        _conventions.TryAdd(name, convention);
    }

    public void RemoveConvention(string name)
    {
        if (HasRegisteredConventions)
            throw new InvalidOperationException(
                "All conventions are already registered. Make sure to add or remove convention pack " +
                $"before getting any {nameof(DbContext)} in the {nameof(DbFactory)}.");

        _conventions.Remove(name);
    }

    private void RegisterConventions()
    {
        var pack = new ConventionPack();
        pack.AddRange(_conventions.Select(x => x.Value));

        ConventionRegistry.Register("conventions", pack, _ => true);

        HasRegisteredConventions = true;
    }

    public List<string> ConventionNames => _conventions.Keys.ToList();

    private string _defaultConnection = string.Empty;

    public string DefaultConnection
    {
        get => _defaultConnection;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Invalid connection string");

            if (!string.IsNullOrEmpty(_defaultConnection))
                throw new InvalidOperationException(
                    "Default connection can only be set once. You may want to use \'Get(string database, string connection)\'" +
                    $"if you want to get {nameof(DbContext)} with different database connection.");

            _defaultConnection = value;
        }
    }

    private string _defaultDatabase = string.Empty;

    public string DefaultDatabase
    {
        get
        {
            if (string.IsNullOrEmpty(_defaultDatabase))
                throw new ArgumentException("Default database is not set");

            return _defaultDatabase;
        }
        set
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invalid database value");

            if (!string.IsNullOrEmpty(_defaultDatabase))
                throw new InvalidOperationException("Default database can only be set once");

            _defaultDatabase = value;
        }
    }

    /// <summary>
    ///     Get <see cref="DbContext" /> object with default database and connection
    /// </summary>
    /// <param name="ignoreGlobalFilter">Indicate whether to ignore global filter</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Throws when no default database has been setup</exception>
    /// <exception cref="InvalidOperationException">Throws when no default connection has been setup</exception>
    public IDbContext Get(bool ignoreGlobalFilter = false) => Get(DefaultDatabase, ignoreGlobalFilter);

    /// <summary>
    ///     Get <see cref="DbContext" /> object with default database connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="ignoreGlobalFilter">Indicate whether to ignore global filter</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws when no default connection has been setup</exception>
    public IDbContext Get(string database, bool ignoreGlobalFilter = false)
    {
        if (string.IsNullOrEmpty(DefaultConnection))
            throw new InvalidOperationException("No default connection has been setup");

        if (!HasRegisteredConventions) RegisterConventions();

        var context = new DbContext(database, DefaultConnection, ignoreGlobalFilter, _globalFilter);
        DbContexts.Add(context);

        return context;
    }

    /// <summary>
    ///     Get <see cref="DbContext" /> object with custom connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="connection">The connection other than default connection</param>
    /// <param name="ignoreGlobalFilter">Indicate whether to ignore global filter</param>
    /// <returns></returns>
    public IDbContext Get(string database, string connection, bool ignoreGlobalFilter = false)
    {
        if (!HasRegisteredConventions) RegisterConventions();

        var context = new DbContext(database, connection, ignoreGlobalFilter, _globalFilter);
        DbContexts.Add(context);

        return context;
    }

    public List<IDbContext> DbContexts { get; }

    public void SetGlobalFilter<T>(string jsonString, Assembly assembly, bool prepend = false) =>
        _globalFilter.Set<T>(jsonString, assembly, prepend);

    public void SetGlobalFilter<T>(FilterDefinition<T> filter, bool prepend = false) where T : IEntity =>
        _globalFilter.Set(filter, prepend);

    public void SetGlobalFilter<T>(BsonDocument document, bool prepend = false) where T : IEntity =>
        _globalFilter.Set<T>(document, prepend);
}