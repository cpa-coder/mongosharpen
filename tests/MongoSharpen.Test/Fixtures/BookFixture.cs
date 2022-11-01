using System.Collections.Immutable;
using Bogus;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.Fixtures;

public class BookFixture : IAsyncLifetime
{
    public List<Book> Books { get; set; }

    public async Task InitializeAsync()
    {
        var ctx = DbFactory.Get("library");
        ctx.StartTransaction();

        var faker = new Faker();

        var authors = new List<Author>();
        for (var i = 0; i < 5; i++)
        {
            var author = new Author { Name = $"{faker.Name.FirstName()} {faker.Name.LastName()}" };
            authors.Add(author);
        }

        await ctx.SaveAsync(authors);

        var books = new List<Book>();
        for (var i = 0; i < 10; i++)
        {
            var book = new Book
            {
                Title = $"{faker.Random.Number(1, 10)}-{faker.Commerce.Department()}",
                ISBN = faker.Vehicle.Model(),
                Authors = new List<Author>
                {
                    authors[faker.Random.Number(max: 4)],
                    authors[faker.Random.Number(max: 4)],
                }.ToImmutableList()
            };
            books.Add(book);
        }

        Books = books;

        await ctx.SaveAsync(books);
        await ctx.CommitAsync();
    }

    public Task DisposeAsync()
    {
        var ctx = DbFactory.Get("library");
        return ctx.DropDataBaseAsync();
    }
}