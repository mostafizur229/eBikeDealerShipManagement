using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Banks.Commands;

public record DeleteBankCommand(int Id) : IRequest;

public class DeleteBankCommandHandler : IRequestHandler<DeleteBankCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteBankCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteBankCommand request, CancellationToken cancellationToken)
    {
        var bank = await _context.Banks
            .FirstOrDefaultAsync(b => b.BankID == request.Id, cancellationToken);

        if (bank == null)
        {
            throw new NotFoundException(nameof(Bank), request.Id);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();
        var counts = await connection.QueryFirstOrDefaultAsync<(int TxCount, int CardSetupCount)>(@"
            SELECT 
                (SELECT COUNT(1) FROM BankTransactions WHERE BankID = @id) AS TxCount,
                (SELECT COUNT(1) FROM CardTypeSetups WHERE BankID = @id) AS CardSetupCount;",
            new { id = request.Id });

        if (counts.TxCount > 0)
        {
            throw new ConflictException($"Cannot delete bank '{bank.BankName}' because it has {counts.TxCount} associated transaction record(s).");
        }

        if (counts.CardSetupCount > 0)
        {
            throw new ConflictException($"Cannot delete bank '{bank.BankName}' because card type setup(s) are configured for this bank.");
        }

        _context.Banks.Remove(bank);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
