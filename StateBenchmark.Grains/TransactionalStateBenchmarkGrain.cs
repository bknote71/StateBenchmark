using Orleans;
using Orleans.Concurrency;
using Orleans.Transactions.Abstractions;
using StateBenchmark.GrainInterfaces;

namespace StateBenchmark.Grains;

public class TransactionalStateBenchmarkGrain : Grain, ITransactionalStateBenchmarkGrain
{
    private readonly ITransactionalState<BenchmarkState> _state;

    public TransactionalStateBenchmarkGrain([TransactionalState("state", "transactionalStateStorage")] ITransactionalState<BenchmarkState> state)
    {
        this._state = state;
    }
    
    public async Task SetValue(string value)
    {
        await this._state.PerformUpdate(state =>
        {
            state.Value = value;
        });
    }

    public async Task<string> GetValue()
    {
        return await this._state.PerformRead(state => state.Value);
    }
}