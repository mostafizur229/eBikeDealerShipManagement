using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Expenditures.DTOs;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Expenditures.Queries;

public record GetExpenditureByIdQuery(int Id) : IRequest<ExpenditureDto>;

public class GetExpenditureByIdQueryHandler : IRequestHandler<GetExpenditureByIdQuery, ExpenditureDto>
{
    private readonly IApplicationDbContext _context;

    public GetExpenditureByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenditureDto> Handle(GetExpenditureByIdQuery request, CancellationToken cancellationToken)
    {
        var e = await _context.Expenditures
            .AsNoTracking()
            .Include(x => x.ExpenseItem)
            .Include(x => x.SisterConcern)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.ExpenditureID == request.Id, cancellationToken);

        if (e == null)
        {
            throw new NotFoundException(nameof(Expenditure), request.Id);
        }

        return new ExpenditureDto
        {
            ExpenditureID = e.ExpenditureID,
            VoucherNo = e.VoucherNo,
            EntryDate = e.EntryDate,
            Amount = e.Amount,
            Purpose = e.Purpose,
            ExpenseItemID = e.ExpenseItemID,
            ExpenseItemName = e.ExpenseItem?.Description,
            ExpenseItemCode = e.ExpenseItem?.Code,
            ExpenseItemStatus = e.ExpenseItem?.Status ?? 1,
            ExpenseItemStatusName = e.ExpenseItem != null ? ((EnumCompanyTransaction)e.ExpenseItem.Status).ToString() : "Expense",
            CustomerType = e.CustomerType,
            Remarks = e.Remarks,
            WFStatus = e.WFStatus,
            WFStatusName = ((EnumWFStatus)e.WFStatus).ToString(),
            ConcernID = e.ConcernID,
            ConcernName = e.SisterConcern?.Name,
            CreatedBy = e.CreatedBy,
            CreatedByName = e.CreatedByUser?.UserName,
            CreateDate = e.CreateDate
        };
    }
}
