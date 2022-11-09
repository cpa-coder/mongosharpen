using FluentAssertions;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoSharpen.Test.Entities;
using Moq;
using Xunit;

namespace MongoSharpen.Test;

public class DbFactoryTests
{
    private readonly Mock<IConventionRegistryWrapper> _wrapperMock;

    public DbFactoryTests()
    {
        _wrapperMock = new Mock<IConventionRegistryWrapper>();
    }

    [Fact]
    public void has_default_camel_case_convention_pack()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object);
        var count = factory.ConventionNames.Count(c => c.Contains("camelCase"));
        count.Should().Be(1);
    }

    [Fact]
    public void on_add_convention__should_add_to_convention_list()
    {
        var pack = new ConventionPack { new IgnoreIfNullConvention(true) };

        var factory = new DbFactoryInternal(_wrapperMock.Object);
        factory.AddConvention("ignore if null", pack);

        var count = factory.ConventionNames.Count(c => c.Contains("ignore if null"));
        count.Should().Be(1);
    }

    [Fact]
    public void on_add_convention__when_already_started_getting_db_context__should_throw_exception()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object) { DefaultConnection = "mongodb://localhost:27107" };
        factory.Get("new-db");

        var pack = new ConventionPack { new IgnoreIfNullConvention(true) };
        Assert.Throws<InvalidOperationException>(() => factory.AddConvention("ignore if null", pack));
    }

    [Fact]
    public void on_remove_convention__should_remove_to_convention_list()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object);
        factory.RemoveConvention("camelCase");

        factory.ConventionNames.Count.Should().Be(0);
    }

    [Fact]
    public void on_remove_convention__when_already_started_getting_db_context__should_throw_exception()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object) { DefaultConnection = "mongodb://localhost:27107" };
        factory.Get("new-db");

        Assert.Throws<InvalidOperationException>(() => factory.RemoveConvention("camelCase"));
    }

    [Fact]
    public void on_set_default_connection__when_empty_string__should_throw_argument_exception()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object);
        Assert.Throws<ArgumentException>(() => factory.DefaultConnection = string.Empty);
    }

    [Fact]
    public void on_set_default_connection__when_already_set__should_throw_invalid_operation_exception()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object) { DefaultConnection = "mongodb://localhost:27107" };
        Assert.Throws<InvalidOperationException>(() => factory.DefaultConnection = "another connection");
    }

    [Fact]
    public void on_set_default_database__when_empty_string__should_throw_argument_exception()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object);
        Assert.Throws<ArgumentException>(() => factory.DefaultDatabase = string.Empty);
    }

    [Fact]
    public void on_set_default_database__when_already_set__should_throw_invalid_operation_exception()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object) { DefaultDatabase = "default-db" };
        Assert.Throws<InvalidOperationException>(() => factory.DefaultDatabase = "another db");
    }

    [Fact]
    public void on_get_default_database__when_default_database_is_not_set__should_throw_invalid_operation_exception()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object);
        Assert.Throws<ArgumentException>(() => factory.DefaultDatabase);
    }

    [Fact]
    public void on_get__when_no_default_database_and_connection__should_throw_argument_exception()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object);
        Assert.Throws<ArgumentException>(() => factory.Get());
    }

    [Fact]
    public void on_get__when_no_default_database__should_throw_argument_exception()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object) { DefaultConnection = "mongodb://localhost:27107" };
        Assert.Throws<ArgumentException>(() => factory.Get());
    }

    [Fact]
    public void on_get__when_properly_configured__should_return_db_context()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object)
            { DefaultConnection = "mongodb://localhost:27107", DefaultDatabase = "db" };

        var context = factory.Get();
        context.Database.DatabaseNamespace.DatabaseName.Should().Be("db");
    }

    [Fact]
    public void on_get__when_no_default_connection__should_throw_invalid_operation_exception()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object);
        Assert.Throws<InvalidOperationException>(() => factory.Get("new-db"));
    }

    [Fact]
    public void on_get__should_get_new_db_context()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object) { DefaultConnection = "mongodb://localhost:27107" };

        var context = factory.Get("db");

        context.Should().NotBeNull();
        factory.DbContexts.Count.Should().Be(1);
    }

    [Fact]
    public void on_get__conventions_should_be_registered()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object) { DefaultConnection = "mongodb://localhost:27107" };

        factory.Get("db");

        _wrapperMock.Verify(m => m.Register(It.IsAny<string>(), It.IsAny<ConventionPack>()));
    }

    [Fact]
    public void on_get__should_always_get_new_db_context()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object) { DefaultConnection = "mongodb://localhost:27107" };
        var context = factory.Get("db");

        var newContext = factory.Get("db");

        newContext.Should().NotBe(context);
    }

    [Fact]
    public void on_get_with_custom_connection__should_get_new_db_context()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object);

        var context = factory.Get("db", "mongodb://localhost:27107");

        context.Should().NotBeNull();
        factory.DbContexts.Count.Should().Be(1);
    }

    [Fact]
    public void on_get_with_custom_connection__conventions_should_be_registered()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object);

        factory.Get("db", "mongodb://localhost:27107");

        _wrapperMock.Verify(m => m.Register(It.IsAny<string>(), It.IsAny<ConventionPack>()),
            Times.Exactly(factory.ConventionNames.Count));
    }

    [Fact]
    public void on_get_with_custom_connection__should_always_get_new_db_context()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object);

        var context = factory.Get("db", "mongodb://localhost:27107");
        var newContext = factory.Get("db", "mongodb://localhost:27107");
        newContext.Should().NotBe(context);
    }

    [Fact]
    public async Task on_get_with_multiple_transactions_with_no_conflicting_document_write()
    {
        const string dbName = "trans-db";
        var factory = new DbFactoryInternal(_wrapperMock.Object) { DefaultConnection = "mongodb://localhost:30051" };

        // to ensure new that the database is created
        foreach (var context in factory.DbContexts)
            await context.DropDataBaseAsync();

        async Task Task1(DbFactoryInternal f)
        {
            var context = f.Get(dbName);
            using var trans = context.Transaction();

            var books = new List<Book>();
            var count = await context.CountAsync<Book>();

            for (var i = 0; i < 100; i++)
            {
                books.Add(new Book { Title = $"Book {i + count}" });
            }
            await context.SaveAsync(books);
            await trans.CommitAsync();
        }

        async Task Task2(DbFactoryInternal f)
        {
            var context = f.Get(dbName);

            using var trans = context.Transaction();

            var authors = new List<Author>();
            for (var i = 0; i < 1000; i++)
            {
                authors.Add(new Author { Name = $"Author {i}" });
            }

            await context.SaveAsync(authors);
            await trans.CommitAsync();
        }

        // to test that this does not throw an exception
        await Task.WhenAll(Task1(factory), Task2(factory));

        foreach (var context in factory.DbContexts)
            await context.DropDataBaseAsync();
    }

    [Fact]
    public async Task on_get_with_multiple_transactions_with_conflicting_document_write()
    {
        const string dbName = "trans-db";
        var factory = new DbFactoryInternal(_wrapperMock.Object) { DefaultConnection = "mongodb://localhost:30051" };

        // to ensure new that the database is created
        foreach (var context in factory.DbContexts)
            await context.DropDataBaseAsync();

        async Task Task1(DbFactoryInternal f)
        {
            var context = f.Get(dbName);
            using var trans = context.Transaction();

            var books = new List<Book>();
            var count = await context.CountAsync<Book>();

            for (var i = 0; i < 100; i++)
            {
                books.Add(new Book { Title = $"Book {i + count}" });
            }
            await context.SaveAsync(books);
            await trans.CommitAsync();
        }

        async Task Task2(DbFactoryInternal f)
        {
            var context = f.Get(dbName);

            using var trans = context.Transaction();

            var books = new List<Book>();
            for (var i = 0; i < 1000; i++)
            {
                books.Add(new Book { Title = $"Book {i}" });
            }

            //make sure not to write to the same document in production
            var authors = new List<Author>();
            for (var i = 0; i < 1000; i++)
            {
                authors.Add(new Author { Name = $"Author {i}" });
            }

            await context.SaveAsync(authors);

            //this is the conflicting write
            await context.SaveAsync(books);

            await trans.CommitAsync();
        }

        await Assert.ThrowsAsync<MongoCommandException>(() => Task.WhenAll(Task1(factory), Task2(factory)));

        // to ensure new that the database is created
        foreach (var context in factory.DbContexts)
            await context.DropDataBaseAsync();
    }
}