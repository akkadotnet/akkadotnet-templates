# AkkaStreamsTemplate

This is a simple template designed to incorporate [Akka.NET](https://getakka.net/)'s [Akka.Streams APIs](https://getakka.net/articles/streams/introduction.html) into a local console template.

## Installation

To use this template, first you must install the `Akka.Templates` package from NuGet:

```shell
dotnet new -i "Akka.Templates::*"
```

From there, you can use this template via the following command:

```
dotnet new akka.streams -n "your project name"
```

## How It Works

This template uses [Akka.Hosting](https://github.com/akkadotnet/Akka.Hosting), as a best practice for managing the lifecycle of Akka.NET applications and for integrating with the Microsoft.Extensions ecosystem.

```csharp
hostBuilder.ConfigureServices((context, services) =>
{
    services.AddAkka("MyActorSystem", (builder, sp) =>
    {
        builder
            .WithActors((system, registry, resolver) =>
            {
                var helloActor = system.ActorOf(Props.Create(() => new TransformerActor()), "transformer");
                registry.Register<TransformerActor>(helloActor);
            });
    });
});
```

However, the real guts of the application happens further down in `Program.cs` - where we use the `IServiceProvider` to resolve both the `ActorSystem` and the `IRequiredActor<TransformerActor>` in order to use both of those inputs inside our Akka.NET stream:

```csharp
var host = hostBuilder.Build();

var completionTask = host.RunAsync();

// grab the ActorSystem from the DI container
var system = host.Services.GetRequiredService<ActorSystem>();

// grab the ActorRef from the DI container
IActorRef transformer = host.Services.GetRequiredService<IRequiredActor<TransformerActor>>().ActorRef;
```

The real guts of this application is, of course, [Akka.Streams](https://getakka.net/articles/streams/introduction.html):

```csharp
// create a stream that iterates over the numbers 1 to 100
await Source.From(Enumerable.Range(1, 1000))
    .Where(i => i % 2 == 0) // even numbers only
    .Select(i => i.ToString()) // convert to string
    .Throttle(10, TimeSpan.FromSeconds(1), 10, ThrottleMode.Shaping) // throttle stream to 10 elements per second
    .SelectAsync(5, async str => // invoke actor, up to 5 times in parallel, to convert string
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        return await transformer.Ask<string>(str, cts.Token);
    })
    .RunForeach(Console.WriteLine, system); // write all output to console
```

This is a simple, finite stream that uses some of [Akka.Streams' built-in stages](https://getakka.net/articles/streams/builtinstages.html) to demonstrate asynchronous stream processing as well as [Akka.NET actor integration with Akka.Streams](https://getakka.net/articles/streams/integration.html).