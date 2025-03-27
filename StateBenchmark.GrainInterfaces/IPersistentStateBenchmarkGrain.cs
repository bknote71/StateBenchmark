using Orleans;
using Orleans.Concurrency;

namespace StateBenchmark.GrainInterfaces;

public interface IPersistentStateBenchmarkGrain : IGrainWithStringKey 
{
    Task SetValue(string value);
    Task<string> GetValue();
    
    Task IncreaseNumber(int workerId);
    Task<int> GetNumber();   
}