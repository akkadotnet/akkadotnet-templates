# AkkaConsoleTemplate

This is a simple template designed to incorporate local [Akka.NET](https://getakka.net/) into a console application.

## Installation

To use this template, first you must install the `Akka.Templates` package from NuGet:

```shell
dotnet new -i "Akka.Templates::*"
```

From there, you can use this template via the following command:

```
dotnet new akka.console -n "your project name"
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
                var helloActor = system.ActorOf(Props.Create(() => new HelloActor()), "hello-actor");
                registry.Register<HelloActor>(helloActor);
            })
            .WithActors((system, registry, resolver) =>
            {
                var timerActorProps =
                    resolver.Props<TimerActor>(); // uses Msft.Ext.DI to inject reference to helloActor
                var timerActor = system.ActorOf(timerActorProps, "timer-actor");
                registry.Register<TimerActor>(timerActor);
            });
    });
});
```

The `TimerActor` depends on the `HelloActor`, so we're going to use the `DependencyResolver` (the `resolver` parameter) to inject a `IRequiredActor<HelloActor>` into the `TimerActor`'s constructor:

```csharp
public TimerActor(IRequiredActor<HelloActor> helloActor)
{
    _helloActor = helloActor.ActorRef;
    Receive<string>(message =>
    {
        _helloActor.Tell(message);
    });
}
```

In a real-world scenario, you could just resolve the `HelloActor`'s `IActorRef` via a `registry.Get<HelloActor>` call, which would technically be simpler and cleaner - but we wanted to demonstrate how to use [`Akka.DependencyInjection`](https://getakka.net/articles/actors/dependency-injection.html) here.