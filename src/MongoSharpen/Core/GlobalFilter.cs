using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoSharpen;

internal sealed class GlobalFilter
{
    private readonly List<Type> _entityTypes;
    private readonly Dictionary<Type, (object filterDef, bool prepend)> _filters = new();
    private readonly List<string> _assemblies;

    public GlobalFilter()
    {
        _assemblies = new List<string>();
        _entityTypes = new List<Type>();
    }

    private void AddFilter(Type type, (object filterDef, bool prepend) filter)
    {
        _filters.TryAdd(type, filter);
    }

    private void RegisterAssembly(Assembly assembly)
    {
        if (assembly.IsDynamic) return;

        var types = assembly.GetTypes();
        foreach (var type in types)
            if (typeof(IEntity).IsAssignableFrom(type))
                _entityTypes.Add(type);

        if (assembly.FullName != null) _assemblies.Add(assembly.FullName);
    }

    internal void Set<T>(string jsonString, Assembly assembly, bool prepend = false)
    {
        var targetType = typeof(T);
        if (!targetType.IsInterface) throw new ArgumentException("Only interfaces are allowed", nameof(T));

        if (_assemblies.All(t => t != assembly.FullName)) RegisterAssembly(assembly);

        foreach (var entType in _entityTypes.Where(t => targetType.IsAssignableFrom(t)))
            AddFilter(entType, (jsonString, prepend));
    }

    internal void Set<T>(FilterDefinition<T> filter, bool prepend = false) where T : IEntity
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