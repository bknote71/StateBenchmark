using Orleans;

namespace StateBenchmark.GrainInterfaces;

public interface ITransactionalStateBenchmarkGrain : IGrainWithStringKey 
{
    [Transaction(TransactionOption.Create)]
    Task SetValue(string value);
    
    [Transaction(TransactionOption.Create)]
    Task<string> GetValue();
}