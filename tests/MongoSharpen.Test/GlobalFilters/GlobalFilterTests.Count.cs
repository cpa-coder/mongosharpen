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
    public async Task global_filter_json__on_count_estimated__should_ignore_global_filter()
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

        var count = await context.CountEstimatedAsync<Book>();

        count.Should().Be(1);
    }

    [Fact]
    public async Task global_filter_definition__on_count__should_ignore_global_filter()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter(Builders<Book>.Filter.Eq(x => x.Deleted, false));

        var books = new List<Book>
        {
            new()
            {
                Title = "Title-Book1",
                ISBN = "123123",
                Deleted = false
            },
            new()
            {
                Title = "Title-Book2",
                ISBN = "123123",
                Deleted = true
            }
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var count = await context.CountAsync<Book>();

        count.Should().Be(books.Count);
    }

    [Fact]
    public async Task global_filter_definition__on_count_with_filter_expression__should_not_count_filtered_item()
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

        var count = await context.CountAsync<Book>(x => x.Match(i => i.Title.Contains("-")));

        count.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_definition__on_count_with_filter_definition__should_not_count_filtered_item()
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

        var count = await context.CountAsync<Book>(x => x.MatchId(book.Id));

        count.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_definition__on_count_with_filter_function__should_not_count_filtered_item()
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

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Delete<Book, BookDto>(x => x.MatchId(book.Id))
                .Project(x => new BookDto())
                .GetAndExecuteAsync());
    }
}