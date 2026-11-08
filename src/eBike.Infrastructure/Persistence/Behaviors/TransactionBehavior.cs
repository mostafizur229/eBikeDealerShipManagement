using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Infrastructure.Persistence.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionBehavior(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Don't wrap queries in transactions
        var requestName = typeof(TRequest).Name;
        if (requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
        {
            return await next();
        }

        // Check if there is already an active transaction
        if (_dbContext.Database.CurrentTransaction != null)
        {
            return await next();
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var response = await next();
                await transaction.CommitAsync(cancellationToken);
                return response;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
