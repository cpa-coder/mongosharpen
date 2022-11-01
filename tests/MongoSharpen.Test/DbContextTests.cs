﻿using FluentAssertions;
using MongoSharpen.Test.Fixtures;
using Xunit;

namespace MongoSharpen.Test;

[CollectionDefinition("db-context")]
public class ContextCollection : ICollectionFixture<DbContextFixture>
{
}

public partial class DbContextTests
{
    [Fact]
    public void start_transaction__session_should_not_be_null()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        using var trans = ctx.Transaction();
        ctx.Session.Should().NotBeNull();
    }

    [Fact]
    public void start_transaction__when_called_more_than_once__should_throw_exception()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        using var trans = ctx.Transaction();

        Assert.Throws<InvalidOperationException>(() =>
        {
            using var newTrans = ctx.Transaction();
        });
    }

    [Fact]
    public async Task commit__when_no_transaction_started__should_throw_exception()
    {
        var randomDb = Guid.NewGuid().ToString();
        var ctx = DbFactory.Get(randomDb);
        using var trans = ctx.Transaction();
        ctx.Session = null;

        await Assert.ThrowsAsync<InvalidOperationException>(() => trans.CommitAsync());
    }
}