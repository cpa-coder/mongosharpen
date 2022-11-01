using MongoDB.Bson.Serialization.Conventions;

namespace MongoSharpen;

internal sealed class DbFactoryInternal
{
    private readonly IConventionRegistryWrapper _registryWrapper;

    /// <summary>
    ///     Instantiate DbFactory for internal testing
    /// </summary>
    internal DbFactoryInternal(IConventionRegistryWrapper registryWrapper)
    {
        _registryWrapper = registryWrapper;
        _contexts = new Dictionary<string, IDbContext>();
        _conventions = new Dictionary<string, ConventionPack>
        {
            { "camelCase", new ConventionPack { new CamelCaseElementNameConvention() } }
        };
    }

    private bool _conventionsRegistered;
    private readonly Dictionary<string, IDbContext> _contexts;
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

    /// <summary>
    ///     Get <see cref="DbContext" /> object with default database connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws when no default connection has been setup</exception>
    public IDbContext Get(string database)
    {
        if (string.IsNullOrEmpty(DefaultConnection))
            throw new InvalidOperationException("No default connection has been setup");

        if (!_conventionsRegistered) RegisterConventions();

        var key = $"{database}@{DefaultConnection}";
        if (_contexts.ContainsKey(key))
            return _contexts[key];

        var context = new DbContext(database, DefaultConnection);
        _contexts.TryAdd(key, context);

        return context;
    }

    /// <summary>
    ///     Get <see cref="DbContext" /> object with custom connection
    /// </summary>
    /// <param name="database">The database name</param>
    /// <param name="connection">The connection other than default connection</param>
    /// <returns></returns>
    public IDbContext Get(string database, string connection)
    {
        if (!_conventionsRegistered) RegisterConventions();

        var key = $"{database}@{connection}";
        if (_contexts.ContainsKey(key))
            return _contexts[key];

        var context = new DbContext(database, connection);
        _contexts.TryAdd(key, context);

        return context;
    }

    public List<IDbContext> DbContexts => _contexts.Values.ToList();
}