namespace NDIS.Application.Abstractions.Persistence;

public interface ITransactionManager
{
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default);
}
