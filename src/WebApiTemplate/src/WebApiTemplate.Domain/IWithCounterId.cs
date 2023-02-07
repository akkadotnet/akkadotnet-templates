namespace WebApiTemplate.Domain;

/// <summary>
/// Counters are the only entities that have a counter id.
///
/// All messages decorated with this interface belong to a specific counter.
/// </summary>
public interface IWithCounterId
{
    string CounterId { get; }
}