using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace MongoSharpen;

internal sealed class DbFactoryInternal
{
    private readonly GlobalFilter _globalFilter;
    private readonly IConventionRegistryWrapper _registryWrapper;

    /// <summary>
    ///     Instantiate DbFactory for internal testing
    /// </summary>
    internal DbFactoryInternal(IConventionRegistryWrapper registryWrapper)
    {
        _registryWrapper = registryWrapper;
        DbContexts = new List<IDbContext>();
        _conventions = new Dictionary<string, ConventionPack>
        {
            { "camelCase", new ConventionPack { new CamelCaseElementNameConvention() } }
        };
        _globalFilter = new GlobalFilter();
    }

    private bool _conventionsRegistered;
    private readonly Dictionary<string, ConventionPack> _conventions;

    public void AddConvention(string name, ConventionPack pack)
    {
        if (_conventionsRegistered)
            throw new InvalidOperationException(
                "All conventions are already registered. Make sure to add or remove convention pack " +
                $"before getting any {nameof(DbContext)} in the {nameof(DbFactory)}.");

        _conventions.TryAdd(name, pack);
    }

    public void RemoveConvention(string name)
    {
        if (_conventionsRegistered)
            throw new InvalidOperationException(
                "All conventions are already registered. Make sure to add or remove convention pack " +
                $"before getting any {nameof(DbContext)} in the {nameof(DbFactory)}.");

        _conventions.Remove(name);
    }

    private void RegisterConventions()
    {
        foreach (var convention in _conventions) _registryWrapper.Register(convention.Key, convention.Value);

        _conventionsRegistered = true;
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

        if (!_conventionsRegistered) RegisterConventions();

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
        if (!_conventionsRegistered) RegisterConventions();

        var context = new DbContext(database, connection, ignoreGlobalFilter, _globalFilter);
        DbContexts.Add(context);

        return context;
    }

    public List<IDbContext> DbContexts { get; }

    internal void SetGlobalFilter<T>(string jsonString, Assembly assembly, bool prepend = false) =>
        _globalFilter.Set<T>(jsonString, assembly, prepend);

    internal void SetGlobalFilter<T>(FilterDefinition<T> filter, bool prepend = false) where T : IEntity =>
        _globalFilter.Set(filter, prepend);

    internal void SetGlobalFilter<T>(BsonDocument document, bool prepend = false) where T : IEntity =>
        _globalFilter.Set<T>(document, prepend);
}