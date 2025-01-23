# Akka.Cluster WebApiTemplate

This template is designed to integrate [Akka.NET](https://getakka.net/) Clusters with ASP.NET Web APIs.

## Installation

To use this template, first you must install the `Akka.Templates` package from NuGet:

```shell
dotnet new -i "Akka.Templates::*"
```

From there, you can use this template via the following command:

```
dotnet new akka.cluster.webapi -n "your project name"
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

## Docker

By default, if you run the following `dotnet` CLI instruction you will produce a Docker image of your application:

```shell
dotnet publish --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer
```

This will use the [.NET SDk's built-in container support](https://devblogs.microsoft.com/dotnet/announcing-builtin-container-support-for-the-dotnet-sdk/) to automatically produce a Linux image of your application using the same .NET version as your application.

Upon running the command you will see a Docker image added to your local registry that looks like the following:

```shell
λ  dotnet publish --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer
MSBuild version 17.4.0+18d5aef85 for .NET
  Determining projects to restore...
  All projects are up-to-date for restore.
  WebApiTemplate.Domain -> E:\Repositories\olympus\akkadotnet-templates\src\WebApiTemplate\src\WebApiTemplate.Domain\bi
  n\Release\net7.0\WebApiTemplate.Domain.dll
  WebApiTemplate.App -> E:\Repositories\olympus\akkadotnet-templates\src\WebApiTemplate\src\WebApiTemplate.App\bin\Rele
  ase\net7.0\linux-x64\WebApiTemplate.App.dll
  WebApiTemplate.App -> E:\Repositories\olympus\akkadotnet-templates\src\WebApiTemplate\src\WebApiTemplate.App\bin\Rele
  ase\net7.0\linux-x64\publish\
  Building image 'webpapitemplate-app' with tags 1.0.0,latest on top of base image mcr.microsoft.com/dotnet/aspnet:7.0
  Pushed container 'webpapitemplate-app:1.0.0' to Docker daemon
  Pushed container 'webpapitemplate-app:latest' to Docker daemon
```

The Docker image will take the following forms:

* `{APPNAME}:{VersionPrefix}` - the name you gave this app when you ran `dotnet new` plus the current `VersionPrefix` and
* `{APPNAME}:latest` - the name you gave this app when you ran `dotnet new` plus the `latest` tag.

> You can launch a multi-node cluster by updating the [`docker-compose.yml`](https://github.com/akkadotnet/akkadotnet-templates/blob/dev/src/WebApiTemplate/docker/docker-compose.yaml) file to use your specific image name.