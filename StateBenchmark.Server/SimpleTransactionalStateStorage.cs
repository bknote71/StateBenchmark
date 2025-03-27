using Orleans.Transactions.Abstractions;

namespace StateBenchmark.Server;

public class SimpleTransactionalStateStorage<TState>: ITransactionalStateStorage<TState> where TState : class, new()
{
    public Task<TransactionalStorageLoadResponse<TState>> Load()
    {
        return Task.FromResult(new TransactionalStorageLoadResponse<TState>());
    }

    public async Task<string> Store(string expectedETag, TransactionalStateMetaData metadata, List<PendingTransactionState<TState>> statesToPrepare, long? commitUpTo,
        long? abortAfter)
    {
        await Task.Delay(40);
        return string.Empty;
    }
}