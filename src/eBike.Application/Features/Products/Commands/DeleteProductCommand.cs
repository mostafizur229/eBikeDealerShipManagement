using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Products.Commands;

public record DeleteProductCommand(int Id) : IRequest;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteProductCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductID == request.Id, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), request.Id);
        }

        // Integrity check: Check if product has Purchase, Sales, Transfers, or Stock records
        using var connection = _sqlConnectionFactory.CreateConnection();
        var counts = await connection.QueryFirstOrDefaultAsync<(int PurchaseCount, int SalesCount, int TransferCount, int StockCount)>(@"
            SELECT 
                (SELECT COUNT(1) FROM POrderDetails WHERE ProductID = @id) +
                (SELECT COUNT(1) FROM POProductDetails WHERE ProductID = @id) AS PurchaseCount,
                (SELECT COUNT(1) FROM SOrderDetails WHERE ProductID = @id) +
                (SELECT COUNT(1) FROM CreditSaleDetails WHERE ProductID = @id) AS SalesCount,
                (SELECT COUNT(1) FROM TransferDetails WHERE ProductID = @id) +
                (SELECT COUNT(1) FROM TransferHistories WHERE ProductId = @id) AS TransferCount,
                (SELECT COUNT(1) FROM Stocks WHERE ProductID = @id) +
                (SELECT COUNT(1) FROM StockDetails WHERE ProductID = @id) AS StockCount;",
            new { id = request.Id });

        if (counts.PurchaseCount > 0)
        {
            throw new ConflictException($"Cannot delete product '{product.ProductName}' because it has {counts.PurchaseCount} associated Purchase Order record(s).");
        }

        if (counts.SalesCount > 0)
        {
            throw new ConflictException($"Cannot delete product '{product.ProductName}' because it has {counts.SalesCount} associated Sales Order record(s).");
        }

        if (counts.TransferCount > 0)
        {
            throw new ConflictException($"Cannot delete product '{product.ProductName}' because it has {counts.TransferCount} associated Stock Transfer record(s).");
        }

        if (counts.StockCount > 0)
        {
            throw new ConflictException($"Cannot delete product '{product.ProductName}' because it has active Stock / Inventory record(s).");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
