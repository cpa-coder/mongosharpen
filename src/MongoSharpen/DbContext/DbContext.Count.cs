﻿using System.Linq.Expressions;
using MongoDB.Driver;
using MongoSharpen.Internal;

namespace MongoSharpen;

internal sealed partial class DbContext
{
    public Task<long> CountEstimatedAsync<T>(CancellationToken token = default) where T : IEntity =>
        Cache<T>.GetCollection(this).EstimatedDocumentCountAsync(null, token);

    public Task<long> CountAsync<T>(CancellationToken token = default) where T : IEntity =>
        GetCountInternalAsync(Builders<T>.Filter.Empty, null, token);

    private Task<long> GetCountInternalAsync<T>(FilterDefinition<T> filter, CountOptions? options, CancellationToken token)
        where T : IEntity =>
        _session == null
            ? Cache<T>.GetCollection(this).CountDocumentsAsync(filter, options, token)
            : Cache<T>.GetCollection(this).CountDocumentsAsync(_session, filter, options, token);

    public Task<long> CountAsync<T>(Expression<Func<T, bool>> expression, CountOptions? options = null,
        CancellationToken token = default) where T : IEntity
    {
        var filter = MergeFilterInternal<T>(expression);
        return GetCountInternalAsync(filter, options, token);
    }

    public Task<long> CountAsync<T>(FilterDefinition<T> filter, CountOptions? options = null,
        CancellationToken token = default) where T : IEntity
    {
        var filterLocal = MergeFilterInternal(filter);
        return GetCountInternalAsync(filterLocal, options, token);

    }

    public Task<long> CountAsync<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> filter,
        CountOptions? options = null, CancellationToken token = default) where T : IEntity
    {
        var filterLocal = MergeFilterInternal(filter(Builders<T>.Filter));
        return GetCountInternalAsync(filterLocal, options, token);
    }
}