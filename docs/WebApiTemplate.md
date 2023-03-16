# WebApiTemplate

This template is designed to integrate [Akka.NET](https://getakka.net/) with ASP.NET Web APIs.

## Installation

To use this template, first you must install the `Akka.Templates` package from NuGet:

```shell
dotnet new -i "Akka.Templates::*"
```

From there, you can use this template via the following command:

```
dotnet new akkawebapi -n "your project name"
```

## Usage

### Key HTTP Routes

* https://localhost:{ASP_NET_PORT}/swagger/index.html - Swagger endpoint for testing out Akka.NET-powered APIs
* https://localhost:{ASP_NET_PORT}/healthz/akka - Akka.HealthCheck HTTP endpoint

### Configuration

This application is highly configurable and supports the following configuration settings out of the box:

```csharp
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
```

These will all be extracted some `appSettings.json`, `appSettings.{ASPNET_ENVIRONMENT}.json`, and environment variables. Please see the [Akka.Hosting](https://github.com/akkadotnet/Akka.Hosting) documentation for relevant details.

#### Running with Azure Persistence and Discovery

To run this template locally using [Akka.Persistence.Azure](https://github.com/petabridge/Akka.Persistence.Azure) and [Akka.Azure.Discovery](https://github.com/akkadotnet/Akka.Management/tree/dev/src/discovery/azure/Akka.Discovery.Azure), use the `AzureDiscoveryAndStorage` profile included in the `launchSettings.json` _after_ you launch [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio), the local emulator for Azure Table / Blob / Queue storage:

**Step 1 - Launch Azurite**

Go to the root of your `WebApiTemplate.App` directory and execute `start-azurite.sh` (Linux / OS X) or `start-azurite.cmd` (Windows):

```shell
./start-azurite.sh
```

This will launch a local Azurite instance using Docker behind the scenes, running on all of the default ports.

**Step 2 - Launch your application using `ASPNET_ENVIRONMENT=Azure`**

You can set the `ASPNET_ENVIRONMEN` enviroment variable in any number of ways, but we've included a default `launchSettings.json` profile that will do this for you from the `dotnet` CLI:

```shell
dotnet run --launch-profile "AzureDiscoveryAndStorage"
```