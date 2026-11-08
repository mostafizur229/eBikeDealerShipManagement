using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ExpenseItems.Commands;

public record DeleteExpenseItemCommand(int Id) : IRequest;

public class DeleteExpenseItemCommandHandler : IRequestHandler<DeleteExpenseItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteExpenseItemCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteExpenseItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.ExpenseItems
            .FirstOrDefaultAsync(e => e.ExpenseItemID == request.Id, cancellationToken);

        if (item == null)
        {
            throw new NotFoundException(nameof(ExpenseItem), request.Id);
        }

        // Integrity check: Check if head is used in Expenditures or BankTransactions
        using var connection = _sqlConnectionFactory.CreateConnection();
        var counts = await connection.QueryFirstOrDefaultAsync<(int ExpenditureCount, int BankTxCount)>(@"
            SELECT 
                (SELECT COUNT(1) FROM Expenditures WHERE ExpenseItemID = @id) AS ExpenditureCount,
                (SELECT COUNT(1) FROM BankTransactions WHERE ExpenseItemID = @id) AS BankTxCount;",
            new { id = request.Id });

        if (counts.ExpenditureCount > 0)
        {
            throw new ConflictException($"Cannot delete Head '{item.Description}' because it is associated with {counts.ExpenditureCount} expense/income voucher(s).");
        }

        if (counts.BankTxCount > 0)
        {
            throw new ConflictException($"Cannot delete Head '{item.Description}' because it is associated with {counts.BankTxCount} bank transaction record(s).");
        }

        _context.ExpenseItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
