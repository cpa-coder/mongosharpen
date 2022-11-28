using Xunit;

namespace MongoSharpen.Test.Fixtures;

public class DbContextFixture : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        DbFactory.DefaultConnection = "mongodb://localhost:30060";
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var ctx in DbFactory.DbContexts) await ctx.DropDataBaseAsync();
    }
}