using Bogus;
using FluentAssertions;
using MongoDB.Driver;
using MongoSharpen.Test.Dtos;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.DbContexts;

public partial class DbContextTests
{
    [Fact]
    public async Task update__on_update_one_item__update_should_reflect_to_db()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);
        var faker = new Faker();
        var author = new Author { Name = $"{faker.Name.FirstName()} {faker.Name.LastName()}" };
        await ctx.SaveAsync(author);

        var newName = $"{faker.Name.FirstName()} {faker.Name.LastName()}";
        await ctx.Update<Author>(x => x
                .Match(i => i.Eq(f => f.Id, author.Id))
                .Match(i => i.Eq(f => f.Name, author.Name)))
            .Modify(x => x.Set(i => i.Name, newName))
            .ExecuteAsync();

        var result = await ctx.Find<Author>(x => x.Match(x.Eq(i => i.Id, author.Id))).ExecuteSingleAsync();
        result.Name.Should().Be(newName);
    }

    [Fact]
    public async Task update__on_update_one_item_when_implements_i_modified_on__update_should_update_modified_on_property()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);
        var faker = new Faker();
        var author = new Author { Name = $"{faker.Name.FirstName()} {faker.Name.LastName()}" };
        await ctx.SaveAsync(author);

        var newName = $"{faker.Name.FirstName()} {faker.Name.LastName()}";
        await ctx.Update<Author>(x => x
                .Match(i => i.Eq(f => f.Id, author.Id))
                .Match(i => i.Eq(f => f.Name, author.Name)))
            .Modify(x => x.Set(i => i.Name, newName))
            .ExecuteAsync();

        var result = await ctx.Find<Author>(x => x.Match(x.Eq(i => i.Id, author.Id))).ExecuteSingleAsync();
        result.Name.Should().Be(newName);
        result.ModifiedOn.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task update__on_execute_and_get__should_return_item_saved_in_db()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);
        var faker = new Faker();
        var author = new Author { Name = $"{faker.Name.FirstName()} {faker.Name.LastName()}" };
        await ctx.SaveAsync(author);

        var newName = $"{faker.Name.FirstName()} {faker.Name.LastName()}";
        var result = await ctx.Update<Author>(x => x
                .Match("{_id:" + $"ObjectId(\'{author.Id}\')" + "}"))
            .Modify(x => x.Set(i => i.Name, newName))
            .ExecuteAndGetAsync();

        result.Name.Should().Be(newName);
    }

    [Fact]
    public async Task update__on_execute_and_get_when_implements_i_modified_on__update_should_update_modified_on_property()
    {
        var random = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(random);
        var faker = new Faker();
        var author = new Author { Name = $"{faker.Name.FirstName()} {faker.Name.LastName()}" };
        await ctx.SaveAsync(author);

        var newName = $"{faker.Name.FirstName()} {faker.Name.LastName()}";
        var result = await ctx.Update<Author>(x => x
                .Match("{_id:" + $"ObjectId(\'{author.Id}\')" + "}"))
            .Modify(x => x.Set(i => i.Name, newName))
            .ExecuteAndGetAsync();

        result.Name.Should().Be(newName);
        result.ModifiedOn.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task update_with_projection__on_execute_and_get__should_return_item_saved_in_db()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        var faker = new Faker();
        var book = new Book
        {
            Title = $"{faker.Commerce.Product()}-{faker.Commerce.Department()}",
            ISBN = faker.Vehicle.Model()
        };
        await ctx.SaveAsync(book);

        var title = $"{faker.Commerce.Product()}-{faker.Commerce.Department()}";
        var isbn = faker.Vehicle.Model();

        var result = await ctx
            .Update<Book, BookDto>(x => x
                .Match("{_id:" + $"ObjectId(\'{book.Id}\')" + "}"))
            .Modify(x => x
                .Set(i => i.Set(a => a.ISBN, isbn))
                .Set(i => i.Set(a => a.Authors, new List<Author>()))
                .Set(Builders<Book>.Update.Set(a => a.Title, title)))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ExecuteAndGetAsync();

        result.Title.Should().Be(title);
        result.ISBN.Should().Be(isbn);
        result.Authors.Count().Should().Be(0);
    }
    
    [Fact]
    public async Task update_with_projection__when_no_projection_setup__should_throw_exception()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        var faker = new Faker();
        var book = new Book
        {
            Title = $"{faker.Commerce.Product()}-{faker.Commerce.Department()}",
            ISBN = faker.Vehicle.Model()
        };
        await ctx.SaveAsync(book);

        var title = $"{faker.Commerce.Product()}-{faker.Commerce.Department()}";
        var isbn = faker.Vehicle.Model();

        await Assert.ThrowsAsync<InvalidOperationException>(()=> ctx
            .Update<Book, BookDto>(x => x
                .Match("{_id:" + $"ObjectId(\'{book.Id}\')" + "}"))
            .Modify(x => x
                .Set(i => i.Set(a => a.ISBN, isbn))
                .Set(i => i.Set(a => a.Authors, new List<Author>()))
                .Set(Builders<Book>.Update.Set(a => a.Title, title)))
            .ExecuteAndGetAsync());
    }
    
    [Fact]
    public async Task update_with_projection__when_multiple_projection_setup__should_throw_exception()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        var faker = new Faker();
        var book = new Book
        {
            Title = $"{faker.Commerce.Product()}-{faker.Commerce.Department()}",
            ISBN = faker.Vehicle.Model()
        };
        await ctx.SaveAsync(book);

        var title = $"{faker.Commerce.Product()}-{faker.Commerce.Department()}";
        var isbn = faker.Vehicle.Model();

        await Assert.ThrowsAsync<InvalidOperationException>(()=> ctx
            .Update<Book, BookDto>(x => x
                .Match("{_id:" + $"ObjectId(\'{book.Id}\')" + "}"))
            .Modify(x => x
                .Set(i => i.Set(a => a.ISBN, isbn))
                .Set(i => i.Set(a => a.Authors, new List<Author>()))
                .Set(Builders<Book>.Update.Set(a => a.Title, title)))
            .Project(x => new BookDto())
            .Project(x => new BookDto())
            .ExecuteAndGetAsync());
    }
}