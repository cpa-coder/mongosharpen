using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoSharpen.Test.Dtos;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test;

public sealed partial class GlobalFilterTests
{
    [Fact]
    public async Task global_filter_json__on_find_and_execute__should_apply_global_filter()
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

        var result = await context.Find<Book>().ExecuteAsync();
        await context.DropDataBaseAsync();

        result.Count.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_filter_definition__on_find_and_execute__should_apply_global_filter()
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

        var result = await context.Find<Book>().ExecuteAsync();
        await context.DropDataBaseAsync();

        result.Count.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_bson_document__on_find_and_execute__should_apply_global_filter()
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

        var result = await context.Find<Book>().ExecuteAsync();
        await context.DropDataBaseAsync();

        result.Count.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_filter_json__on_find_and_execute_first__should_not_return_deleted()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

        var faker = new Faker();
        var book = new Book
        {
            Title = faker.Commerce.Department(),
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.Find<Book>().ExecuteFirstAsync());
        await context.DropDataBaseAsync();
    }

    [Fact]
    public async Task global_filter_json__on_find_and_execute_single__should_not_return_deleted()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

        var faker = new Faker();
        var book = new Book
        {
            Title = faker.Commerce.Department(),
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.Find<Book>().ExecuteSingleAsync());
        await context.DropDataBaseAsync();
    }

    [Fact]
    public async Task global_filter_json__on_find_and_execute_one__should_not_return_deleted()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

        var faker = new Faker();
        var book = new Book
        {
            Title = faker.Commerce.Department(),
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.Find<Book>().OneAsync(book.Id));
        await context.DropDataBaseAsync();
    }

    [Fact]
    public async Task global_filter_json__on_find_and_execute_many_func__should_not_return_deleted()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

        var faker = new Faker();
        var book = new Book
        {
            Title = faker.Commerce.Department(),
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var result = await context.Find<Book>().ManyAsync(x => x.MatchId(book.Id));
        await context.DropDataBaseAsync();

        result.Count.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_json__on_find_and_execute_many_expression__should_not_return_deleted()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

        var faker = new Faker();
        var book = new Book
        {
            Title = faker.Commerce.Department(),
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var result = await context.Find<Book>().ManyAsync(x => !x.Deleted);
        await context.DropDataBaseAsync();

        result.Count.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute__should_apply_global_filter()
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

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ExecuteAsync();
        await context.DropDataBaseAsync();

        result.Count.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_filter_definition__on_find_with_projection_and_execute__should_apply_global_filter()
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

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ExecuteAsync();
        await context.DropDataBaseAsync();

        result.Count.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_bson_document__on_find_with_projection_and_execute__should_apply_global_filter()
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

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ExecuteAsync();
        await context.DropDataBaseAsync();

        result.Count.Should().Be(books.Count(x => !x.Deleted));
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute_first__should_not_return_deleted()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

        var faker = new Faker();
        var book = new Book
        {
            Title = faker.Commerce.Department(),
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ExecuteFirstAsync();
        result.Should().BeNull();

        await context.DropDataBaseAsync();
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute_single__should_not_return_deleted()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

        var faker = new Faker();
        var book = new Book
        {
            Title = faker.Commerce.Department(),
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ExecuteSingleAsync();
        result.Should().BeNull();

        await context.DropDataBaseAsync();
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute_one__should_not_return_deleted()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

        var faker = new Faker();
        var book = new Book
        {
            Title = faker.Commerce.Department(),
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).OneAsync(book.Id);
        result.Should().BeNull();
        await context.DropDataBaseAsync();
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute_many_func__should_not_return_deleted()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

        var faker = new Faker();
        var book = new Book
        {
            Title = faker.Commerce.Department(),
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ManyAsync(x => x.MatchId(book.Id));
        await context.DropDataBaseAsync();

        result.Count.Should().Be(0);
    }

    [Fact]
    public async Task global_filter_json__on_find_with_projection_and_execute_many_expression__should_not_return_deleted()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }");

        var faker = new Faker();
        var book = new Book
        {
            Title = faker.Commerce.Department(),
            ISBN = faker.Vehicle.Model(),
            Deleted = true
        };

        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(book);

        var result = await context.Find<Book, BookDto>().Project(x => new BookDto()).ManyAsync(x => !x.Deleted);
        await context.DropDataBaseAsync();

        result.Count.Should().Be(0);
    }
}