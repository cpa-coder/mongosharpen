using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using MongoSharpen.Test.Dtos;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test;

public sealed partial class GlobalFilterTests
{
    [Fact]
    public async Task global_filter_json__on_update_and_execute__should_not_update_filtered_item()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

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

        var result = await context.Update<Book>(x => x.Match(i => i.Title.Contains("-")))
            .Modify(x => x.Set(i => i.Title, $"edited-{faker.Commerce.Department()}"))
            .ExecuteAsync();
        await context.DropDataBaseAsync();

        result.ModifiedCount.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_filter_definition__on_update_and_execute__should_not_update_filtered_item()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter(MongoDB.Driver.Builders<Book>.Filter.Eq(x => x.Deleted, false));

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

        var result = await context.Update<Book>(x => x.Match(i => i.Title.Contains("-")))
            .Modify(x => x.Set(i => i.Title, $"edited-{faker.Commerce.Department()}"))
            .ExecuteAsync();
        await context.DropDataBaseAsync();

        result.ModifiedCount.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_bson_document__on_update_and_execute__should_not_update_filtered_item()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<Book>(new BsonDocument(new BsonElement("deleted", false)));

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

        var result = await context.Update<Book>(x => x.Match(i => i.Title.Contains("-")))
            .Modify(x => x.Set(i => i.Title, $"edited-{faker.Commerce.Department()}"))
            .ExecuteAsync();
        await context.DropDataBaseAsync();

        result.ModifiedCount.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_filter_json__on_update_execute_and_get_with_projection_and_execute__should_not_update_filtered_item()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

        var faker = new Faker();
        var book = new Book
        {
            Title = $"Title-{faker.Commerce.Department()}",
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var result = await context.Update<Book, BookDto>(x => x.MatchId(book.Id))
            .Modify(x => x.Set(i => i.Title, $"edited-{faker.Commerce.Department()}"))
            .Project(x => new BookDto())
            .ExecuteAndGetAsync();
        await context.DropDataBaseAsync();

        result.Should().BeNull();
    }
}