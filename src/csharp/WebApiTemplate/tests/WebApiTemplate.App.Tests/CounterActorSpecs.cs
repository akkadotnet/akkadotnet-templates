using Akka.Hosting;
using Akka.Hosting.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApiTemplate.App.Actors;
using WebApiTemplate.App.Configuration;
using WebApiTemplate.Domain;
using Xunit.Abstractions;

namespace WebApiTemplate.App.Tests;

public class CounterActorSpecs : TestKit
{
    public CounterActorSpecs(ITestOutputHelper output) : base(output:output)
    {
    }
    
    [Fact]
    public void CounterActor_should_follow_Protocol()
    {
        // arrange (counter actor parent is already running)
        var counterActor = ActorRegistry.Get<CounterActor>();
        var counterId1 = "counterId";
        var counter1Messages = new IWithCounterId[]
        {
            new SetCounterCommand(counterId1, 3),
            new IncrementCounterCommand(counterId1, 10),
            new IncrementCounterCommand(counterId1, -5),
            new IncrementCounterCommand(counterId1, 2),
      
            new FetchCounter(counterId1)
        };

        // act

        foreach (var msg in counter1Messages)
        {
            counterActor.Tell(msg, TestActor);
        }

        // assert
        var counter = (Counter)FishForMessage(c => c is Counter);
        counter.CounterId.Should().Be(counterId1);
        counter.CurrentValue.Should().Be(3+10-5+2);
    }

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var settings = new AkkaSettings() { UseClustering = false, PersistenceMode = PersistenceMode.InMemory };
        services.AddSingleton(settings);
        base.ConfigureServices(context, services);
    }

    protected override void ConfigureAkka(AkkaConfigurationBuilder builder, IServiceProvider provider)
    {
        builder.ConfigureCounterActors(provider).ConfigurePersistence(provider);
    }
}