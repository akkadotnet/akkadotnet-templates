namespace AkkaStreamsTemplate

open Akka.Actor

[<AutoOpen>]
type TransformActor() as this =
    inherit ReceiveActor()    

    do
        this.Receive<string> (fun (message:string)-> 
                                    let actor = this :> IInternalActor             
                                    actor.ActorContext.Sender.Tell (message.ToUpper())                                    
                                    )