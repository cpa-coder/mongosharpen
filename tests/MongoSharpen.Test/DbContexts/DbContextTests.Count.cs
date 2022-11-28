using FluentAssertions;
using MongoDB.Driver;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.DbContexts;

public partial class DbContextTests
{
    [Fact]
    public async Task count_estimated__should_return_count_not_less_than_the_actual()
    {
        var ctx = DbFactory.Get("library");
        var count = await ctx.CountEstimatedAsync<Book>();

        count.Should().BeLessOrEqualTo(_bookFixture.Books.Count);
    }

    [Fact]
    public async Task count__should_return_the_actual_count()
    {
        var ctx = DbFactory.Get("library");
        var count = await ctx.CountAsync<Book>();

        count.Should().Be(_bookFixture.Books.Count);
    }

    [Fact]
    public async Task count__with_filter_expression__should_return_the_filtered_count()
    {
        var ctx = DbFactory.Get("library");

        var book = _bookFixture.Books.First();
        var count = await ctx.CountAsync<Book>(x => x.Id == book.Id);

        count.Should().Be(1);
    }

    [Fact]
    public async Task count__with_filter_definition__should_return_the_filtered_count()
    {
        var ctx = DbFactory.Get("library");

        var book = _bookFixture.Books.First();
        var count = await ctx.CountAsync(Builders<Book>.Filter.Eq(x => x.Id, book.Id));

        count.Should().Be(1);
    }

    [Fact]
    public async Task count__with_filter_function__should_return_the_filtered_count()
    {
        var ctx = DbFactory.Get("library");

        var book = _bookFixture.Books.First();
        var count = await ctx.CountAsync<Book>(x => x.MatchId(book.Id));

        count.Should().Be(1);
    }
}