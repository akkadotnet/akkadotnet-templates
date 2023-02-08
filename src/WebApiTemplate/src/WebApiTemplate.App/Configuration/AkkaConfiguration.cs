using System.Diagnostics;
using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Akka.Discovery.Azure;
using Akka.Hosting;
using Akka.Management;
using Akka.Management.Cluster.Bootstrap;
using Akka.Persistence.Hosting;
using Akka.Remote.Hosting;
using Akka.Util;
using WebApiTemplate.App.Actors;
using WebApiTemplate.Domain;

namespace WebApiTemplate.App.Configuration;

public static class AkkaConfiguration
{
    public static IServiceCollection ConfigureWebApiAkka(this IServiceCollection services, IConfiguration configuration, Action<AkkaConfigurationBuilder, IServiceProvider> additionalConfig)
    {
        var akkaSettings = configuration.GetRequiredSection("AkkaSettings").Get<AkkaSettings>();
        Debug.Assert(akkaSettings != null, nameof(akkaSettings) + " != null");
        
        return services.AddAkka(akkaSettings.ActorSystemName, (builder, sp) =>
        {
            builder.ConfigureActorSystem(akkaSettings);
            additionalConfig(builder, sp);
        });
    }

    public static AkkaConfigurationBuilder ConfigureActorSystem(this AkkaConfigurationBuilder builder, AkkaSettings settings)
    {
        return builder
            .ConfigureNetwork(settings)
            .ConfigurePersistence(settings)
            .ConfigureCounterActors(settings);
    }

    public static AkkaConfigurationBuilder ConfigureNetwork(this AkkaConfigurationBuilder builder,
        AkkaSettings settings, IConfiguration configuration)
    {
        if (!settings.UseClustering)
            return builder;
        
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
                {
                    var connectionStringName = configuration.GetSection("AzureStorageSettings")
                        .Get<AzureStorageSettings>()?.ConnectionStringName;
                    Debug.Assert(connectionStringName != null, nameof(connectionStringName) + " != null");
                    var connectionString = configuration.GetConnectionString(connectionStringName);
                    
                    
                    b = b.WithAzureDiscovery(options =>
                    {
                        options.ServiceName = settings.AkkaManagementOptions.ServiceName;
                        options.ConnectionString = connectionString;
                    });
                    break;
                }
                case DiscoveryMethod.Config:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            b = b.WithClustering(settings.ClusterOptions);
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
        var extractor = CreateCounterMessageRouter();

        if (settings.UseClustering)
        {
            return builder.WithShardRegion<CounterActor>("counter",
                (system, registry, resolver) => s => Props.Create(() => new CounterActor(s)),
                extractor, settings.ShardOptions);
        }
        else
        {
            return builder.WithActors((system, registry, resolver) =>
            {
                var parent = system.ActorOf(GenericChildPerEntityParent.Props(extractor, s => Props.Create(() => new CounterActor(s))), "counters");
                registry.Register<CounterActor>(parent);
            });
        }
    }

    public static HashCodeMessageExtractor CreateCounterMessageRouter()
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
        return extractor;
    }
}