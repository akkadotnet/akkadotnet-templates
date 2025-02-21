open Akka.Actor
open Akka.Hosting
open Akka.Streams
open Akka.Streams.Dsl
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open AkkaStreamsTemplate
open System
open System.Threading

let hostbuilder = HostBuilder()

hostbuilder.ConfigureServices(fun services ->
    services.AddAkka("MyActorSystem", fun b ->

        b.WithActors(fun sys reg resolver -> 
            let transformActorProps = resolver.Props<TransformActor>()
            let transformActor = sys.ActorOf(transformActorProps, "transform-actor")
            reg.Register<TransformActor>(transformActor)) |> ignore

    ) |> ignore
) |> ignore

let host = hostbuilder.Build()
let completionTask = host.RunAsync()

let system = host.Services.GetRequiredService<ActorSystem>()

let transformer = host.Services.GetRequiredService<IRequiredActor<TransformActor>>().ActorRef

let transformTask number = 
                    let cts = new CancellationTokenSource(TimeSpan.FromSeconds(3.0))
                    transformer.Ask<string>(number.ToString(), cts.Token)                    
                    

Source.From(seq {1..1000})
            .Where(fun i -> i % 2 = 0)
            .Select(fun i -> i.ToString())
            .Throttle(10, TimeSpan.FromSeconds(1.0), 10, ThrottleMode.Shaping)
            .SelectAsync(5, transformTask )
            .RunForeach((fun s -> printfn "%s" s), system)
            |> Async.AwaitTask |> Async.RunSynchronously

completionTask |> Async.AwaitTask |> Async.RunSynchronously
