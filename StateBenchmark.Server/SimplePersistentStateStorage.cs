using Orleans;
using Orleans.Runtime;
using Orleans.Storage;

namespace StateBenchmark.Server;

public class SimplePersistentStateStorage : IGrainStorage
{
    public Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        return Task.CompletedTask;
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        await Task.Delay(40);
    }

    public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        return Task.CompletedTask;
    }
}