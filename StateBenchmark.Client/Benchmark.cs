using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using StateBenchmark.GrainInterfaces;

var host = Host.CreateDefaultBuilder()
    .UseOrleansClient(builder =>
    {
        builder
            .UseLocalhostClustering()
            .UseTransactions()
            .Configure<ClusterOptions>(options =>
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
    })
    .Build();
    

await host.StartAsync();
Console.WriteLine("Connected to Orleans.");

var client = host.Services.GetRequiredService<IClusterClient>();


var grainId = "test-1";
var persistentGrain = client.GetGrain<IPersistentStateBenchmarkGrain>(grainId);
var transactionalGrain = client.GetGrain<ITransactionalStateBenchmarkGrain>(grainId);


var bench1 = await Benchmark(client, "PersistentState", persistentGrain.SetValue,80);
var bench2 = await Benchmark(client, "TransactionalState", transactionalGrain.SetValue, 80);
Console.WriteLine(bench1);
Console.WriteLine(bench2);

var number = await persistentGrain.GetNumber();
Console.WriteLine($"number: {number}");

static async Task<string> Benchmark(IClusterClient client, string name, Func<string, Task> setter, int concurrency)
{
    Console.WriteLine($"Benchmarking {name} with {concurrency} concurrent workers for 10 seconds...");

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    var totalCount = 0;
    
    var tasks = Enumerable.Range(0, concurrency).Select(workerId => Task.Run(async () =>
    {
        var localCount = 0;
        var threadId = System.Environment.CurrentManagedThreadId;
        Console.WriteLine($"current thread id is {threadId} for {workerId}");
        
        while (!cts.Token.IsCancellationRequested)
        {
            await Task.Delay(1);
            
            // if (name == "PersistentState")
            // {
            //     var grain = client.GetGrain<IPersistentStateBenchmarkGrain>("test1");
            //     await grain.SetValue($"worker-{workerId}, value: {localCount++}");
            // }
            // else
            // {
            //     var grain = client.GetGrain<ITransactionalStateBenchmarkGrain>("test2");
            //     await grain.SetValue($"worker-{workerId}, value: {localCount++}");
            // }
            var grain = client.GetGrain<IProxyGrain>(workerId);
            // await setter($"worker-{workerId}, value: {localCount++}");
            if (name == "PersistentState")
            {
                await grain.SetValue($"worker-{workerId}, value: {localCount++}", "persistent");
            }
            else
            {
                await grain.SetValue($"worker-{workerId}, value: {localCount++}", "transactional");
            }
        }
        
        Interlocked.Add(ref totalCount, localCount);
    })).ToArray();

    var sw = Stopwatch.StartNew();
    await Task.WhenAll(tasks);
    sw.Stop();

    return $"{name}: {totalCount} operations in {sw.Elapsed.TotalSeconds:F1} sec → {(totalCount / sw.Elapsed.TotalSeconds):F2} ops/sec";
}

// BenchmarkV2: 총 요청 처리 시간
static async Task BenchmarkV2(string name, Func<int, Task> setter, int totalRequests)
{
    Console.WriteLine($"Benchmarking {name} with {totalRequests} total requests");

    var sw = Stopwatch.StartNew();

    var tasks = Enumerable.Range(0, totalRequests)
        .Select(setter)
        .ToArray();

    await Task.WhenAll(tasks);

    sw.Stop();

    Console.WriteLine($"{name}: {totalRequests} requests completed in {sw.Elapsed.TotalSeconds:F2} sec → {(totalRequests / sw.Elapsed.TotalSeconds):F2} req/sec");
}

// Test
static async Task Benchmark2(string name, Func<int, Task> func, int concurrency)
{
    Console.WriteLine($"Benchmarking {name} with {concurrency} concurrent workers for 10 seconds...");

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    var totalCount = 0;
    
    var tasks = Enumerable.Range(0, concurrency).Select(workerId => Task.Run(async () =>
    {
        Console.WriteLine($"worker id: {workerId} started");
        while (!cts.Token.IsCancellationRequested)
        {
            // await Task.Delay(1);
            await func(workerId);
            int x = (int)1e6 * 2;
            Interlocked.Add(ref totalCount, x);
        }
        
        Console.WriteLine($"worker id: {workerId} done.");
    })).ToArray();

    var sw = Stopwatch.StartNew();
    await Task.WhenAll(tasks);
    sw.Stop();

    Console.WriteLine($"{name}: {totalCount} operations in {sw.Elapsed.TotalSeconds:F1} sec → {(totalCount / sw.Elapsed.TotalSeconds):F2} ops/sec");
}