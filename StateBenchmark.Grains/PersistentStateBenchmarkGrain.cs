using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using StateBenchmark.GrainInterfaces;

namespace StateBenchmark.Grains;

public class PersistentStateBenchmarkGrain : Grain, IPersistentStateBenchmarkGrain
{
    private readonly IPersistentState<BenchmarkState> _state;
    private static int staticnumber = 0;
    
    public PersistentStateBenchmarkGrain([PersistentState("state", "persistentStateStorage")] IPersistentState<BenchmarkState> state)
    {
        this._state = state;
    }
    public async Task SetValue(string value)
    {
        this._state.State.Value = value;
        await this._state.WriteStateAsync();
    }

    public async Task IncreaseNumber(int workerId)
    {
        for (int i = 0; i < 1e6; i++)
        {
            staticnumber++;
            this._state.State.Number++;

            if (i % 100000 == 0)
            {
                Console.WriteLine($"increased number {workerId}");
            }
        }
        await this._state.WriteStateAsync();
        
        for (int i = 0; i < 1e6; i++)
        {
            staticnumber++;
            this._state.State.Number++;

            if (i % 100000 == 0)
            {
                Console.WriteLine($"increased number {workerId}");
            }
        }
        
        await this._state.WriteStateAsync();
        
        Console.WriteLine($"worker ({workerId}) has been done.");
    }

    public Task<string> GetValue()
    {
        return Task.FromResult(this._state.State.Value);
    }

    public Task<int> GetNumber()
    {
        Console.WriteLine($"static number {staticnumber}");
        return Task.FromResult(this._state.State.Number);
    }
}