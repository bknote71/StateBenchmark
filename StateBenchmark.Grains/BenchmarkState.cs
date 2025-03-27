using Orleans;

namespace StateBenchmark.Grains;

[GenerateSerializer]
public class BenchmarkState
{
    [Id(0)]
    public string Value { get; set; }
    
    [Id(1)]
    public int Number { get; set; } 
}