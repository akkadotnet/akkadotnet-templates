using Akka.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// The Discovery template symbol selects the backend on instantiation (redis | azure).
// Only the chosen backend's code + package is compiled.
var akka = builder.AddAkka("sample-cluster");

#if REDIS
var redis = builder.AddRedis("akka-discovery");
akka = akka.WithClustering(redis);
#elif AZURE
var storage = builder.AddAzureStorage("azure-storage").RunAsEmulator();
var tables = storage.AddTables("akka-discovery");
akka = akka.WithClustering(tables);
#endif

builder.AddProject<Projects.WebApiTemplate_App>("app")
    .WithHttpEndpoint(name: "http")
    .WithReplicas(3)
    .WithReference(akka);

builder.Build().Run();
