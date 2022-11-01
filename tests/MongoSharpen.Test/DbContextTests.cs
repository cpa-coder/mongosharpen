using MongoSharpen.Test.Fixtures;
using Xunit;

namespace MongoSharpen.Test;

[CollectionDefinition("db-context")]
public class ContextCollection : ICollectionFixture<DbContextFixture>
{
}

public partial class DbContextTests
{
}