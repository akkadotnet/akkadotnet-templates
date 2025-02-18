namespace AkkaConsoleTemplate

open Akka.Actor
open Akka.Event

[<AutoOpen>]
type HelloActor() as this =
    inherit ReceiveActor()

    let log = UntypedActor.Context.GetLogger()    
    let mutable helloCounter = 0

    do
        this.Receive<string> (fun message -> 
                                    log.Info (sprintf "%s %i" message helloCounter)
                                    helloCounter <- helloCounter + 1
                                    )

    