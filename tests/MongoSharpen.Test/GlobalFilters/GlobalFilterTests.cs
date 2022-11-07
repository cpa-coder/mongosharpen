using FluentAssertions;
using MongoDB.Driver;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.GlobalFilters;

public sealed partial class GlobalFilterTests
{
    [Fact]
    public void global_filter_json__when_not_interface__should_throw_exception()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        Assert.Throws<ArgumentException>(() => factory.SetGlobalFilter<Entity>("{ deleted : false }"));
    }

    [Fact]
    public void merge_with_global_filter__when_no_filter_setup__should_return_filter()
    {
        var conn = Environment.GetEnvironmentVariable("MONGOSHARPEN") ?? "mongodb://localhost:27107";
        var factory = new DbFactoryInternal(new ConventionRegistryWrapper()) { DefaultConnection = conn };

        factory.SetGlobalFilter(Builders<Book>.Filter.Eq(i => i.Deleted, false));

        var context = factory.Get(Guid.NewGuid().ToString());
        var filter = Builders<Author>.Filter.Eq(i => i.Deleted, false);

        var result = context.MergeWithGlobalFilter(filter);
        context.DropDataBaseAsync();

        result.Should().BeEquivalentTo(filter);
    }
}