namespace WebApiTemplate.Domain;

/// <summary>
/// Events are facts of the system. Counter events deal in definitive state changes with the counter.
/// </summary>
public interface ICounterEvent : IWithCounterId
{
}

public sealed record CounterValueIncremented(string CounterId, int Amount, int NewValue) : ICounterEvent;

public sealed record CounterValueSet(string CounterId, int NewValue) : ICounterEvent;