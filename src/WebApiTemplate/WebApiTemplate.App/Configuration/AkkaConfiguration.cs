using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Akka.Hosting;
using Akka.Management;
using Akka.Management.Cluster.Bootstrap;
using Akka.Persistence.Hosting;
using Akka.Remote.Hosting;
using WebApiTemplate.App.Actors;
using WebApiTemplate.Domain;

namespace WebApiTemplate.App.Configuration;

public static class AkkaConfiguration
{
    public static AkkaConfigurationBuilder ConfigureWebApiActorSystem(this AkkaConfigurationBuilder builder,
        AkkaSettings settings)
    {
        var b = builder
            .WithRemoting(settings.RemoteOptions);

        if (settings.AkkaManagementOptions is { Enabled: true })
        {
            // need to delete seed-nodes so Akka.Management will take precedence
            var clusterOptions = settings.ClusterOptions;
            clusterOptions.SeedNodes = Array.Empty<string>();

            b = b
                .WithClustering(clusterOptions)
                .WithAkkaManagement(hostName: settings.AkkaManagementOptions.Hostname,
                    settings.AkkaManagementOptions.Port)
                .WithClusterBootstrap(serviceName: settings.AkkaManagementOptions.ServiceName,
                    portName: settings.AkkaManagementOptions.PortName,
                    requiredContactPoints: settings.AkkaManagementOptions.RequiredContactPointsNr);

            switch (settings.AkkaManagementOptions.DiscoveryMethod)
            {
                case DiscoveryMethod.Kubernetes:
                    break;
                case DiscoveryMethod.AwsEcsTagBased:
                    break;
                case DiscoveryMethod.AwsEc2TagBased:
                    break;
                case DiscoveryMethod.AzureTableStorage:
                    break;
                case DiscoveryMethod.Config:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return b;
    }

    public static AkkaConfigurationBuilder ConfigurePersistence(this AkkaConfigurationBuilder builder,
        AkkaSettings settings)
    {
        return builder.WithInMemoryJournal().WithInMemorySnapshotStore();
    }

    public static AkkaConfigurationBuilder ConfigureCounterActors(this AkkaConfigurationBuilder builder,
        AkkaSettings settings)
    {
        var extractor = HashCodeMessageExtractor.Create(30, o =>
        {
            return o switch
            {
                IWithCounterId counterId => counterId.CounterId,
                ShardRegion.StartEntity startEntity => startEntity.EntityId,
                _ => string.Empty
            };
        }, o => o);
        
        return builder.WithShardRegion<CounterActor>("counter",
            (system, registry, resolver) => s => Props.Create(() => new CounterActor(s)),
            extractor, settings.ShardOptions);
    }
}