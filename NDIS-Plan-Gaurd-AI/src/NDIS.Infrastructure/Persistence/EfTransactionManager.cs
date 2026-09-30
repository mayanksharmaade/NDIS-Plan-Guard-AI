using NDIS.Application.Abstractions.Persistence;

namespace NDIS.Infrastructure.Persistence;

public sealed class EfTransactionManager
    : ITransactionManager
{
    private readonly AppDbContext _dbContext;

    public EfTransactionManager(
        AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await action(cancellationToken);
        }

        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync(
                    cancellationToken);

        try
        {
            var result =
                await action(cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }
}
