using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SaleOffers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SaleOffers.Queries;

public record GetSaleOffersQuery(string? Search = null, int? ProductId = null) : IRequest<List<SaleOfferListDto>>;

public class GetSaleOffersQueryHandler : IRequestHandler<GetSaleOffersQuery, List<SaleOfferListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSaleOffersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SaleOfferListDto>> Handle(GetSaleOffersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SaleOffers
            .Include(o => o.Product)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(o => (o.OfferCode != null && o.OfferCode.ToLower().Contains(search))
                                  || o.Description.ToLower().Contains(search)
                                  || (o.Product != null && o.Product.ProductName.ToLower().Contains(search)));
        }

        if (request.ProductId.HasValue && request.ProductId > 0)
        {
            query = query.Where(o => o.ProductID == request.ProductId.Value);
        }

        var items = await query.OrderByDescending(o => o.OfferID).ToListAsync(cancellationToken);

        return items.Select(o => new SaleOfferListDto
        {
            OfferID = o.OfferID,
            OfferCode = o.OfferCode,
            ProductID = o.ProductID,
            ProductName = o.Product?.ProductName,
            FromDate = o.FromDate,
            ToDate = o.ToDate,
            Description = o.Description,
            OfferValue = o.OfferValue,
            OfferType = o.OfferType,
            OfferTypeName = o.OfferType switch
            {
                1 => "PCS",
                2 => "Flat Amount",
                3 => "Total Value %",
                _ => "Other"
            },
            ThresholdValue = o.ThresholdValue,
            Status = o.Status,
            StatusName = o.Status == 2 ? "Freeze" : "Ongoing"
        }).ToList();
    }
}

public record GetSaleOfferByIdQuery(int Id) : IRequest<SaleOfferDto>;

public class GetSaleOfferByIdQueryHandler : IRequestHandler<GetSaleOfferByIdQuery, SaleOfferDto>
{
    private readonly IApplicationDbContext _context;

    public GetSaleOfferByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SaleOfferDto> Handle(GetSaleOfferByIdQuery request, CancellationToken cancellationToken)
    {
        var o = await _context.SaleOffers
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.OfferID == request.Id, cancellationToken);

        if (o == null)
        {
            throw new NotFoundException("SaleOffer", request.Id.ToString());
        }

        return new SaleOfferDto
        {
            OfferID = o.OfferID,
            OfferCode = o.OfferCode,
            ProductID = o.ProductID,
            ProductName = o.Product?.ProductName,
            FromDate = o.FromDate,
            ToDate = o.ToDate,
            Description = o.Description,
            OfferValue = o.OfferValue,
            OfferType = o.OfferType,
            OfferTypeName = o.OfferType switch
            {
                1 => "PCS",
                2 => "Flat Amount",
                3 => "Total Value %",
                _ => "Other"
            },
            ThresholdValue = o.ThresholdValue,
            Status = o.Status,
            StatusName = o.Status == 2 ? "Freeze" : "Ongoing",
            ConcernID = o.ConcernID
        };
    }
}

public record GetNextSaleOfferCodeQuery() : IRequest<string>;

public class GetNextSaleOfferCodeQueryHandler : IRequestHandler<GetNextSaleOfferCodeQuery, string>
{
    private readonly IApplicationDbContext _context;

    public GetNextSaleOfferCodeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GetNextSaleOfferCodeQuery request, CancellationToken cancellationToken)
    {
        var count = await _context.SaleOffers.CountAsync(cancellationToken);
        return $"OFR-{(count + 1):D4}";
    }
}
