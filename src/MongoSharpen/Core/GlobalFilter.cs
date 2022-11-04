using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoSharpen;

internal sealed class GlobalFilter
{
    private Type[]? _entityTypes;
    private readonly Dictionary<Type, (object filterDef, bool prepend)> _filters = new();

    private static Type[] GetAllEntityTypes()
    {
        var excludes = new[]
        {
            "Microsoft.",
            "System.",
            "MongoDB.",
            "testhost.",
            "netstandard",
            "Newtonsoft.",
            "mscorlib",
            "NuGet."
        };

        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !excludes.Any(n => a.FullName.StartsWith(n)))
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IEntity).IsAssignableFrom(t))
            .ToArray();
    }

    private void AddFilter(Type type, (object filterDef, bool prepend) filter)
    {
        _filters.TryAdd(type, filter);
    }

    internal void Set<T>(string jsonString, bool prepend = false)
    {
        var targetType = typeof(T);
        if (!targetType.IsInterface) throw new ArgumentException("Only interfaces are allowed", nameof(T));

        _entityTypes ??= GetAllEntityTypes();

        foreach (var entType in _entityTypes.Where(t => targetType.IsAssignableFrom(t)))
            AddFilter(entType, (jsonString, prepend));
    }

    internal void Set<T>(FilterDefinition<T> filter, bool prepend = false)where T : IEntity
    {
        AddFilter(typeof(T), (filter, prepend));
    }

    internal void Set<T>(BsonDocument document, bool prepend = false)
    {
        AddFilter(typeof(T), (document, prepend));
    }

    internal FilterDefinition<T> Merge<T>(FilterDefinition<T> filter)
    {
        if (_filters.Count <= 0) return filter;

        if (!_filters.TryGetValue(typeof(T), out var globalFilter)) return filter;

        return globalFilter.filterDef switch
        {
            FilterDefinition<T> definition => globalFilter.prepend ? definition & filter : filter & definition,
            BsonDocument bsonDoc => globalFilter.prepend ? bsonDoc & filter : filter & bsonDoc,
            string jsonString => globalFilter.prepend ? jsonString & filter : filter & jsonString,
            _ => filter
        };
    }
}