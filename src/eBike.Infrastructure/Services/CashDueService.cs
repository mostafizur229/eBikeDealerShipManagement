using System.Data;
using Dapper;
using eBike.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace eBike.Infrastructure.Services;

public class CashDueService : ICashDueService
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<CashDueService> _logger;

    public CashDueService(
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<CashDueService> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
    }

    public async Task UpdateTotalDueAsync(int customerId, int supplierId, decimal amount, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        try
        {
            await connection.ExecuteAsync("UpdateTotalDue", new
            {
                CusId = customerId,
                SupId = supplierId,
                BankDepositId = 0,
                BankWithdrawId = 0,
                CollectionAmount = amount
            }, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing UpdateTotalDue SP for CustomerId: {CustomerId}, SupplierId: {SupplierId}, Amount: {Amount}",
                customerId, supplierId, amount);
            throw;
        }
    }

    public async Task UpdateTotalDueWhenEditAsync(int customerId, int supplierId, int cashCollectionId, decimal newAmount, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        try
        {
            await connection.ExecuteAsync("UpdateTotalDueWhenEdit", new
            {
                CusId = customerId,
                SupId = supplierId,
                CashCollectionID = cashCollectionId,
                NewCollectionAmount = newAmount
            }, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing UpdateTotalDueWhenEdit SP for CustomerId: {CustomerId}, SupplierId: {SupplierId}, CashCollectionId: {CashCollectionId}, NewAmount: {NewAmount}",
                customerId, supplierId, cashCollectionId, newAmount);
            throw;
        }
    }
}
