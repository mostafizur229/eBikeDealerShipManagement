using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.VatSetup.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.VatSetup.Queries;

public record GetVatSetupQuery(int? ConcernId = null) : IRequest<VatSetupDto>;

public class GetVatSetupQueryHandler : IRequestHandler<GetVatSetupQuery, VatSetupDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetVatSetupQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VatSetupDto> Handle(GetVatSetupQuery request, CancellationToken cancellationToken)
    {
        var targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var concern = await _context.SisterConcerns
            .Where(sc => targetConcernId == null || targetConcernId == 0 || sc.ConcernID == targetConcernId)
            .OrderBy(sc => sc.ConcernID)
            .FirstOrDefaultAsync(cancellationToken);

        if (concern == null)
        {
            throw new NotFoundException("SisterConcern", targetConcernId?.ToString() ?? "Default");
        }

        return new VatSetupDto
        {
            ConcernID = concern.ConcernID,
            Name = concern.Name,
            Address = concern.Address,
            ContactNo = concern.ContactNo,
            SalesShowPercent = concern.SalesShowPercent,
            PurchaseShowPercent = concern.PurchaseShowPercent,
            StockShowPercent = concern.StockShowPercent
        };
    }
}
