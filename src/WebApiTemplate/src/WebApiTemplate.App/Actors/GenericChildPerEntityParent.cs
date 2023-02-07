using Akka.Actor;
using Akka.Cluster.Sharding;

namespace WebApiTemplate.App.Actors;

/// <summary>
/// A generic "child per entity" parent actor.
/// </summary>
/// <remarks>
/// Intended for simplifying unit tests where we don't want to use Akka.Cluster.Sharding.
/// </remarks>
public sealed class GenericChildPerEntityParent : ReceiveActor
{
    public static Props Props(IMessageExtractor extractor, Func<string, Props> propsFactory)
    {
        ExtractEntityId realExtractor = message => (extractor.EntityId(message), extractor.EntityMessage(message)); 
        return Akka.Actor.Props.Create(() => new GenericChildPerEntityParent(realExtractor, propsFactory));
    }
    
    /*
     * Re-use Akka.Cluster.Sharding's infrastructure here to keep things simple.
     */
    private ExtractEntityId _extractor;
    private Func<string, Props> _propsFactory;

    public GenericChildPerEntityParent(ExtractEntityId extractor, Func<string, Props> propsFactory)
    {
        _extractor = extractor;
        _propsFactory = propsFactory;
        
        ReceiveAny(o =>
        {
            var result = extractor(o);
            if (!result.HasValue) return;
            var (id, msg) = result.Value;
            Context.Child(id).GetOrElse(() => Context.ActorOf(propsFactory(id), id)).Forward(msg);
        });
    }
}