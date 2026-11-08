using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Suppliers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Suppliers.Queries;

public record GetSuppliersQuery(
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<SupplierListDto>>;

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, List<SupplierListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetSuppliersQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<SupplierListDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        var targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.Suppliers
            .AsNoTracking()
            .Include(s => s.SisterConcern)
            .AsQueryable();

        if (targetConcernId.HasValue)
        {
            query = query.Where(s => s.ConcernID == targetConcernId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(s =>
                (s.Name != null && s.Name.ToLower().Contains(search)) ||
                (s.Code != null && s.Code.ToLower().Contains(search)) ||
                (s.ContactNo != null && s.ContactNo.ToLower().Contains(search)) ||
                (s.OwnerName != null && s.OwnerName.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(s => s.Code)
            .Select(s => new SupplierListDto
            {
                SupplierID = s.SupplierID,
                Code = s.Code,
                Name = s.Name,
                OwnerName = s.OwnerName,
                ContactNo = s.ContactNo,
                Address = s.Address,
                TotalDue = s.TotalDue,
                ConcernID = s.ConcernID,
                ConcernName = s.SisterConcern != null ? s.SisterConcern.Name : null,
                OpeningDue = s.OpeningDue
            })
            .ToListAsync(cancellationToken);
    }
}
