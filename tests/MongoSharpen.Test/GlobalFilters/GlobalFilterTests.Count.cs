using System.Reflection;
using Bogus;
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
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

        var faker = new Faker();
        var book = new Book
        {
            Title = $"Title-{faker.Commerce.Department()}",
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var count = await context.CountEstimatedAsync<Book>();
        await context.DropDataBaseAsync();

        count.Should().Be(1);
    }

    [Fact]
    public async Task global_filter_definition__on_count__should_ignore_global_filter()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter(Builders<Book>.Filter.Eq(x => x.Deleted, false));

        var faker = new Faker();
        var books = new List<Book>();
        for (var i = 1; i <= 10; i++)
        {
            var oddOrEven = i % 2 != 0 ? "odd" : "even";
            var book = new Book
            {
                Title = $"{oddOrEven}-{faker.Commerce.Department()}",
                ISBN = faker.Vehicle.Model(),
                Deleted = faker.Random.Bool()
            };
            books.Add(book);
        }

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var count = await context.CountAsync<Book>();
        await context.DropDataBaseAsync();

        count.Should().Be(books.Count);
    }

    [Fact]
    public async Task global_filter_definition__on_count_with_filter_expression__should_not_count_filtered_item()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<Book>(new BsonDocument(new BsonElement("deleted", false)));

        var faker = new Faker();
        var book = new Book
        {
            Title = $"Title-{faker.Commerce.Department()}",
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var count = await context.CountAsync<Book>(x => x.Match(i => i.Title.Contains("-")));
        await context.DropDataBaseAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_definition__on_count_with_filter_definition__should_not_count_filtered_item()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

        var faker = new Faker();
        var book = new Book
        {
            Title = $"Title-{faker.Commerce.Department()}",
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var count = await context.CountAsync<Book>(x => x.MatchId(book.Id));
        await context.DropDataBaseAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_definition__on_count_with_filter_function__should_not_count_filtered_item()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

        var faker = new Faker();
        var book = new Book
        {
            Title = $"Title-{faker.Commerce.Department()}",
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Delete<Book, BookDto>(x => x.MatchId(book.Id))
                .Project(x => new BookDto())
                .GetAndExecuteAsync());

        await context.DropDataBaseAsync();
    }
}