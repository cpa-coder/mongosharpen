using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoSharpen.Internal;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.DbContexts;

public partial class DbContextTests
{
    private static async Task<BsonElement> GetBsonElement<T>(IDbContext ctx, string id)
    {
        var coll = Cache<T>.GetCollection<BsonDocument>(ctx, "log");

        var filter = Builders<BsonDocument>.Filter.Empty;
        filter &= "{old_id:" + $"ObjectId(\'{id}\')" + "}";
        var cursor = await coll.FindAsync<BsonDocument>(filter);

        var list = new List<BsonDocument>();
        while (await cursor.MoveNextAsync().ConfigureAwait(false)) list.AddRange(cursor.Current);
        return list.First().Elements.First(x => x.Name == "old_id");
    }

    private static async Task<int> GetBsonElementLogCount<T>(IDbContext ctx)
    {
        var coll = Cache<T>.GetCollection<BsonDocument>(ctx, "log");

        var filter = Builders<BsonDocument>.Filter.Empty;
        var cursor = await coll.FindAsync<BsonDocument>(filter);

        var list = new List<BsonDocument>();
        while (await cursor.MoveNextAsync().ConfigureAwait(false)) list.AddRange(cursor.Current);
        return list.Count;
    }

    [Fact]
    public async Task log_by_id__should_save_log_in_db()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);

        var book = _bookFixture.Books[0];
        await ctx.SaveAsync(book);

        await ctx.LogAsync<Book>(book.Id);

        var element = await GetBsonElement<Book>(ctx, book.Id);
        var id = element.Value.ToString();

        id.Should().Be(book.Id);
    }

    [Fact]
    public async Task log_by_entity__should_save_log_in_db()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);

        var book = _bookFixture.Books[0];
        await ctx.SaveAsync(book);

        await ctx.LogAsync(book);

        var element = await GetBsonElement<Book>(ctx, book.Id);
        var id = element.Value.ToString();

        id.Should().Be(book.Id);
    }

    [Fact]
    public async Task log_by_entity_using_transaction__should_save_log_in_db()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);

        using var trans = ctx.Transaction();

        var book = _bookFixture.Books[0];
        await ctx.SaveAsync(book);

        await ctx.LogAsync(book);

        await trans.CommitAsync();

        var element = await GetBsonElement<Book>(ctx, book.Id);
        var id = element.Value.ToString();

        id.Should().Be(book.Id);
    }

    [Fact]
    public async Task log_by_expression__should_save_log_in_db()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);

        await ctx.SaveAsync(_bookFixture.Books);

        await ctx.LogAsync<Book>(x => x.Match(i => i.Title.Contains("-")));

        var count = await GetBsonElementLogCount<Book>(ctx);

        count.Should().Be(_bookFixture.Books.Count);
    }

    [Fact]
    public async Task log_by_entities__should_save_log_in_db()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);

        await ctx.SaveAsync(_bookFixture.Books);

        await ctx.LogAsync(_bookFixture.Books);

        var count = await GetBsonElementLogCount<Book>(ctx);

        count.Should().Be(_bookFixture.Books.Count);
    }

    [Fact]
    public async Task log_by_entities_using_transaction__should_save_log_in_db()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        using var trans = ctx.Transaction();

        await ctx.SaveAsync(_bookFixture.Books);

        await ctx.LogAsync(_bookFixture.Books);

        await trans.CommitAsync();

        var count = await GetBsonElementLogCount<Book>(ctx);

        count.Should().Be(_bookFixture.Books.Count);
    }
}