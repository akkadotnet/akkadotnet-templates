namespace WebApiTemplate.Domain;

/// <summary>
/// Queries are similar to commands, but they have no side effects.
///
/// They are used to retrieve information from the actors.
/// </summary>
public interface ICounterQuery : IWithCounterId{ }

public sealed record FetchCounter(string CounterId) : ICounterQuery;