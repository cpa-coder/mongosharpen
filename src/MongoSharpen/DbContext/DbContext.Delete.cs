using MongoDB.Driver;
using MongoSharpen.Builders;

namespace MongoSharpen;

public sealed partial class DbContext
{
    public Delete<T> Delete<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new(expression) { Context = this };

    public Delete<T, TProjection> Delete<T, TProjection>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression)
        where T : IEntity => new(expression) { Context = this };

    public async Task<DeleteResult> SoftDeleteAsync<T>(string id, string userId) where T : IEntity, ISoftDelete
    {
        var hasSystemGenerated = (await Find<T>(x => x.MatchId(id)).ExecuteAsync()).Any(x => x.SystemGenerated);
        if (hasSystemGenerated) throw new InvalidOperationException("System generated records cannot be deleted");

        var result = await Update<T>(x => x.MatchId(id))
            .Modify(x => x
                .Set(t => t.Deleted, true)
                .Set(t => t.DeletedBy, new DeletedBy { Id = userId })
                .Set(t => t.DeletedOn, DateTime.UtcNow))
            .ExecuteAsync();
        return new DeleteResult.Acknowledged(result.ModifiedCount);
    }

    public async Task<DeleteResult> SoftDeleteAsync<T>(Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> expression,
        string userId) where T : IEntity, ISoftDelete
    {
        var hasSystemGenerated = (await Find(expression).ExecuteAsync()).Any(x => x.SystemGenerated);
        if (hasSystemGenerated) throw new InvalidOperationException("System generated records cannot be deleted");

        var result = await Update(expression)
            .Modify(x => x
                .Set(t => t.Deleted, true)
                .Set(t => t.DeletedBy, new DeletedBy { Id = userId })
                .Set(t => t.DeletedOn, DateTime.UtcNow))
            .ExecuteAsync();
        return new DeleteResult.Acknowledged(result.ModifiedCount);
    }
}