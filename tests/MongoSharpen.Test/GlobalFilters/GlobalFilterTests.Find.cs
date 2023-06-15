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
    public async Task global_filter_json__on_find_and_execute__should_apply_global_filter()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

        var books = new List<Book>
        {
            new() { Deleted = true },
            new() { Deleted = false },
            new() { Deleted = false }
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Find<Book>().ExecuteAsync();

        result.Count.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_filter_definition__on_find_and_execute__should_apply_global_filter()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter(Builders<Book>.Filter.Eq(x => x.Deleted, false));

        var books = new List<Book>
        {
            new() { Deleted = true },
            new() { Deleted = false },
            new() { Deleted = false }
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Find<Book>().ExecuteAsync();

        result.Count.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_bson_document__on_find_and_execute__should_apply_global_filter()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter<Book>(new BsonDocument(new BsonElement("deleted", false)));

        var books = new List<Book>
        {
            new() { Deleted = true },
            new() { Deleted = false },
            new() { Deleted = false }
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Find<Book>().ExecuteAsync();

        result.Count.Should().Be(2);
    }

    [Fact]
    public async Task global_filter_json__on_find_and_execute_first__should_not_return_deleted()
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

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.Find<Book>().ExecuteFirstAsync());
    }

    [Fact]
    public async Task global_filter_json__on_find_and_execute_single__should_not_return_deleted()
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

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.Find<Book>().ExecuteSingleAsync());
    }

    [Fact]
    public async Task global_filter_json__on_find_and_execute_one__should_not_return_deleted()
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

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.Find<Book>().OneAsync(book.Id));
    }

    [Fact]
    public async Task global_filter_json__on_find_and_execute_many_func__should_not_return_deleted()
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

        var result = await context.Find<Book>().ManyAsync(x => x.MatchId(book.Id));

        result.Count.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_json__on_find_and_execute_many_expression__should_not_return_deleted()
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

        var result = await context.Find<Book>().ManyAsync(x => !x.Deleted);

        result.Count.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute__should_apply_global_filter()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

        var books = new List<Book>
        {
            new() { Deleted = true },
            new() { Deleted = false },
            new() { Deleted = false }
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ExecuteAsync();

        result.Count.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_filter_definition__on_find_with_projection_and_execute__should_apply_global_filter()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter(Builders<Book>.Filter.Eq(x => x.Deleted, false));

        var books = new List<Book>
        {
            new() { Deleted = true },
            new() { Deleted = false },
            new() { Deleted = false }
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ExecuteAsync();

        result.Count.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_bson_document__on_find_with_projection_and_execute__should_apply_global_filter()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter<Book>(new BsonDocument(new BsonElement("deleted", false)));

        var books = new List<Book>
        {
            new() { Deleted = true },
            new() { Deleted = false },
            new() { Deleted = false }
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ExecuteAsync();

        result.Count.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute_first__should_not_return_deleted()
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

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ExecuteFirstOrDefaultAsync();
        result.Should().BeNull();
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute_single__should_not_return_deleted()
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

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ExecuteSingleOrDefaultAsync();
        result.Should().BeNull();
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute_one__should_not_return_deleted()
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

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).OneOrDefaultAsync(book.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute_many_func__should_not_return_deleted()
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

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ManyAsync(x => x.MatchId(book.Id));

        result.Count.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute_many_expression__should_not_return_deleted()
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

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ManyAsync(x => !x.Deleted);

        result.Count.Should().Be(0);
    }
}