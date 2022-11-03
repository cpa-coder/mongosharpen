using Bogus;
using FluentAssertions;
using MongoSharpen.Test.Dtos;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test;

public partial class DbContextTests
{
    private static List<Book> GenerateBooks()
    {
        var faker = new Faker();
        var books = new List<Book>();
        for (var i = 1; i <= 10; i++)
        {
            var oddOrEven = i % 2 != 0 ? "odd" : "even";
            var book = new Book
            {
                Title = $"{oddOrEven}-{faker.Commerce.Department()}",
                ISBN = faker.Vehicle.Model()
            };
            books.Add(book);
        }
        return books;
    }

    private static List<Book> GenerateBooksWithSystemGenerated()
    {
        var faker = new Faker();
        var books = new List<Book>();
        for (var i = 1; i <= 10; i++)
        {
            var systemGenerated = faker.Random.Bool();
            var oddOrEven = i % 2 != 0 ? "odd" : "even";
            var book = new Book
            {
                Title = $"{oddOrEven}-{faker.Commerce.Department()}",
                ISBN = faker.Vehicle.Model(),
                SystemGenerated = systemGenerated
            };
            books.Add(book);
        }
        return books;
    }

    [Fact]
    public async Task delete__on_execute_many__should_delete_matched_items()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooks();
        await ctx.SaveAsync(books);

        await ctx.Delete<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteManyAsync();

