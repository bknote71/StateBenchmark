using Orleans;

namespace StateBenchmark.GrainInterfaces;

public interface IProxyGrain : IGrainWithIntegerKey
{
    Task SetValue(string value, string mode);
    Task<string> GetValue(string mode);
}