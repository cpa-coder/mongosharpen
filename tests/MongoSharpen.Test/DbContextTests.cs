using FluentAssertions;
using MongoDB.Driver;
using MongoSharpen.Internal;
using MongoSharpen.Test.Entities;
using MongoSharpen.Test.Fixtures;
using Xunit;

namespace MongoSharpen.Test;

[CollectionDefinition("db-context")]
public class ContextCollection : ICollectionFixture<DbContextFixture>
{
}

[Xunit.Collection("db-context")]
public partial class DbContextTests
{
    [Fact]
    public async Task exist__should_return_valid_result()
    {
        var libDb = DbFactory.Get("library");
        var libraryExist = await libDb.ExistAsync();

        libraryExist.Should().BeTrue();

        var random = Guid.NewGuid().ToString();
        var randomDb = DbFactory.Get(random);
        var randomDbExist = await randomDb.ExistAsync();

        randomDbExist.Should().BeFalse();
    }

    [Fact]
    public void start_transaction__session_should_not_be_null()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        using var trans = ctx.Transaction();
        ctx.Session.Should().NotBeNull();
    }

    [Fact]
    public void start_transaction__when_called_more_than_once__should_throw_exception()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        using var trans = ctx.Transaction();

        Assert.Throws<InvalidOperationException>(() => ctx.Transaction());
    }

    [Fact]
    public async Task commit__when_no_transaction_started__should_throw_exception()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        using var trans = ctx.Transaction();
        ctx.Session = null;

        await Assert.ThrowsAsync<InvalidOperationException>(() => trans.CommitAsync());
    }
    
    [Fact]
    public async Task drop_database__should_delete_database()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);
        await ctx.Database.CreateCollectionAsync("test-collection");

        var initiallyExist = await ctx.ExistAsync();
        initiallyExist.Should().BeTrue();
    
        await ctx.DropDataBaseAsync();

        var finallyExist = await ctx.ExistAsync();
        finallyExist.Should().BeFalse();
    }
    
    [Fact]
    public void queryable__should_return_queryable_of_type_with_the_same_db_context()
    {
        var ctx = DbFactory.Get("library");
        var query = ctx.Queryable<Book>();

        var internalQueryable = Cache<Book>.GetCollection(ctx).AsQueryable(ctx.Session);
        query.Should().BeEquivalentTo(internalQueryable);
    }

    [Fact]
    public void collection__should_return_collection_of_type_with_the_same_db_context()
    {
        var ctx = DbFactory.Get("library");
        var collection = ctx.Collection<Book>();

        var internalCollection = Cache<Book>.GetCollection(ctx);
        collection.Should().BeEquivalentTo(internalCollection);
    }
}