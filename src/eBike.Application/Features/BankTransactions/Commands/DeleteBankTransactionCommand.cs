using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.BankTransactions.Commands;

public record DeleteBankTransactionCommand(int Id) : IRequest;

public class DeleteBankTransactionCommandHandler : IRequestHandler<DeleteBankTransactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IBankBalanceService _bankBalanceService;

    public DeleteBankTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IBankBalanceService bankBalanceService)
    {
        _context = context;
        _currentUser = currentUser;
        _bankBalanceService = bankBalanceService;
    }

    public async Task Handle(DeleteBankTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.BankTransactions
            .FirstOrDefaultAsync(t => t.BankTranID == request.Id, cancellationToken);

        if (transaction == null)
        {
            throw new NotFoundException(nameof(BankTransaction), request.Id);
        }

        int userId = _currentUser.UserId ?? 1;
        await _bankBalanceService.UpdateBankBalanceAsync(transaction, isNewEntry: false, userId, cancellationToken);

        _context.BankTransactions.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
