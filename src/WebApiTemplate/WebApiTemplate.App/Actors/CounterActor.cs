using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using WebApiTemplate.Domain;

namespace WebApiTemplate.App.Actors;

public record Counter(string CounterId, int CurrentValue)
{
}

public static class CounterExtensions
{
    public static CounterCommandResponse ProcessCommand(this Counter counter, ICounterCommand command)
    {
        return command switch
        {
            IncrementCounterCommand increment => new CounterCommandResponse(counter.CounterId, true,
                new CounterValueIncremented(counter.CounterId, increment.Amount,
                    increment.Amount + counter.CurrentValue)),
            SetCounterCommand set => new CounterCommandResponse(counter.CounterId, true,
                new CounterValueSet(counter.CounterId, set.Value)),
            _ => throw new InvalidOperationException($"Unknown command type: {command.GetType().Name}")
        };
    }
    
    public static Counter ApplyEvent(this Counter counter, ICounterEvent @event)
    {
        return @event switch
        {
            CounterValueIncremented increment => counter with {CurrentValue = increment.NewValue},
            CounterValueSet set => counter with {CurrentValue = set.NewValue},
            _ => throw new InvalidOperationException($"Unknown event type: {@event.GetType().Name}")
        };
    }
}

public sealed class CounterActor : ReceivePersistentActor
{
    // currently, do not persist subscribers, but would be easy to add
    private readonly HashSet<IActorRef> _subscribers = new();
    private Counter _counter;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public CounterActor(string counterName)
    {
        // distinguish both type and entity Id in the EventJournal
        PersistenceId = $"Counter_{counterName}";
        _counter = new Counter(counterName, 0);


        Recover<SnapshotOffer>(offer =>
        {
            if (offer.Snapshot is Counter c)
            {
                _counter = c;
                _log.Info("Recovered initial count value of [{0}]", c);
            }
        });

        Recover<ICounterEvent>(@event =>
        {
            _counter = _counter.ApplyEvent(@event);
        });

        Command<FetchCounter>(f => Sender.Tell(_counter));

        Command<SubscribeToCounter>(subscribe =>
        {
            _subscribers.Add(subscribe.Subscriber);
            Sender.Tell(new CounterCommandResponse(_counter.CounterId, true));
            Context.Watch(subscribe.Subscriber);
        });

        Command<UnsubscribeToCounter>(counter =>
        {
            Context.Unwatch(counter.Subscriber);
            _subscribers.Remove(counter.Subscriber);
        });

        Command<ICounterCommand>(cmd =>
        {
            var response = _counter.ProcessCommand(cmd);

            if (!response.IsSuccess)
            {
                Sender.Tell(response);
                return;
            }

            if (response.Event != null) // only persist if there is an event to persist
            {
                Persist(response.Event, @event =>
                {
                    _counter = _counter.ApplyEvent(@event);
                    _log.Info("Updated counter via {0} - new value is {1}", @event, _counter.CurrentValue);
                    Sender.Tell(response);
                    
                    // push events to all subscribers
                    foreach (var s in _subscribers)
                    {
                        s.Tell(@event);
                    }
                    SaveSnapshotWhenAble();
                });
            }
        });

        Command<SaveSnapshotSuccess>(success =>
        {
            // delete all older snapshots (but leave journal intact, in case we want to do projections with that data)
            DeleteSnapshots(new SnapshotSelectionCriteria(success.Metadata.SequenceNr - 1));
        });
    }

    private void SaveSnapshotWhenAble()
    {
        // save a new snapshot every 25 events, in order to keep recovery times bounded
        if (LastSequenceNr % 25 == 0)
        {
            SaveSnapshot(_counter);
        }
    }

    public override string PersistenceId { get; }
}