using Bogus;
using FluentAssertions;
using MongoDB.Driver;
using MongoSharpen.Builders;
using MongoSharpen.Test.Dtos;
using MongoSharpen.Test.Entities;
using Xunit;

namespace MongoSharpen.Test.DbContexts;

public partial class DbContextTests
{
    [Fact]
    public async Task find__with_no_projection__when_no_filters__should_get_all()
    {
        var ctx = DbFactory.Get("library");
        var books = await ctx.Find<Book>().ExecuteAsync();

        books.Count.Should().Be(_bookFixture.Books.Count);
    }

    [Fact]
    public async Task find__with_no_projection_and_with_match_id__should_return_only_one_entity()
    {
        var ctx = DbFactory.Get("library");
        var reference = _bookFixture.Books.First();

        var actual = await ctx.Find<Book>(x => x.MatchId(reference.Id)).ExecuteAsync();

        actual.Count.Should().Be(1);
    }

    [Fact]
    public async Task find__with_no_projection_and_with_match_id__should_return_reference_entity()
    {
        var ctx = DbFactory.Get("library");
        var reference = _bookFixture.Books.First();

        var actual = await ctx.Find(Builders<Book>.Filter.Eq(x => x.Id, reference.Id)).ExecuteAsync();

        actual.Count.Should().Be(1);
    }

    [Fact]
    public async Task find__with_no_projection_and_with_sort__should_return_sorted_items()
    {
        var sorted = _bookFixture.Books.OrderBy(t => t.Title).ToList();

        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book>()
            .Sort(x => x.By(t => t.Title))
            .ExecuteAsync();

        var faker = new Faker();
        var index = faker.Random.Number(0, 9);

        actual[index].Id.Should().Be(sorted[index].Id);
    }

    [Fact]
    public async Task find__with_no_projection_and_with_sort_and_collation_override__should_return_sorted_items()
    {
        var sorted = _bookFixture.Books.OrderBy(t => t.Title).ToList();

        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book>()
            .Sort(x => x.By(t => t.Title))
            .Collation(new Collation("fil"))
            .ExecuteAsync();

        var index = new Random().Next(0, 9);
        actual[index].Id.Should().Be(sorted[index].Id);
    }

    [Fact]
    public async Task find__with_no_projection_and_with_skip__should_return_item_after_skip()
    {
        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book>().Skip(1).ExecuteFirstAsync();

        actual.Id.Should().Be(_bookFixture.Books[1].Id);
    }

    [Fact]
    public async Task find__with_no_projection_and_with_limit__should_return_item_after_skip()
    {
        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book>().Limit(1).ExecuteFirstAsync();

        actual.Id.Should().Be(_bookFixture.Books[0].Id);
    }

    [Fact]
    public async Task find__with_no_projection_and_with_sort_skip_and_limit__should_return_item_after_skip()
    {
        var sorted = _bookFixture.Books.OrderBy(t => t.Title).ToList();

        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book>().Sort(x => x.By(t => t.Title)).Skip(5).Limit(5).ExecuteFirstAsync();

        actual.Id.Should().Be(sorted[5].Id);
    }

    [Fact]
    public async Task find__with_no_projection_and_match_id_on_execute_single_should_return_match_with_id()
    {
        var first = _bookFixture.Books.First();
        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book>(x => x.MatchId(first.Id)).ExecuteSingleAsync();

        actual.Id.Should().Be(first.Id);
    }

