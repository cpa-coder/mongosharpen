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
    public async Task global_filter_json__on_soft_delete_and_execute_one__should_not_delete_filtered_item()
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

        var result = await context.SoftDelete<Book>(x => x.Match(i => i.Title.Contains("-")))
            .ExecuteOneAsync(ObjectId.GenerateNewId().ToString());

        result.DeletedCount.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_definition__on_soft_delete_and_execute_many__should_not_delete_filtered_item()
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

        var result = await context.SoftDelete<Book>(x => x.Match(i => i.Title.Contains("-")))
            .ExecuteManyAsync(ObjectId.GenerateNewId().ToString());

        result.DeletedCount.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_bson_document__on_soft_delete_and_get_and_execute__should_not_delete_filtered_item()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter<Book>(new BsonDocument(new BsonElement("deleted", false)));

        var book = new Book
        {
            Title = "Title-Book",
            ISBN = "123123",
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var result = await context.SoftDelete<Book>(x => x.Match(i => i.Title.Contains("-"))).ExecuteAndGetAsync(book.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task global_filter_json__on_soft_delete_execute_and_get_with_projection__should_not_delete_filtered_item()
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

        var result = await context.SoftDelete<Book, BookDto>(x => x.MatchId(book.Id))
            .Project(x => new BookDto())
            .ExecuteAndGetAsync(ObjectId.GenerateNewId().ToString());

        result.Should().BeNull();
    }
}