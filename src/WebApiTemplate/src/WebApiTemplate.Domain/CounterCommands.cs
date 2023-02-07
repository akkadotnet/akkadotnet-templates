namespace WebApiTemplate.Domain;

/// <summary>
/// Defines a command that is related to a counter.
/// </summary>
public interface ICounterCommand : IWithCounterId
{
}

public sealed record IncrementCounterCommand(string CounterId, int Amount) : ICounterCommand;

public sealed record SetCounterCommand(string CounterId, int Value) : ICounterCommand;

public sealed record CounterCommandResponse
    (string CounterId, bool IsSuccess, ICounterEvent? Event = null, string? ErrorMessage = null) : ICounterCommand;
    
public sealed record SubscribeToCounter(string CounterId, IActorRef Subscriber) : ICounterCommand;

public sealed record UnsubscribeToCounter(string CounterId, IActorRef Subscriber) : ICounterCommand;