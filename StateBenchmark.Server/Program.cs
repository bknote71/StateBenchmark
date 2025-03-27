using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Storage;
using Orleans.Transactions.Abstractions;
using StateBenchmark.Server;

var builder = Host.CreateDefaultBuilder()
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering();
        
        // storage
        var persistentStorage = "persistentStateStorage";
        var transactionalStorage = "transactionalStateStorage";
        
        silo.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<IGrainStorage>(
                persistentStorage,
                (sp, key) => new SimplePersistentStateStorage());
            
            services.AddKeyedSingleton<ITransactionalStateStorageFactory>(
                transactionalStorage, 
                (sp, key) => SimpleTransactionalStateStorageFactory.Create(sp, key as string));
        });
        
        silo.UseTransactions();
        
        silo.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "dev";
            options.ServiceId = "OrleansStatePerfTest";
        });
    })
    .ConfigureServices(services =>
    {
        services.AddOpenTelemetry()
            .WithTracing(tracer =>
            {
                tracer.SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: "Benchmark", serviceVersion: "1.0"));

                tracer.AddSource("Microsoft.Orleans.Runtime");
                tracer.AddSource("Microsoft.Orleans.Application");
                tracer.AddSource("Microsoft.AspNetCore")
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4317");
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            });
    });

await builder.Build().RunAsync();