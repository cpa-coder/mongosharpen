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
    public async Task queryable__when_global_filter_is_setup_should_apply_global_filter()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };
        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

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

        var queryable = context.Queryable<Book>();
        var queryableList = await queryable.ToListAsync();
        await context.DropDataBaseAsync();

        queryableList.Should().HaveCount(books.Count(i => !i.Deleted));
    }

    [Fact]
    public async Task queryable__when_global_filter_is_setup_but_ignore_should_disregard_global_filter()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };
        factory.SetGlobalFilter<ISoftDelete>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

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

        var context = factory.Get(Guid.NewGuid().ToString(), ignoreGlobalFilter: true);
        await context.SaveAsync(books);

        var queryable = context.Queryable<Book>();
        var queryableList = await queryable.ToListAsync();
        await context.DropDataBaseAsync();

        queryableList.Should().HaveCount(books.Count);
    }
}