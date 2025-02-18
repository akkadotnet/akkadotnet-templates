namespace AkkaConsoleTemplate

open Akka.Actor
open Akka.Hosting

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
        