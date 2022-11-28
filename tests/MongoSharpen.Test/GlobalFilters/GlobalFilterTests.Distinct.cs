using System.Reflection;
using Bogus;
using FluentAssertions;
using MongoDB.Driver;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.GlobalFilters;

public sealed partial class GlobalFilterTests
{
    [Fact]
    public async Task global_filter_json__on_distinct_without_global_filter__should_return_all_distinct_property_values()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        var faker = new Faker();
        var books = new List<Book>();
        for (var i = 0; i < 10; i++)
        {
            var num = faker.Random.Number(0, 3);
            books.Add(new Book
            {
                Title = $"Book{num}",
                Deleted = num != 3
            });
        }

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Distinct<Book, string>().Property(nameof(Book.Title)).ExecuteAsync();
        await context.DropDataBaseAsync();

        result.Count.Should().Be(books.DistinctBy(x => x.Title).Count());
    }

    [Fact]
    public async Task global_filter_json__on_distinct_with_global_filter__should_return_filtered_distinct_property_values()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

        var faker = new Faker();
        var books = new List<Book>();
        for (var i = 0; i < 10; i++)
        {
            var num = faker.Random.Number(0, 3);
            books.Add(new Book
            {
                Title = $"Book{num}",
                Deleted = num != 3
            });
        }

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Distinct<Book, string>().Property(nameof(Book.Title)).ExecuteAsync();
        await context.DropDataBaseAsync();

        result.Count.Should().Be(books.Where(t => !t.Deleted).DistinctBy(x => x.Title).Count());
    }

    [Fact]
    public async Task
        global_filter_json__on_distinct_with_specific_and_global_filters__should_return_filtered_distinct_property_values()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

        var faker = new Faker();
        var books = new List<Book>();
        for (var i = 0; i < 10; i++)
        {
            var num = faker.Random.Number(0, 3);
            books.Add(new Book
            {
                Title = $"Book{num}",
                Deleted = num != 3
            });
        }

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Distinct<Book, string>(Builders<Book>.Filter.Match(t => t.Deleted))
            .Property(nameof(Book.Title)).ExecuteAsync();
        await context.DropDataBaseAsync();

        result.Count.Should().Be(0);
    }
}