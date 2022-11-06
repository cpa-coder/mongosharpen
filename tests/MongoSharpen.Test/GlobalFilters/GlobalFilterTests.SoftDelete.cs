using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using MongoSharpen.Test.Dtos;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.GlobalFilters;

public sealed partial class GlobalFilterTests
{
    [Fact]
    public async Task global_filter_json__on_soft_delete_and_execute_one__should_not_delete_filtered_item()
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

        var result = await context.SoftDelete<Book>(x => x.Match(i => i.Title.Contains("-")))
            .ExecuteOneAsync(ObjectId.GenerateNewId().ToString());
        await context.DropDataBaseAsync();

        result.DeletedCount.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_definition__on_soft_delete_and_execute_many__should_not_delete_filtered_item()
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

        var result = await context.SoftDelete<Book>(x => x.Match(i => i.Title.Contains("-")))
            .ExecuteManyAsync(ObjectId.GenerateNewId().ToString());
        await context.DropDataBaseAsync();

        result.DeletedCount.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_bson_document__on_soft_delete_and_get_and_execute__should_not_delete_filtered_item()
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

        var result = await context.SoftDelete<Book>(x => x.Match(i => i.Title.Contains("-"))).ExecuteAndGetAsync(book.Id);
        await context.DropDataBaseAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task
        global_filter_json__on_soft_delete_execute_and_get_with_projection_and_execute__should_not_delete_filtered_item()
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

        var result = await context.SoftDelete<Book, BookDto>(x => x.MatchId(book.Id))
            .Project(x => new BookDto())
            .ExecuteAndGetAsync(ObjectId.GenerateNewId().ToString());

        result.Should().BeNull();
        await context.DropDataBaseAsync();
    }
}