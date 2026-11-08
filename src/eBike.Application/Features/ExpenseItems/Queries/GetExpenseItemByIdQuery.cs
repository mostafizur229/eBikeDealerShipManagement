using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.ExpenseItems.DTOs;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ExpenseItems.Queries;

public record GetExpenseItemByIdQuery(int Id) : IRequest<ExpenseItemDto>;

public class GetExpenseItemByIdQueryHandler : IRequestHandler<GetExpenseItemByIdQuery, ExpenseItemDto>
{
    private readonly IApplicationDbContext _context;

    public GetExpenseItemByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseItemDto> Handle(GetExpenseItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.ExpenseItems
            .AsNoTracking()
            .Include(e => e.SisterConcern)
            .FirstOrDefaultAsync(e => e.ExpenseItemID == request.Id, cancellationToken);

        if (item == null)
        {
            throw new NotFoundException(nameof(ExpenseItem), request.Id);
        }

        return new ExpenseItemDto
        {
            ExpenseItemID = item.ExpenseItemID,
            Code = item.Code,
            Description = item.Description,
            Status = item.Status,
            StatusName = ((EnumCompanyTransaction)item.Status).ToString(),
            ConcernID = item.ConcernID,
            ConcernName = item.SisterConcern?.Name
        };
    }
}
