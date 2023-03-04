using Ductus.FluentDocker.Services;

namespace MongoSharpen.Test.Fixtures;

public abstract class DockerCompose : IDisposable
{
    protected ICompositeService? CompositeService;

    protected IHostService? DockerHost;

    protected void InitializeDocker()
    {
        EnsureDockerHost();
        CompositeService = Build();

        try
        {
            CompositeService.Start();
        }
        catch
        {
            CompositeService.Dispose();
            throw;
        }
    }

    protected abstract ICompositeService Build();

    private void EnsureDockerHost()
    {
        if (DockerHost?.State == ServiceRunningState.Running) return;

        var hosts = new Hosts().Discover();
        DockerHost = hosts.FirstOrDefault(x => x.IsNative) ?? hosts.FirstOrDefault();

        if (DockerHost != null && DockerHost.State != ServiceRunningState.Running)
        {
            DockerHost.Start();
            return;
        }

        if (hosts.Count > 0) DockerHost = hosts.First();

        if (DockerHost == null) EnsureDockerHost();
    }

    public void Dispose()
    {
        var compositeService = CompositeService;
        CompositeService = null;
        try
        {
            compositeService?.Dispose();
        }
        catch
        {
            //ignore
        }
    }
}