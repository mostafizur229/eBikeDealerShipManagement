namespace eBike.Application.Common.Interfaces;

public interface ICashDueService
{
    Task UpdateTotalDueAsync(int customerId, int supplierId, decimal amount, CancellationToken cancellationToken = default);
    Task UpdateTotalDueWhenEditAsync(int customerId, int supplierId, int cashCollectionId, decimal newAmount, CancellationToken cancellationToken = default);
}
