using System.Diagnostics;
using Akka.Actor;
using Akka.Aspire;
using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using Akka.Discovery.Azure;
using Akka.Discovery.Redis;
using Akka.Hosting;
using Akka.Persistence.Azure;
using Akka.Persistence.Azure.Hosting;
using Akka.Persistence.Hosting;
using Akka.Util;
using WebApiTemplate.App.Actors;
using WebApiTemplate.Domain;

namespace WebApiTemplate.App.Configuration;

public static class AkkaConfiguration
{
    public static IServiceCollection ConfigureWebApiAkka(this IServiceCollection services, IConfiguration configuration,
        Action<AkkaConfigurationBuilder, IServiceProvider> additionalConfig)
    {
        var akkaSettings = configuration.GetRequiredSection("AkkaSettings").Get<AkkaSettings>();
        Debug.Assert(akkaSettings != null, nameof(akkaSettings) + " != null");

        services.AddSingleton(akkaSettings);

        return services.AddAkka(akkaSettings.ActorSystemName, (builder, sp) =>
        {
            builder.ConfigureActorSystem(sp);
            additionalConfig(builder, sp);
        });
    }

    public static AkkaConfigurationBuilder ConfigureActorSystem(this AkkaConfigurationBuilder builder,
        IServiceProvider sp)
    {
        var settings = sp.GetRequiredService<AkkaSettings>();

        return builder
            .ConfigureLoggers(configBuilder =>
            {
                configBuilder.LogConfigOnStart = settings.LogConfigOnStart;
                configBuilder.AddLoggerFactory();
            })
            .ConfigureNetwork(sp)
            .ConfigurePersistence(sp)
            .ConfigureCounterActors(sp);
    }

    public static AkkaConfigurationBuilder ConfigureNetwork(this AkkaConfigurationBuilder builder,
        IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<AkkaSettings>();

        if (!settings.UseClustering)
            return builder;

        // When running under .NET Aspire, the AppHost injects the cluster configuration
        // (Akka:Cluster:* env vars) and the discovery connection string. WithAspireClusterBootstrap
        // wires remoting, cluster, Akka.Management, Cluster Bootstrap, and the built-in
        // liveness + cluster-membership health checks. It no-ops unless Akka:Cluster:Enabled is true.
        return builder.WithAspireClusterBootstrap(
            serviceProvider,
            configureDiscovery: (b, config) =>
            {
                // The AppHost injects the clustering resource's connection string name via
                // Akka__Cluster__Clustering__ConnectionStringName; the resource itself is named
                // "akka-discovery" in the AppHost, so fall back to that literal.
                var connectionStringName = config["Akka:Cluster:Clustering:ConnectionStringName"] ?? "akka-discovery";
                var connectionString = config.GetConnectionString(connectionStringName);
                if (string.IsNullOrEmpty(connectionString))
                    return;

                var serviceName = config["Akka:Cluster:ServiceName"];
                if (settings.DiscoveryBackend.Equals("azure", StringComparison.OrdinalIgnoreCase))
                {
                    b.WithAzureDiscovery(connectionString, serviceName);
                }
                else
                {
                    b.WithRedisDiscovery(connectionString, serviceName);
                }
            },
            clusterConfigure: c => c.Roles = settings.ClusterOptions.Roles);
    }

    public static AkkaConfigurationBuilder ConfigurePersistence(this AkkaConfigurationBuilder builder,
        IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<AkkaSettings>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        switch (settings.PersistenceMode)
        {
            case PersistenceMode.InMemory:
                return builder.WithInMemoryJournal().WithInMemorySnapshotStore();
            case PersistenceMode.Azure:
            {
                var connectionStringName = configuration.GetSection("AzureStorageSettings")
                    .Get<AzureStorageSettings>()?.ConnectionStringName;
                Debug.Assert(connectionStringName != null, nameof(connectionStringName) + " != null");
                var connectionString = configuration.GetConnectionString(connectionStringName);
                Debug.Assert(connectionString != null, nameof(connectionString) + " != null");

                return builder.WithAzurePersistence(connectionString);
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static AkkaConfigurationBuilder ConfigureCounterActors(this AkkaConfigurationBuilder builder,
        IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<AkkaSettings>();
        var extractor = CreateCounterMessageRouter();

        if (settings.UseClustering)
        {
            return builder.WithShardRegion<CounterActor>("counter",
                (system, registry, resolver) => s => Props.Create(() => new CounterActor(s)),
                extractor, settings.ShardOptions);
        }

        return builder.WithActors((system, registry, resolver) =>
        {
            var parent =
                system.ActorOf(
                    GenericChildPerEntityParent.Props(extractor, s => Props.Create(() => new CounterActor(s))),
                    "counters");
            registry.Register<CounterActor>(parent);
        });
    }

    public static HashCodeMessageExtractor CreateCounterMessageRouter()
    {
        return HashCodeMessageExtractor.Create(30, o =>
        {
            return o switch
            {
                IWithCounterId counterId => counterId.CounterId,
                _ => null
            };
        }, o => o);
    }
}
