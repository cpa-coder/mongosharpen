using Bogus;
using FluentAssertions;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.DbContexts;

public partial class DbContextTests
{
    [Fact]
    public async Task distinct__when_no_property_set__should_throw_exception()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.Distinct<Book, string>().ExecuteAsync());
    }

    [Fact]
    public async Task distinct__when_property_is_set_multiple_times__should_throw_exception()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ctx.Distinct<Book, string>()
                .Property(nameof(Book.Title))
                .Property(nameof(Book.Title))
                .ExecuteAsync());
    }

    [Fact]
    public async Task distinct__without_filter__should_return_list_of_distinct_values_of_selected_property()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = new List<Book>();
        for (var i = 0; i < 10; i++)
        {
            var num = new Faker().Random.Number(0, 3);
            books.Add(new Book
            {
                Title = $"Book{num}"
            });
        }

        await ctx.SaveAsync(books);

        var items = await ctx.Distinct<Book, string>().Property(nameof(Book.Title)).ExecuteAsync();
        items.Count.Should().Be(books.DistinctBy(x => x.Title).Count());
    }

    [Fact]
    public async Task distinct__with_filter__should_return_filtered_list_of_distinct_values_of_selected_property()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

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

        await ctx.SaveAsync(books);

        var undeleted = books.Where(t => !t.Deleted).DistinctBy(x => x.Title).Count();

        var items = await ctx.Distinct<Book, string>(x => x.Match(t => !t.Deleted)).Property("Title").ExecuteAsync();
        items.Count.Should().Be(undeleted);
    }
}