using System.Reflection;
using FluentAssertions;
using MongoDB.Driver;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.GlobalFilters;

[Xunit.Collection("Server collection")]
public sealed partial class GlobalFilterTests
{
    private DbFactoryInternal InitializeFactory()
    {
        var factory = new DbFactoryInternal
        {
            DefaultConnection = "mongodb://localhost:41253"
        };

        return factory;
    }

    [Fact]
    public void global_filter_json__when_not_interface__should_throw_exception()
    {
        var factory = InitializeFactory();

        Assert.Throws<ArgumentException>(() =>
            factory.SetGlobalFilter<Entity>("{ deleted : false }", Assembly.GetAssembly(typeof(Book))!));
    }

    [Fact]
    public void merge_with_global_filter__when_no_filter_setup__should_return_filter()
    {
        var factory = InitializeFactory();
        factory.SetGlobalFilter(Builders<Book>.Filter.Eq(i => i.Deleted, false));

        var context = factory.Get(Guid.NewGuid().ToString()) as DbContext;
        var filter = Builders<Author>.Filter.Eq(i => i.Deleted, false);

        var result = context!.MergeWithGlobalFilter(filter);

        result.Should().BeEquivalentTo(filter);
    }
}