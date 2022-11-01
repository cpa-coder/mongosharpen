using Xunit;

namespace MongoSharpen.Test.Fixtures;

public class DbContextFixture : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        DbFactory.DefaultConnection = conn;
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var ctx in DbFactory.DbContexts)
        {
            await ctx.DropDataBaseAsync();
        }
    }
}