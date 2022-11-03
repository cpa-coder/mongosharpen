using FluentAssertions;
using MongoDB.Bson;
using MongoSharpen.Test.Dtos;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test;

public partial class DbContextTests
{
    [Fact]
    public async Task soft_delete__on_execute_many__should_mark_matched_items_as_deleted()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooks();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        await ctx.SoftDelete<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteManyAsync(userId);

        var found = await ctx.Find<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteAsync();
        found.Should().NotBeEmpty();
        foreach (var i in found)
        {
            i.Deleted.Should().BeTrue();
            i.DeletedBy.Id.Should().Be(userId);
            i.DeletedOn.Should().BeOnOrBefore(DateTime.UtcNow);
        }
    }

    [Fact]
    public async Task soft_delete_with_system_generated__on_execute_many__should_mark_non_system_generated_items_as_deleted()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        await ctx.SoftDelete<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteManyAsync(userId);

        var found = await ctx.Find<Book>(x => x
                .Match(i => i.Title.Contains("odd"))
                .Match(i => i.Deleted))
            .ExecuteAsync();

        found.Count.Should().Be(books.Count(x => !x.SystemGenerated && x.Title.Contains("odd")));
    }

    [Fact]
    public async Task soft_delete_with_system_generated__on_execute_many_by_force__should_mark_all_matched_items_as_deleted()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        await ctx.SoftDelete<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteManyAsync(userId, forceDelete: true);

        var found = await ctx.Find<Book>(x => x
                .Match(i => i.Title.Contains("odd"))
                .Match(i => i.Deleted))
            .ExecuteAsync();

        found.Count.Should().Be(books.Count(x => x.Title.Contains("odd")));
    }

    [Fact]
    public async Task soft_delete__on_execute_one__should_mark_single_matched_item_as_deleted()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooks();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        await ctx.SoftDelete<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteOneAsync(userId);

        var found = await ctx.Find<Book>(x => x.MatchId(books[0].Id)).ExecuteFirstAsync();
        found.Deleted.Should().BeTrue();
    }

    [Fact]
    public async Task soft_delete__on_execute_one__when_system_generated__should_not_mark_item_as_deleted()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        var systemGeneratedItem = books.First(i => i.SystemGenerated);
        await ctx.SoftDelete<Book>(x => x.Match(i => i.Id == systemGeneratedItem.Id)).ExecuteOneAsync(userId);

        var found = await ctx.Find<Book>().OneAsync(systemGeneratedItem.Id);
        found.Deleted.Should().BeFalse();
    }

    [Fact]
    public async Task soft_delete__on_execute_one_by_force__when_system_generated__should_mark_item_as_deleted()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        var systemGeneratedItem = books.First(i => i.SystemGenerated);
        await ctx.SoftDelete<Book>(x => x.Match(i => i.Id == systemGeneratedItem.Id)).ExecuteOneAsync(userId, forceDelete: true);

        var found = await ctx.Find<Book>().OneAsync(systemGeneratedItem.Id);
        found.Deleted.Should().BeTrue();
    }

    [Fact]
    public async Task soft_delete__on_execute_and_get__should_get_matched_item_in_db_after_marking_as_deleted()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooks();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        var book = await ctx.SoftDelete<Book>(x => x.MatchId(books[0].Id)).ExecuteAndGetAsync(userId);

        book.Id.Should().Be(books[0].Id);

        var found = await ctx.Find<Book>(x => x.MatchId(books[0].Id)).ExecuteFirstAsync();
        found.Deleted.Should().BeTrue();
    }

    [Fact]
    public async Task soft_delete__on_execute_and_get__when_system_generated__should_return_null()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        var systemGeneratedItem = books.First(i => i.SystemGenerated);
        var book = await ctx.SoftDelete<Book>(x => x.MatchId(systemGeneratedItem.Id)).ExecuteAndGetAsync(userId);

        book.Should().BeNull();
    }

    [Fact]
    public async Task soft_delete__on_execute_and_get_by_force__when_system_generated__should_return_marked_as_deleted_item()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        var systemGeneratedItem = books.First(i => i.SystemGenerated);
        var book = await ctx.SoftDelete<Book>(x => x.MatchId(systemGeneratedItem.Id)).ExecuteAndGetAsync(userId, forceDelete: true);

        book.Should().NotBeNull();
        book.Deleted.Should().BeTrue();
    }

    [Fact]
    public async Task soft_delete_with_projection__on_execute_and_get__should_get_matched_item_in_db()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooks();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        var book = await ctx.SoftDelete<Book, BookDto>(x => x.MatchId(books[0].Id))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN
            }).ExecuteAndGetAsync(userId);

        book.Id.Should().Be(books[0].Id);
        book.Title.Should().Be(books[0].Title);
        book.ISBN.Should().Be(books[0].ISBN);
    }

    [Fact]
    public async Task soft_delete_with_projection__on_execute_and_get__when_system_generated__should_return_null()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        var systemGeneratedItem = books.First(i => i.SystemGenerated);
        var book = await ctx.SoftDelete<Book, BookDto>(x => x.MatchId(systemGeneratedItem.Id))
            .Project(x => new BookDto())
            .ExecuteAndGetAsync(userId);

        book.Should().BeNull();
    }

    [Fact]
    public async Task soft_delete_with_projection__on_execute_and_get_by_force__when_system_generated__should_deleted_item()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var userId = ObjectId.GenerateNewId().ToString();
        var systemGeneratedItem = books.First(i => i.SystemGenerated);
        var book = await ctx.SoftDelete<Book, BookDto>(x => x
                .MatchId(systemGeneratedItem.Id))
            .Project(x => new BookDto { Id = x.Id })
            .ExecuteAndGetAsync(userId, forceDelete: true);

        book.Should().NotBeNull();
        book.Id.Should().Be(systemGeneratedItem.Id);
    }

    [Fact]
    public async Task soft_delete_with_projection__when_no_projection_setup__should_throw_exception()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var userId = ObjectId.GenerateNewId().ToString();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ctx.SoftDelete<Book, BookDto>(x => x.MatchId(userId)).ExecuteAndGetAsync(userId));
    }
}