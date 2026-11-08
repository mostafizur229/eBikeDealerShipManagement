using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Customers.Commands;

public record DeleteCustomerCommand(int Id) : IRequest;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteCustomerCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerID == request.Id, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), request.Id);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();
        var counts = await connection.QueryFirstOrDefaultAsync<(
            int SalesCount,
            int CreditSalesCount,
            int ReturnSalesCount,
            int CashTxCount,
            int BankTxCount,
            int DOCount,
            int BookingCount,
            int JobCardCount,
            int PartsBookingCount,
            int VehicleCount)>(@"
            SELECT 
                (SELECT COUNT(1) FROM SOrders WHERE CustomerID = @id) AS SalesCount,
                (SELECT COUNT(1) FROM CreditSales WHERE CustomerID = @id) AS CreditSalesCount,
                (SELECT COUNT(1) FROM ROrders WHERE CustomerID = @id) AS ReturnSalesCount,
                (SELECT COUNT(1) FROM CashCollections WHERE CustomerID = @id) AS CashTxCount,
                (SELECT COUNT(1) FROM BankTransactions WHERE CustomerID = @id) AS BankTxCount,
                (SELECT COUNT(1) FROM DOes WHERE CustomerID = @id) AS DOCount,
                (SELECT COUNT(1) FROM BikeBookings WHERE CustomerID = @id) AS BookingCount,
                (SELECT COUNT(1) FROM JobCards WHERE CustomerID = @id) AS JobCardCount,
                (SELECT COUNT(1) FROM PartsBookings WHERE CustomerID = @id) AS PartsBookingCount,
                (SELECT COUNT(1) FROM CustomerVehicles WHERE CustomerID = @id) AS VehicleCount;",
            new { id = request.Id });

        if (counts.SalesCount > 0 || counts.CreditSalesCount > 0 || counts.ReturnSalesCount > 0)
        {
            int totalSales = counts.SalesCount + counts.CreditSalesCount + counts.ReturnSalesCount;
            throw new ConflictException($"Cannot delete customer '{customer.Name}' because they have {totalSales} associated sales/order record(s).");
        }

        if (counts.CashTxCount > 0 || counts.BankTxCount > 0)
        {
            int totalTx = counts.CashTxCount + counts.BankTxCount;
            throw new ConflictException($"Cannot delete customer '{customer.Name}' because they have {totalTx} associated payment/collection transaction(s).");
        }

        if (counts.DOCount > 0)
        {
            throw new ConflictException($"Cannot delete customer '{customer.Name}' because they have {counts.DOCount} associated Delivery Order record(s).");
        }

        if (counts.BookingCount > 0 || counts.JobCardCount > 0 || counts.PartsBookingCount > 0 || counts.VehicleCount > 0)
        {
            throw new ConflictException($"Cannot delete customer '{customer.Name}' because service/booking records or vehicle(s) are registered under this customer.");
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
