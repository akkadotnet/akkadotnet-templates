using System.Net;
using Akka.Cluster.Hosting;
using Akka.Remote.Hosting;

namespace WebApiTemplate.App.Configuration;

public class AkkaManagementOptions
{
    public bool Enabled { get; set; } = false;
    public string Hostname { get; set; } = Dns.GetHostName();
    public int Port { get; set; } = 8558;
    public string PortName { get; set; } = "management";

    public string ServiceName { get; set; } = "akka-management";

    /// <summary>
    /// Determines the number of nodes we need to make contact with in order to form a cluster initially.
    ///
    /// 3 is a safe default value.
    /// </summary>
    public int RequiredContactPointsNr { get; set; } = 3;

    public DiscoveryMethod DiscoveryMethod { get; set; } = DiscoveryMethod.Config;
}

/// <summary>
/// Determines which Akka.Discovery method to use when discovering other nodes to form and join clusters.
/// </summary>
public enum DiscoveryMethod
{
    Config,
    Kubernetes,
    AwsEcsTagBased,
    AwsEc2TagBased,
    AzureTableStorage
}

public enum PersistenceMode
{
    InMemory,
    Azure
}

public class AzureStorageSettings
{
    public string ConnectionStringName { get; set; } = "Azurite";
}

public class AkkaSettings
{
    public string ActorSystemName { get; set; } = "AkkaWeb";

    public bool UseClustering { get; set; } = true;

    public bool LogConfigOnStart { get; set; } = false;

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

    public PersistenceMode PersistenceMode { get; set; } = PersistenceMode.InMemory;

    public AkkaManagementOptions? AkkaManagementOptions { get; set; }
}