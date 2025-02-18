open Akka.Actor
open Akka.Hosting
open Microsoft.Extensions.Hosting
open AkkaConsoleTemplate

let hostbuilder = HostBuilder()

hostbuilder.ConfigureServices(fun services ->
    services.AddAkka("MyActorSystem", fun b ->

        b.WithActors(fun sys reg  -> 
            let helloActor = sys.ActorOf(Props.Create<HelloActor>(fun () -> HelloActor()), "hello-actor")
            reg.Register<HelloActor>(helloActor)) |> ignore

        b.WithActors(fun sys reg resolver -> 
            let timerActorProps = resolver.Props<TimerActor>()
            let timerActor = sys.ActorOf(timerActorProps, "timer-actor")
            reg.Register<TimerActor>(timerActor)) |> ignore

    ) |> ignore
) |> ignore

let host = hostbuilder.Build()
host.RunAsync().Wait()