        var found = await ctx.Find<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteAsync();
        found.Should().BeEmpty();
    }

    [Fact]
    public async Task delete_with_system_generated__on_execute_many__should_delete_non_system_generated_items()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        await ctx.Delete<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteManyAsync();

        var found = await ctx.Find<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteAsync();
        found.Count.Should().Be(books.Count(x => x.SystemGenerated && x.Title.Contains("odd")));
    }

    [Fact]
    public async Task delete_with_system_generated__on_execute_many_by_force__should_delete_all_matched_items()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        await ctx.Delete<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteManyAsync(forceDelete: true);

        var found = await ctx.Find<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteAsync();
        found.Should().BeEmpty();
    }

    [Fact]
    public async Task delete__on_execute_one__should_delete_single_matched_item_in_db()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooks();
        await ctx.SaveAsync(books);

        await ctx.Delete<Book>(x => x.Match(i => i.Title.Contains("odd"))).ExecuteOneAsync();

        var found = await ctx.Find<Book>(x => x.MatchId(books[0].Id)).ExecuteAsync();
        found.Should().BeEmpty();
    }

    [Fact]
    public async Task delete__on_execute_one__when_system_generated__should_not_delete_item()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var systemGeneratedItem = books.First(i => i.SystemGenerated);
        await ctx.Delete<Book>(x => x.Match(i => i.Id == systemGeneratedItem.Id)).ExecuteOneAsync();

        var found = await ctx.Find<Book>().OneAsync(systemGeneratedItem.Id);
        found.Should().NotBeNull();
    }

    [Fact]
    public async Task delete__on_execute_one_by_force__when_system_generated__should_delete_item()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var systemGeneratedItem = books.First(i => i.SystemGenerated);
        await ctx.Delete<Book>(x => x.Match(i => i.Id == systemGeneratedItem.Id)).ExecuteOneAsync(forceDelete: true);

        //throw exception when no items found
        await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.Find<Book>(x =>
            x.Match(i => i.Id == systemGeneratedItem.Id)).ExecuteFirstAsync());
    }

    [Fact]
    public async Task delete__on_get_and_execute__should_get_matched_item_in_db_before_delete()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooks();
        await ctx.SaveAsync(books);

        var book = await ctx.Delete<Book>(x => x.MatchId(books[0].Id)).GetAndExecuteAsync();

        book.Id.Should().Be(books[0].Id);

        var found = await ctx.Find<Book>(x => x.MatchId(books[0].Id)).ExecuteAsync();
        found.Should().BeEmpty();
    }

    [Fact]
    public async Task delete__on_get_and_execute__when_system_generated__should_not_delete_item()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var systemGeneratedItem = books.First(i => i.SystemGenerated);

        //throw exception when no item deleted
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ctx.Delete<Book>(x => x.Match(i => i.Id == systemGeneratedItem.Id)).GetAndExecuteAsync());
    }

    [Fact]
    public async Task delete__on_get_and_execute_by_force__when_system_generated__should_get_delete_item()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var systemGeneratedItem = books.First(i => i.SystemGenerated);
        var book = await ctx.Delete<Book>(x => x.Match(i => i.Id == systemGeneratedItem.Id)).GetAndExecuteAsync(forceDelete: true);

        book.Id.Should().Be(systemGeneratedItem.Id);

        //throw exception when deleted is not found in db
        await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.Find<Book>(x =>
            x.Match(i => i.Id == systemGeneratedItem.Id)).ExecuteFirstAsync());
    }

    [Fact]
    public async Task delete_using_transaction__on_execute_and_get_by_force__when_system_generated__should_get_delete_item()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        using var trans = ctx.Transaction();

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var systemGeneratedItem = books.First(i => i.SystemGenerated);
        var book = await ctx.Delete<Book>(x => x.Match(i => i.Id == systemGeneratedItem.Id)).GetAndExecuteAsync(forceDelete: true);

        await trans.CommitAsync();

        book.Id.Should().Be(systemGeneratedItem.Id);

        //throw exception when deleted is not found in db
        await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.Find<Book>(x =>
            x.Match(i => i.Id == systemGeneratedItem.Id)).ExecuteFirstAsync());
    }

    [Fact]
    public async Task delete_with_projection__when_no_project_setup__should_throw_exception()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooks();
        await ctx.SaveAsync(books);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ctx.Delete<Book, BookDto>(x => x.MatchId(books[0].Id)).GetAndExecuteAsync());
    }

    [Fact]
    public async Task delete_with_projection__when_multiple_project_setup__should_throw_exception()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooks();
        await ctx.SaveAsync(books);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ctx.Delete<Book, BookDto>(x => x.MatchId(books[0].Id))
                .Project(x => new BookDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    ISBN = x.ISBN
                })
                .Project(x => new BookDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    ISBN = x.ISBN
                }).GetAndExecuteAsync());
    }

    [Fact]
    public async Task delete_with_projection__on_get_and_execute__should_get_projection()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooks();
        await ctx.SaveAsync(books);

        var book = await ctx.Delete<Book, BookDto>(x => x.MatchId(books[0].Id))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN
            })
            .GetAndExecuteAsync();

        book.Id.Should().Be(books[0].Id);
        book.Title.Should().Be(books[0].Title);
        book.ISBN.Should().Be(books[0].ISBN);
    }

    [Fact]
    public async Task delete__with_projection__on_get_and_execute__when_system_generated__should_not_delete_item()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var systemGeneratedItem = books.First(i => i.SystemGenerated);

        //throw exception when no item deleted
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ctx.Delete<Book, BookDto>(x => x.Match(i => i.Id == systemGeneratedItem.Id))
                .Project(x => new BookDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    ISBN = x.ISBN
                })
                .GetAndExecuteAsync());
    }

    [Fact]
    public async Task
        delete__with_projection__on_get_and_execute_and_get_by_force__when_system_generated__should_get_and_delete_item()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var systemGeneratedItem = books.First(i => i.SystemGenerated);

        var book = await ctx.Delete<Book, BookDto>(x => x.MatchId(systemGeneratedItem.Id))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN
            })
            .GetAndExecuteAsync(forceDelete: true);

        book.Id.Should().Be(systemGeneratedItem.Id);
        book.Title.Should().Be(systemGeneratedItem.Title);
        book.ISBN.Should().Be(systemGeneratedItem.ISBN);
    }

    [Fact]
    public async Task delete__with_projection_using_transaction__on_get_and_execute_and_get_by_force__when_system_generated__should_get_and_delete_item()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);

        using var trans = ctx.Transaction();

        var books = GenerateBooksWithSystemGenerated();
        await ctx.SaveAsync(books);

        var systemGeneratedItem = books.First(i => i.SystemGenerated);

        var book = await ctx.Delete<Book, BookDto>(x => x.MatchId(systemGeneratedItem.Id))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN
            })
            .GetAndExecuteAsync(forceDelete: true);

        await trans.CommitAsync();

        book.Id.Should().Be(systemGeneratedItem.Id);
        book.Title.Should().Be(systemGeneratedItem.Title);
        book.ISBN.Should().Be(systemGeneratedItem.ISBN);
    }
}