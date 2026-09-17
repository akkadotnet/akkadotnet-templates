using System.Net;
using Akka.Cluster.Hosting;
using Akka.Remote.Hosting;

namespace WebApiTemplate.App.Configuration;

public class AkkaSettings
{
    public string ActorSystemName { get; set; } = "AkkaWeb";

    public bool UseClustering { get; set; } = true;

    public bool LogConfigOnStart { get; set; } = false;

    /// <summary>
    /// Determines which Akka.Discovery backend is used for cluster bootstrap when
    /// <see cref="UseClustering"/> is enabled. Valid values: "redis" (default) or "azure".
    /// This is injected from the template's Discovery symbol at template-generation time.
    /// </summary>
    public string DiscoveryBackend { get; set; } = "DiscoveryBackendParameter";

    public RemoteOptions RemoteOptions { get; set; } = new()
    {
        // can be overridden via config, but is dynamic by default
        PublicHostName = Dns.GetHostName()
    };

    public ClusterOptions ClusterOptions { get; set; } = new ClusterOptions()
    {
        // use our dynamic local host name by default
        SeedNodes = new[] { $"akka.tcp://AkkaWebApi@{Dns.GetHostName()}:8081" }
    };

    public ShardOptions ShardOptions { get; set; } = new ShardOptions();
}
