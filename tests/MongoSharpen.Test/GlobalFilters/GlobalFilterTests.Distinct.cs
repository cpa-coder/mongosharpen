using System.Reflection;
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

        var factory = InitializeFactory();
        var context = factory.Get(Guid.NewGuid().ToString());
        await context.SaveAsync(books);

        var result = await context.Distinct<Book, string>().Property(nameof(Book.Title)).ExecuteAsync();

        result.Count.Should().Be(books.DistinctBy(x => x.Title).Count());
    }

    [Fact]
    public async Task global_filter_json__on_distinct_with_global_filter__should_return_filtered_distinct_property_values()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

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

        var result = await context.Distinct<Book, string>().Property(nameof(Book.Title)).ExecuteAsync();

        result.Count.Should().Be(books.Where(t => !t.Deleted).DistinctBy(x => x.Title).Count());
    }

    [Fact]
    public async Task global_filter_json__on_distinct_with_global_filters__should_return_filtered_distinct_property_values()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

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

        var result = await context.Distinct<Book, string>(Builders<Book>.Filter.Match(t => t.Deleted))
            .Property(nameof(Book.Title)).ExecuteAsync();

        result.Count.Should().Be(0);
    }
}