using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Suppliers.Commands;

public record DeleteSupplierCommand(int Id) : IRequest;

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteSupplierCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierID == request.Id, cancellationToken);

        if (supplier == null)
        {
            throw new NotFoundException(nameof(Supplier), request.Id);
        }

        // Integrity check: Check if supplier has Purchase Orders, DOs, or Transactions
        using var connection = _sqlConnectionFactory.CreateConnection();
        var counts = await connection.QueryFirstOrDefaultAsync<(int PurchaseCount, int DOCount, int CashTxCount, int BankTxCount, int PriceProtCount)>(@"
            SELECT 
                (SELECT COUNT(1) FROM POrders WHERE SupplierID = @id) AS PurchaseCount,
                (SELECT COUNT(1) FROM DOes WHERE SupplierID = @id) AS DOCount,
                (SELECT COUNT(1) FROM CashCollections WHERE SupplierID = @id) AS CashTxCount,
                (SELECT COUNT(1) FROM BankTransactions WHERE SupplierID = @id) AS BankTxCount,
                (SELECT COUNT(1) FROM PriceProtections WHERE SupplierID = @id) AS PriceProtCount;",
            new { id = request.Id });

        if (counts.PurchaseCount > 0)
        {
            throw new ConflictException($"Cannot delete supplier '{supplier.Name}' because it has {counts.PurchaseCount} associated Purchase Order record(s).");
        }

        if (counts.DOCount > 0)
        {
            throw new ConflictException($"Cannot delete supplier '{supplier.Name}' because it has {counts.DOCount} associated Delivery Order record(s).");
        }

        if (counts.CashTxCount > 0 || counts.BankTxCount > 0)
        {
            int totalTx = counts.CashTxCount + counts.BankTxCount;
            throw new ConflictException($"Cannot delete supplier '{supplier.Name}' because it has {totalTx} associated Payment/Transaction record(s).");
        }

        if (counts.PriceProtCount > 0)
        {
            throw new ConflictException($"Cannot delete supplier '{supplier.Name}' because it has {counts.PriceProtCount} associated Price Protection record(s).");
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
