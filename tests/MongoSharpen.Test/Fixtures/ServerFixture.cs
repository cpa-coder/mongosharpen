using Ductus.FluentDocker.Model.Common;
using Ductus.FluentDocker.Model.Compose;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Impl;
using Xunit;

namespace MongoSharpen.Test.Fixtures;

public sealed class ServerFixture : DockerCompose
{
    public ServerFixture()
    {
        InitializeDocker();
        DbFactory.DefaultConnection = "mongodb://localhost:41253";
    }

    protected override ICompositeService Build()
    {
        var file = Path.Combine(Directory.GetCurrentDirectory(), (TemplateString) "docker-compose.yml");

        return new DockerComposeCompositeService(DockerHost, new DockerComposeConfig
        {
            ComposeFilePath = new List<string> { file },
            ForceRecreate = true,
            RemoveOrphans = true,
            StopOnDispose = true
        });
    }
}

[CollectionDefinition("Server collection")]
public class ServerCollection : ICollectionFixture<ServerFixture>
{
}