# AkkaConsoleTemplate

This is a simple template designed to incorporate local [Akka.NET](https://getakka.net/) into a console application. The template supports both C# and F#.

## Installation

To use this template, first you must install the `Akka.Templates` package from NuGet:

```shell
dotnet new install "Akka.Templates::*"
```

From there, you can use this template via the following command:

```shell
# For C#
dotnet new akka.console -n "your-project-name"

# For F#
dotnet new akka.console -n "your-project-name" -lang F#
```

## How It Works

This template uses [Akka.Hosting](https://github.com/akkadotnet/Akka.Hosting), as a best practice for managing the lifecycle of Akka.NET applications and for integrating with the Microsoft.Extensions ecosystem.

<details open>
<summary><b>C# Implementation</b></summary>

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

// Example actor with dependency injection
public class TimerActor : ReceiveActor
{
    private readonly IActorRef _helloActor;

    public TimerActor(IRequiredActor<HelloActor> helloActor)
    {
        _helloActor = helloActor.ActorRef;
        Receive<string>(message =>
        {
            _helloActor.Tell(message);
        });
    }
}
```

</details>

<details>
<summary><b>F# Implementation</b></summary>

```fsharp
let configureAkka (builder: IServiceCollection) =
    builder.AddAkka("MyActorSystem", (fun (builder: AkkaConfigurationBuilder) (sp: IServiceProvider) ->
        builder
            .WithActors(fun (system, registry, _) ->
                let helloActor = spawn system "hello-actor" (actorOf HelloActor.actorBehavior)
                registry.Register<HelloActor>(helloActor))
            .WithActors(fun (system, registry, resolver) ->
                let timerActorProps = resolver.Props<TimerActor>()
                let timerActor = system.ActorOf(timerActorProps, "timer-actor")
                registry.Register<TimerActor>(timerActor))
        |> ignore))

// Example actor with dependency injection
type TimerActor =
    inherit ReceiveActor
    [<DefaultValue>] val mutable timer: ITimerScheduler
    val mutable helloActor: IActorRef

    new(helloActor: IRequiredActor<HelloActor>) = 
        {helloActor = helloActor.ActorRef}
        then
            base.Receive<string>(fun message -> helloActor.ActorRef.Tell(message))        

    interface IWithTimers with 
        member this.Timers with get() = this.timer and set(value) = this.timer <- value

    override this.PreStart() =
        let timer = this :> IWithTimers
        timer.Timers.StartPeriodicTimer("key", "hello", System.TimeSpan.FromSeconds(1.0) )
```

</details>

In both implementations, the `TimerActor` depends on the `HelloActor`, demonstrating how to use the `DependencyResolver` (the `resolver` parameter) to inject a `IRequiredActor<HelloActor>` into the `TimerActor`'s constructor.

In a real-world scenario, you could just resolve the `HelloActor`'s `IActorRef` via a `registry.Get<HelloActor>` call, which would technically be simpler and cleaner - but we wanted to demonstrate how to use [`Akka.DependencyInjection`](https://getakka.net/articles/actors/dependency-injection.html) here.

## Additional Resources
- [Akka.Hosting Documentation](https://github.com/akkadotnet/Akka.Hosting)
- [Akka.NET Dependency Injection](https://getakka.net/articles/actors/dependency-injection.html)
- [Microsoft.Extensions.Hosting](https://docs.microsoft.com/en-us/dotnet/core/extensions/hosting)