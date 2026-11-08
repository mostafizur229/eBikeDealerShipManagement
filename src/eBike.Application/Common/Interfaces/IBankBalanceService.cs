using eBike.Domain.Entities;

namespace eBike.Application.Common.Interfaces;

public interface IBankBalanceService
{
    Task UpdateBankBalanceAsync(BankTransaction transaction, bool isNewEntry, int userId, CancellationToken cancellationToken = default);
}
