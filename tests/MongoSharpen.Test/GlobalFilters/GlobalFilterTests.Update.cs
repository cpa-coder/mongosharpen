using System.Reflection;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoSharpen.Test.Dtos;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.GlobalFilters;

public sealed partial class GlobalFilterTests
{
    [Fact]
    public async Task global_filter_json__on_update_and_execute__should_not_update_filtered_item()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

        var books = new List<Book>
        {
            new() { Title = "Book-1", Deleted = true },
            new() { Title = "Book-2", Deleted = false },
            new() { Title = "Book-3", Deleted = false }
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Update<Book>(x => x.Match(i => i.Title.Contains("-")))
            .Modify(x => x.Set(i => i.Title, "edited-edited-book-1"))
            .ExecuteAsync();

        result.ModifiedCount.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_filter_definition__on_update_and_execute__should_not_update_filtered_item()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter(Builders<Book>.Filter.Eq(x => x.Deleted, false));

        var books = new List<Book>
        {
            new() { Title = "Book-1", Deleted = true },
            new() { Title = "Book-2", Deleted = false },
            new() { Title = "Book-3", Deleted = false }
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var updateDefinitions = new List<UpdateDefinition<Book>>().Set(x => x.Title, "edited-book-1");

        var result = await context.Update<Book>(x => x.Match(i => i.Title.Contains("-")))
            .Modify(updateDefinitions)
            .ExecuteAsync();

        result.ModifiedCount.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_bson_document__on_update_and_execute__should_not_update_filtered_item()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter<Book>(new BsonDocument(new BsonElement("deleted", false)));

        var books = new List<Book>
        {
            new() { Title = "Book-1", Deleted = true },
            new() { Title = "Book-2", Deleted = false },
            new() { Title = "Book-3", Deleted = false }
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Update<Book>(x => x.Match(i => i.Title.Contains("-")))
            .Modify(x => x.Set(i => i.Title, "edited-book-1"))
            .ExecuteAsync();

        result.ModifiedCount.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_filter_json__on_update_execute_and_get_with_projection_and_execute__should_not_update_filtered_item()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

        var book = new Book
        {
            Title = "Title-Book",
            ISBN = "123123",
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var result = await context.Update<Book, BookDto>(x => x.MatchId(book.Id))
            .Modify(x => x.Set(i => i.Title, "edited-title-book"))
            .Project(x => new BookDto())
            .ExecuteAndGetAsync();

        result.Should().BeNull();
    }
}