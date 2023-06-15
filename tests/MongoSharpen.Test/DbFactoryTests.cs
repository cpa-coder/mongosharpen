using FluentAssertions;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoSharpen.Test.Entities;
using Moq.AutoMock;
using Xunit;

namespace MongoSharpen.Test;

[Xunit.Collection("Server collection")]
public class DbFactoryTests
{
    private const string ConnectionString = "mongodb://localhost:41253";
    private string RandomDb() => Guid.NewGuid().ToString();

    [Fact]
    public void has_default_camel_case_convention_pack()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();

        var count = factory.ConventionNames.Count(c => c.Contains("camelCase"));
        count.Should().Be(1);
    }

    [Fact]
    public void on_add_convention__should_add_to_convention_list()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();

        factory.AddConvention("ignore if null", new IgnoreIfNullConvention(true));

        var count = factory.ConventionNames.Count(c => c.Contains("ignore if null"));
        count.Should().Be(1);
    }

    [Fact]
    public void on_add_convention__when_already_started_getting_db_context__should_throw_exception()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();
        factory.DefaultConnection = ConnectionString;

        factory.Get(RandomDb());

        Assert.Throws<InvalidOperationException>(() => factory.AddConvention("ignore if null", new IgnoreIfNullConvention(true)));
    }

    [Fact]
    public void on_remove_convention__should_remove_to_convention_list()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();

        factory.RemoveConvention("camelCase");

        factory.ConventionNames.Count.Should().Be(0);
    }

    [Fact]
    public void on_remove_convention__when_already_started_getting_db_context__should_throw_exception()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();
        factory.DefaultConnection = ConnectionString;
        
        factory.Get(RandomDb());

        Assert.Throws<InvalidOperationException>(() => factory.RemoveConvention("camelCase"));
    }

    [Fact]
    public void on_set_default_connection__when_empty_string__should_throw_argument_exception()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();

        Assert.Throws<ArgumentException>(() => factory.DefaultConnection = string.Empty);
    }

    [Fact]
    public void on_set_default_connection__when_already_set__should_throw_invalid_operation_exception()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();
        factory.DefaultConnection = ConnectionString;

        Assert.Throws<InvalidOperationException>(() => factory.DefaultConnection = "another connection");
    }

    [Fact]
    public void on_set_default_database__when_empty_string__should_throw_argument_exception()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();
        Assert.Throws<ArgumentException>(() => factory.DefaultDatabase = string.Empty);
    }

    [Fact]
    public void on_set_default_database__when_already_set__should_throw_invalid_operation_exception()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();
        factory.DefaultDatabase = RandomDb();

        Assert.Throws<InvalidOperationException>(() => factory.DefaultDatabase = RandomDb());
    }

    [Fact]
    public void on_get_default_database__when_default_database_is_not_set__should_throw_invalid_operation_exception()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();

        Assert.Throws<ArgumentException>(() => factory.DefaultDatabase);
    }

    [Fact]
    public void on_get__when_no_default_database_and_connection__should_throw_argument_exception()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();

        Assert.Throws<ArgumentException>(() => factory.Get());
    }

    [Fact]
    public void on_get__when_no_default_database__should_throw_argument_exception()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();
        factory.DefaultConnection = ConnectionString;

        Assert.Throws<ArgumentException>(() => factory.Get());
    }

    [Fact]
    public void on_get__when_properly_configured__should_return_db_context()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();
        factory.DefaultConnection = ConnectionString;
        factory.DefaultDatabase = RandomDb();

        var context = factory.Get();
        context.Database.DatabaseNamespace.DatabaseName.Should().Be(factory.DefaultDatabase);
    }

    [Fact]
    public void on_get__when_no_default_connection__should_throw_invalid_operation_exception()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();

        Assert.Throws<InvalidOperationException>(() => factory.Get(RandomDb()));
    }

    [Fact]
    public void on_get__should_get_new_db_context()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();
        factory.DefaultConnection = ConnectionString;

        var context = factory.Get(RandomDb());

        context.Should().NotBeNull();
        factory.DbContexts.Count.Should().Be(1);
    }

    [Fact]
    public void on_get__should_always_get_new_db_context()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();
        factory.DefaultConnection = ConnectionString;

        var context = factory.Get(RandomDb());
        var newContext = factory.Get(RandomDb());

        newContext.Should().NotBe(context);
    }

    [Fact]
    public void on_get_with_custom_connection__should_get_new_db_context()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();

        var context = factory.Get(RandomDb(), ConnectionString);

        context.Should().NotBeNull();
        factory.DbContexts.Count.Should().Be(1);
    }

    [Fact]
    public void on_get_with_custom_connection__conventions_should_be_registered()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();

        factory.Get(RandomDb(), ConnectionString);

        factory.HasRegisteredConventions.Should().BeTrue();
    }

    [Fact]
    public void on_get_with_custom_connection__should_always_get_new_db_context()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();

        var context = factory.Get(RandomDb(), ConnectionString);
        var newContext = factory.Get(RandomDb(), ConnectionString);
        newContext.Should().NotBe(context);
    }

    [Fact]
    public async Task on_get_with_multiple_transactions_with_no_conflicting_document_write()
    {
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();
        factory.DefaultConnection = ConnectionString;

        var dbName = RandomDb();
        async Task Task1(IDbFactory f)
        {
            var context = f.Get(dbName);
            using var trans = context.Transaction();

            var books = new List<Book>();
            var count = await context.CountAsync<Book>();

            for (var i = 0; i < 100; i++) books.Add(new Book { Title = $"Book {i + count}" });
            await context.SaveAsync(books);
            await trans.CommitAsync();
        }

        async Task Task2(IDbFactory f)
        {
            var context = f.Get(dbName);

            using var trans = context.Transaction();

            var authors = new List<Author>();
            for (var i = 0; i < 1000; i++) authors.Add(new Author { Name = $"Author {i}" });

            await context.SaveAsync(authors);
            await trans.CommitAsync();
        }

        // to test that this does not throw an exception
        await Task.WhenAll(Task1(factory), Task2(factory));
    }

    [Fact]
    public async Task on_get_with_multiple_transactions_with_conflicting_document_write()
    {
        var dbName = RandomDb();
        var mocker = new AutoMocker();
        var factory = mocker.CreateInstance<DbFactoryInternal>();
        factory.DefaultConnection = ConnectionString;

        async Task Task1(IDbFactory f)
        {
            var context = f.Get(dbName);
            using var trans = context.Transaction();

            var books = new List<Book>();
            var count = await context.CountAsync<Book>();

            for (var i = 0; i < 100; i++) books.Add(new Book { Title = $"Book {i + count}" });
            await context.SaveAsync(books);
            await trans.CommitAsync();
        }

        async Task Task2(IDbFactory f)
        {
            var context = f.Get(dbName);

            using var trans = context.Transaction();

            var books = new List<Book>();
            for (var i = 0; i < 1000; i++) books.Add(new Book { Title = $"Book {i}" });

            //make sure not to write to the same document in production
            var authors = new List<Author>();
            for (var i = 0; i < 1000; i++) authors.Add(new Author { Name = $"Author {i}" });

            await context.SaveAsync(authors);

            //this is the conflicting write
            await context.SaveAsync(books);

            await trans.CommitAsync();
        }

        await Assert.ThrowsAsync<MongoCommandException>(() => Task.WhenAll(Task1(factory), Task2(factory)));
    }
}