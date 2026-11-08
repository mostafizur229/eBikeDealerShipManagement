using System.Data;
using Dapper;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace eBike.Infrastructure.Services;

public class BankBalanceService : IBankBalanceService
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<BankBalanceService> _logger;

    public BankBalanceService(
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<BankBalanceService> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
    }

    public async Task UpdateBankBalanceAsync(BankTransaction transaction, bool isNewEntry, int userId, CancellationToken cancellationToken = default)
    {
        decimal amountSign = isNewEntry ? 1m : -1m;
        decimal amount = transaction.Amount * amountSign;
        int bankId = transaction.BankID ?? 0;
        int customerId = transaction.CustomerID ?? 0;
        int supplierId = transaction.SupplierID ?? 0;
        int anotherBankId = transaction.AnotherBankID ?? 0;
        int sihid = transaction.SIHID ?? 0;
        int expenseItemId = transaction.ExpenseItemID ?? 0;
        int concernId = transaction.ConcernID ?? 0;

        using var connection = _sqlConnectionFactory.CreateConnection();

        try
        {
            switch ((EnumTransactionType)transaction.TransactionType)
            {
                case EnumTransactionType.Deposit:
                    await connection.ExecuteAsync("UpdateTotalDue", new
                    {
                        CusId = 0,
                        SupId = 0,
                        BankDepositId = bankId,
                        BankWithdrawId = 0,
                        CollectionAmount = amount
                    }, commandType: CommandType.StoredProcedure);
                    break;

                case EnumTransactionType.Withdraw:
                    await connection.ExecuteAsync("UpdateTotalDue", new
                    {
                        CusId = 0,
                        SupId = 0,
                        BankDepositId = 0,
                        BankWithdrawId = bankId,
                        CollectionAmount = amount
                    }, commandType: CommandType.StoredProcedure);
                    break;

                case EnumTransactionType.CashCollection:
                    await connection.ExecuteAsync("UpdateTotalDue", new
                    {
                        CusId = customerId,
                        SupId = 0,
                        BankDepositId = bankId,
                        BankWithdrawId = 0,
                        CollectionAmount = amount
                    }, commandType: CommandType.StoredProcedure);
                    break;

                case EnumTransactionType.CashDelivery:
                    await connection.ExecuteAsync("UpdateTotalDue", new
                    {
                        CusId = 0,
                        SupId = supplierId,
                        BankDepositId = 0,
                        BankWithdrawId = bankId,
                        CollectionAmount = amount
                    }, commandType: CommandType.StoredProcedure);
                    break;

                case EnumTransactionType.FundTransfer:
                    await connection.ExecuteAsync("UpdateTotalDue", new
                    {
                        CusId = 0,
                        SupId = 0,
                        BankDepositId = anotherBankId,
                        BankWithdrawId = bankId,
                        CollectionAmount = amount
                    }, commandType: CommandType.StoredProcedure);
                    break;

                case EnumTransactionType.LiaRec:
                    await connection.ExecuteAsync("UpdateTotalDueForInvestment", new
                    {
                        SIHID = sihid,
                        SIHIdV2 = 0,
                        BankDepositId = bankId,
                        BankWithdrawId = 0,
                        CollectionAmount = amount
                    }, commandType: CommandType.StoredProcedure);
                    break;

                case EnumTransactionType.LiaPay:
                    await connection.ExecuteAsync("UpdateTotalDueForInvestment", new
                    {
                        SIHID = 0,
                        SIHIdV2 = sihid,
                        BankDepositId = 0,
                        BankWithdrawId = bankId,
                        CollectionAmount = amount
                    }, commandType: CommandType.StoredProcedure);
                    break;

                case EnumTransactionType.BankIncome:
                    await connection.ExecuteAsync("UpdateTotalDueForExpenditure", new
                    {
                        ExpenseItemID = expenseItemId,
                        BankDepositId = bankId,
                        BankWithdrawId = 0,
                        CollectionAmount = amount,
                        ConcernID = concernId,
                        userId = userId
                    }, commandType: CommandType.StoredProcedure);
                    break;

                case EnumTransactionType.BankExpense:
                    await connection.ExecuteAsync("UpdateTotalDueForExpenditure", new
                    {
                        ExpenseItemID = expenseItemId,
                        BankDepositId = 0,
                        BankWithdrawId = bankId,
                        CollectionAmount = amount,
                        ConcernID = concernId,
                        userId = userId
                    }, commandType: CommandType.StoredProcedure);
                    break;

                default:
                    _logger.LogWarning("Unhandled Bank TransactionType {Type} for BankTranID {ID}", transaction.TransactionType, transaction.BankTranID);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Stored Procedure for BankTransaction {ID}", transaction.BankTranID);
            throw;
        }
    }
}