    [Fact]
    public async Task find__with_no_projection_and_with_more_than_one_result_on_execute_single_should_throw_exception()
    {
        var ctx = DbFactory.Get("library");
        await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.Find<Book>().ExecuteSingleAsync());
    }

    [Fact]
    public async Task find__with_no_projection_and_on_execute_one__should_return_item_from_specified_filter()
    {
        var first = _bookFixture.Books.First();
        var last = _bookFixture.Books.Last();

        var ctx = DbFactory.Get("library");
        var result = await ctx.Find<Book>(x => x.MatchId(first.Id)).OneAsync(last.Id);

        result.Id.Should().Be(last.Id);
    }

    [Fact]
    public async Task find__with_no_projection_and_on_execute_many__should_return_items_from_specified_filter()
    {
        var first = _bookFixture.Books.First();

        var ctx = DbFactory.Get("library");
        var result = await ctx.Find<Book>(x => x.MatchId(first.Id)).ManyAsync(x => x.Title.Contains("-"));

        result.Count.Should().Be(_bookFixture.Books.Count);
    }

    [Fact]
    public async Task
        find__with_no_projection_and_on_execute_many_using_filter_expression__should_return_items_from_specified_filter()
    {
        var first = _bookFixture.Books.First();

        var ctx = DbFactory.Get("library");
        var result = await ctx
            .Find<Book>(x => x.MatchId(first.Id))
            .ManyAsync(x => x.Match(t => t.Title == first.Title));

        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task find__on_execute_any__when_found__should_return_true()
    {
        var first = _bookFixture.Books.First();

        var ctx = DbFactory.Get("library");
        var result = await ctx.Find<Book>(x => x.MatchId(first.Id)).AnyAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task find__on_execute_any__when_not_found__should_return_false()
    {
        var ctx = DbFactory.Get("library");
        var result = await ctx.Find<Book>(x => x.MatchId(Guid.NewGuid().ToString())).AnyAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task find__with_projection__when_no_filters__should_get_all()
    {
        var ctx = DbFactory.Get("library");
        var books = await ctx.Find<Book, BookDto>()
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ExecuteAsync();

        books.Count.Should().Be(_bookFixture.Books.Count);
    }

    [Fact]
    public async Task find__with_projection_and_with_match_id__should_return_only_one_entity()
    {
        var ctx = DbFactory.Get("library");
        var reference = _bookFixture.Books.First();

        var actual = await ctx.Find<Book, BookDto>(x => x.MatchId(reference.Id))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ExecuteAsync();

        actual.Count.Should().Be(1);
    }

    [Fact]
    public async Task find__with_projection_and_with_match_id__should_return_reference_entity()
    {
        var ctx = DbFactory.Get("library");
        var reference = _bookFixture.Books.First();

        var actual = await ctx.Find<Book, BookDto>(Builders<Book>.Filter.Eq(i => i.Id, reference.Id))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ExecuteAsync();

        actual.Count.Should().Be(1);
    }

    [Fact]
    public async Task find__with_projection_and_with_sort__should_return_sorted_items()
    {
        var sorted = _bookFixture.Books.OrderByDescending(t => t.Title).ToList();

        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book, BookDto>()
            .Sort(x => x.By(t => t.Title, Order.Descending))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ExecuteAsync();

        var faker = new Faker();
        var index = faker.Random.Number(0, 9);

        actual[index].Id.Should().Be(sorted[index].Id);
    }

    [Fact]
    public async Task find__with_projection_and_with_sort_and_collation_override__should_return_sorted_items()
    {
        var sorted = _bookFixture.Books.OrderByDescending(t => t.Title).ToList();

        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book, BookDto>()
            .Sort(x => x.By(t => t.Title, Order.Descending))
            .Collation(new Collation("fil"))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ExecuteAsync();

        var index = new Random().Next(0, 9);
        actual[index].Id.Should().Be(sorted[index].Id);
    }

    [Fact]
    public async Task find__with_projection_and_with_skip__should_return_item_after_skip()
    {
        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book, BookDto>()
            .Skip(1)
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ExecuteFirstAsync();

        actual.Id.Should().Be(_bookFixture.Books[1].Id);
    }

    [Fact]
    public async Task find__with_projection_and_with_limit__should_return_item_after_skip()
    {
        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book, BookDto>()
            .Limit(1)
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ExecuteFirstAsync();

        actual.Id.Should().Be(_bookFixture.Books[0].Id);
    }

    [Fact]
    public async Task find__with_projection_and_with_sort_skip_and_limit__should_return_item_after_skip()
    {
        var sorted = _bookFixture.Books.OrderBy(t => t.Title).ToList();

        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book, BookDto>()
            .Sort(x => x.By(t => t.Title))
            .Skip(5)
            .Limit(5)
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ExecuteFirstAsync();

        actual.Id.Should().Be(sorted[5].Id);
    }

    [Fact]
    public async Task find__with_projection_and_match_id_on_execute_single_should_return_match_with_id()
    {
        var first = _bookFixture.Books.First();
        var ctx = DbFactory.Get("library");
        var actual = await ctx.Find<Book, BookDto>(x => x.MatchId(first.Id))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ExecuteSingleAsync();

        actual.Id.Should().Be(first.Id);
    }

    [Fact]
    public async Task find__with_projection_and_with_more_than_one_result_on_execute_single_should_throw_exception()
    {
        var ctx = DbFactory.Get("library");
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ctx.Find<Book, BookDto>()
                .Project(x => new BookDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    ISBN = x.ISBN,
                    Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
                }).ExecuteSingleAsync());
    }

    [Fact]
    public async Task find__with_projection_and_on_execute_one__should_return_item_from_specified_filter()
    {
        var first = _bookFixture.Books.First();
        var last = _bookFixture.Books.Last();

        var ctx = DbFactory.Get("library");
        var result = await ctx.Find<Book, BookDto>(x => x.MatchId(first.Id))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).OneAsync(last.Id);

        result.Id.Should().Be(last.Id);
    }

    [Fact]
    public async Task find__with_projection_and_on_execute_many__should_return_items_from_specified_filter()
    {
        var first = _bookFixture.Books.First();

        var ctx = DbFactory.Get("library");
        var result = await ctx.Find<Book, BookDto>(x => x.MatchId(first.Id))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ManyAsync(x => x.Title.Contains("-"));

        result.Count.Should().Be(_bookFixture.Books.Count);
    }

    [Fact]
    public async Task find__with_projection_and_on_execute_many_using_filter_expression__should_return_items_from_specified_filter()
    {
        var first = _bookFixture.Books.First();

        var ctx = DbFactory.Get("library");
        var result = await ctx
            .Find<Book, BookDto>(x => x.MatchId(first.Id))
            .Project(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title,
                ISBN = x.ISBN,
                Authors = x.Authors.Select(a => new AuthorDto { Id = a.Id, Name = a.Name })
            }).ManyAsync(x => x.Match(t => t.Title == first.Title));

        result.Count.Should().Be(1);
    }

    [Fact]
    private async Task find__with_projection__when_set_projection_more_than_once__should_throw_exception()
    {
        var ctx = DbFactory.Get("library");
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ctx.Find<Book, BookDto>()
                .Project(x => new BookDto { Id = x.Id })
                .Project(x => new BookDto { Id = x.Id })
                .ExecuteSingleAsync());
    }

    [Fact]
    private async Task find__with_projection__when_no_projection_is_set__should_throw_exception()
    {
        var ctx = DbFactory.Get("library");
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ctx.Find<Book, BookDto>().ExecuteSingleAsync());
    }
}