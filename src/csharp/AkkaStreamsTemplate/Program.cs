using Akka.Hosting;
using Akka.Streams;
using AkkaStreamsTemplate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var hostBuilder = new HostBuilder();

hostBuilder.ConfigureServices((context, services) =>
{
    services.AddAkka("MyActorSystem", (builder, sp) =>
    {
        builder
            .WithActors((system, registry, resolver) =>
            {
                var helloActor = system.ActorOf(Props.Create(() => new TransformerActor()), "transformer");
                registry.Register<TransformerActor>(helloActor);
            });
    });
});

var host = hostBuilder.Build();

var completionTask = host.RunAsync();

// grab the ActorSystem from the DI container
var system = host.Services.GetRequiredService<ActorSystem>();

// grab the ActorRef from the DI container
IActorRef transformer = host.Services.GetRequiredService<IRequiredActor<TransformerActor>>().ActorRef;

// create a stream that iterates over the numbers 1 to 100
await Source.From(Enumerable.Range(1, 1000))
    .Where(i => i % 2 == 0) // even numbers only
    .Select(i => i.ToString()) // convert to string
    .Throttle(10, TimeSpan.FromSeconds(1), 10, ThrottleMode.Shaping) // throttle stream to 10 elements per second
    .SelectAsync(5, async str => // invoke actor, up to 5 times in parallel, to convert string
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        return await transformer.Ask<string>(str, cts.Token);
    })
    .RunForeach(Console.WriteLine, system); // write all output to console
    
await completionTask; // wait for the host to shut down