using System.Reflection;
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

        var queryable = context.Queryable<Book>();
        var queryableList = await queryable.ToListAsync();

        queryableList.Should().HaveCount(books.Count(i => !i.Deleted));
    }

    [Fact]
    public async Task queryable__when_global_filter_is_setup_but_ignore_should_disregard_global_filter()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter<IDeleteOn>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!);

        var books = new List<Book>
        {
            new() { Deleted = true },
            new() { Deleted = false },
            new() { Deleted = false }
        };

        var context = factory.Get(Guid.NewGuid().ToString(), true);
        await context.SaveAsync(books);

        var queryable = context.Queryable<Book>();
        var queryableList = await queryable.ToListAsync();

        queryableList.Should().HaveCount(books.Count);
    }
}