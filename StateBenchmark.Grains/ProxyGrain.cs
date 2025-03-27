using Orleans;
using Orleans.Transactions.Abstractions;
using StateBenchmark.GrainInterfaces;

namespace StateBenchmark.Grains;

public class ProxyGrain : Grain, IProxyGrain
{
    public async Task SetValue(string value, string mode)
    {
        if (mode == "persistent")
        {
            var grain = this.GrainFactory.GetGrain<IPersistentStateBenchmarkGrain>("0");
            await grain.SetValue(value);
        }
        else
        {
            var grain = this.GrainFactory.GetGrain<ITransactionalStateBenchmarkGrain>("0");
            await grain.SetValue(value);
        }
    }

    public async Task<string> GetValue(string mode)
    {
        if (mode == "persistent")
        {
            var grain = this.GrainFactory.GetGrain<IPersistentStateBenchmarkGrain>("0");
            return await grain.GetValue();
        }
        else
        {
            var grain = this.GrainFactory.GetGrain<ITransactionalStateBenchmarkGrain>("0");
            return await grain.GetValue();
        }
    }
}