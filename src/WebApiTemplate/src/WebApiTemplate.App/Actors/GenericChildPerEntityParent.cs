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
        return Akka.Actor.Props.Create(() => new GenericChildPerEntityParent(extractor, propsFactory));
    }
    
    /*
     * Re-use Akka.Cluster.Sharding's infrastructure here to keep things simple.
     */
    private readonly IMessageExtractor _extractor;
    private Func<string, Props> _propsFactory;

    public GenericChildPerEntityParent(IMessageExtractor extractor, Func<string, Props> propsFactory)
    {
        _extractor = extractor;
        _propsFactory = propsFactory;
        
        ReceiveAny(o =>
        {
            var entityId = _extractor.EntityId(o);
            if (string.IsNullOrEmpty(entityId)) 
                return;
            Context.Child(entityId).GetOrElse(() => Context.ActorOf(propsFactory(entityId), entityId))
                .Forward(_extractor.EntityMessage(o));
        });
    }
}