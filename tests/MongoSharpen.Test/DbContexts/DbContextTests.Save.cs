using Bogus;
using FluentAssertions;
using MongoSharpen.Test.Dtos;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.DbContexts;

public partial class DbContextTests
{
    [Fact]
    public async Task save__on_save_one_item__should_reflect_to_db()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        var faker = new Faker();
        var author = new Author { Name = $"{faker.Name.FirstName()} {faker.Name.LastName()}" };

        await ctx.SaveAsync(author);

        var result = await ctx.Find<Author, AuthorDto>(x => x.MatchId(author.Id))
            .Project(x => new AuthorDto { Id = x.Id, Name = x.Name })
            .ExecuteSingleAsync();
        result.Id.Should().Be(author.Id);
        result.Name.Should().Be(author.Name);
    }

    [Fact]
    public async Task save__on_save_one_item_with_assigned_id__should_reflect_to_db_with_assigned_id()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        var faker = new Faker();
        var author = new Author { Name = $"{faker.Name.FirstName()} {faker.Name.LastName()}" };
        author.SetNewId();

        var currentId = author.Id;

        await ctx.SaveAsync(author);

        var result = await ctx.Find<Author, AuthorDto>(x => x.MatchId(author.Id))
            .Project(x => new AuthorDto { Id = x.Id, Name = x.Name })
            .ExecuteSingleAsync();
        result.Id.Should().Be(currentId);
    }

    [Fact]
    public async Task save__on_save_one_and_implements_i_created_on__should_set_created_on_property()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        var faker = new Faker();
        var book = new Book
        {
            Title = $"{faker.Commerce.Product()}-{faker.Commerce.Department()}",
            ISBN = faker.Vehicle.Model(),
        };

        await ctx.SaveAsync(book);

        var result = await ctx.Find<Book>(x => x.MatchId(book.Id)).ExecuteSingleAsync();
        result.CreatedOn.Should().NotBeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task save__on_save_multiple__should_reflect_to_db()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        var faker = new Faker();

        var randomText = Guid.NewGuid().ToString();
        var books = new List<Book>();
        for (var i = 0; i < 10; i++)
        {
            var book = new Book
            {
                Title = $"{randomText}-{faker.Commerce.Department()}",
                ISBN = faker.Vehicle.Model(),
                Authors = new List<Author> { new() }
            };
            books.Add(book);
        }

        await ctx.SaveAsync(books);

        var result = await ctx.Find<Book, BookDto>(x => x
                .Match(i => i.Title.Contains(randomText)))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            })
            .ExecuteAsync();

        result.Count.Should().Be(books.Count);
        result[0].Title.Should().Be(books[0].Title);
        result[0].ISBN.Should().Be(books[0].ISBN);
        result[0].Authors.Count().Should().Be(1);
    }

    [Fact]
    public async Task save__on_save_multiple_with_assigned_id__should_reflect_to_db_with_assigned_id()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        var faker = new Faker();

        var randomText = Guid.NewGuid().ToString();
        var books = new List<Book>();
        for (var i = 0; i < 10; i++)
        {
            var book = new Book
            {
                Title = $"{randomText}-{faker.Commerce.Department()}",
                ISBN = faker.Vehicle.Model(),
                Authors = new List<Author> { new() }
            };
            book.SetNewId();
            books.Add(book);
        }

        await ctx.SaveAsync(books);

        var result = await ctx.Find<Book, BookDto>(x => x
                .Match(i => i.Title.Contains(randomText)))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            })
            .ExecuteAsync();

        result[0].Id.Should().Be(books[0].Id);
    }

    [Fact]
    public async Task save__on_save_multiple_and_implements_i_created_on__should_set_created_on_property()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        var faker = new Faker();
        var books = new List<Book>();

        for (var i = 0; i < 10; i++)
        {
            var book = new Book
            {
                Title = $"{faker.Commerce.Product()}-{faker.Commerce.Department()}",
                ISBN = faker.Vehicle.Model(),
            };
            books.Add(book);
        }

        await ctx.SaveAsync(books);

        var result = await ctx.Find<Book>().ExecuteAsync();

        foreach (var i in result)
        {
            i.CreatedOn.Should().NotBeAfter(DateTime.UtcNow);
        }
    }
}