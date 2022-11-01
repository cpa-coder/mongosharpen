using FluentAssertions;
using MongoDB.Bson.Serialization.Conventions;
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
    public void on_get__when_exist__should_get_existing_db_context()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object) { DefaultConnection = "mongodb://localhost:27107" };
        var context = factory.Get("db");

        var sameContext = factory.Get("db");

        sameContext.Should().Be(context);
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
    public void on_get_with_custom_connection__when_exist__should_get_existing_db_context()
    {
        var factory = new DbFactoryInternal(_wrapperMock.Object);

        var context = factory.Get("db", "mongodb://localhost:27107");

        var sameContext = factory.Get("db", "mongodb://localhost:27107");
        sameContext.Should().Be(context);
    }
}