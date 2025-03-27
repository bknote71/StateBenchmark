using Orleans.Runtime;
using Orleans.Transactions.Abstractions;

namespace StateBenchmark.Server;

public class SimpleTransactionalStateStorageFactory : ITransactionalStateStorageFactory
{
    public static ITransactionalStateStorageFactory Create(IServiceProvider services, string name)
    {
        return new SimpleTransactionalStateStorageFactory();    
    }
    
    public ITransactionalStateStorage<TState> Create<TState>(string stateName, IGrainContext context) where TState : class, new()
    {
        return new SimpleTransactionalStateStorage<TState>();
    }
}
