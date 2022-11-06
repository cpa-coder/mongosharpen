using FluentAssertions;
using MongoDB.Bson;
using Xunit;

namespace MongoSharpen.Test;

public class EntityTests
{
    [Fact]
    public void created_by_string_to_object_conversion()
    {
        var id = ObjectId.GenerateNewId().ToString();
        CreatedBy createdBy = id;

        createdBy.Id.Should().Be(id);
    }

    [Fact]
    public void created_by_object_to_string_conversion()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var createdBy = new CreatedBy { Id = id };

        string idString = createdBy;

        idString.Should().Be(id);
    }

    [Fact]
    public void modified_by_string_to_object_conversion()
    {
        var id = ObjectId.GenerateNewId().ToString();
        ModifiedBy modifiedBy = id;

        modifiedBy.Id.Should().Be(id);
    }

    [Fact]
    public void modified_by_object_to_string_conversion()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var modifiedBy = new ModifiedBy { Id = id };

        string idString = modifiedBy;

        idString.Should().Be(id);
    }

    [Fact]
    public void deleted_by_string_to_object_conversion()
    {
        var id = ObjectId.GenerateNewId().ToString();
        DeletedBy deletedBy = id;

        deletedBy.Id.Should().Be(id);
    }

    [Fact]
    public void deleted_by_object_to_string_conversion()
    {
        var id = ObjectId.GenerateNewId().ToString();
        var deletedBy = new DeletedBy { Id = id };

        string idString = deletedBy;

        idString.Should().Be(id);
    }
}