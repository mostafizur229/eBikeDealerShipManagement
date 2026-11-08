using eBike.Application.Common.DTOs;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Products.Queries;

public record GetProductsQuery(
    string? SearchTerm = null,
    int? CategoryId = null,
    int? CompanyId = null,
    int? ConcernId = null,
    bool? IsParts = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedList<ProductListDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetProductsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ProductListDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Company)
            .Include(p => p.SisterConcern)
            .AsNoTracking()
            .AsQueryable();

        int? filterConcernId = request.ConcernId ?? _currentUser.ConcernId;
        if (filterConcernId.HasValue && filterConcernId.Value > 0)
        {
            query = query.Where(p => p.ConcernID == filterConcernId.Value);
        }

        if (request.IsParts.HasValue)
        {
            int isPartsVal = request.IsParts.Value ? 1 : 0;
            query = query.Where(p => p.IsPartsService == isPartsVal);
        }

        if (request.CategoryId.HasValue && request.CategoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryID == request.CategoryId.Value);
        }

        if (request.CompanyId.HasValue && request.CompanyId.Value > 0)
        {
            query = query.Where(p => p.CompanyID == request.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.ProductName.ToLower().Contains(search) ||
                p.Code.ToLower().Contains(search) ||
                (p.MakersName != null && p.MakersName.ToLower().Contains(search)) ||
                (p.ApplicableModel != null && p.ApplicableModel.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        int pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        int pageSize = request.PageSize <= 0 ? 10 : (request.PageSize > 5000 ? 5000 : request.PageSize);

        var pagedQuery = query
            .OrderBy(p => p.Code)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var items = await (from p in pagedQuery
                           join part in _context.Parts on p.PartsNoID equals part.PartsNoID into partJ
                           from part in partJ.DefaultIfEmpty()
                           join loc in _context.PLocations on p.PLocationID equals loc.PLocationID into locJ
                           from loc in locJ.DefaultIfEmpty()
                           select new ProductListDto
                           {
                               Id = p.ProductID,
                               Code = p.Code,
                               ProductName = p.ProductName,
                               CategoryId = p.CategoryID,
                               CategoryName = p.Category != null ? p.Category.Description : null,
                               CompanyId = p.CompanyID,
                               CompanyName = p.Company != null ? p.Company.Name : null,
                               ConcernId = p.ConcernID,
                               ConcernName = p.SisterConcern != null ? p.SisterConcern.Name : null,
                               MRP = p.MRP,
                               PMRate = p.PMRate,
                               CommissionRate = p.CommissionRate,
                               VatAmount = p.VatAmount,
                               PWDiscount = p.PWDiscount,
                               CC = p.CC,
                               MFGYear = p.MFGYear,
                               MadeIn = p.MadeIn,
                               PicturePath = p.PicturePath,
                               IsPartsService = p.IsPartsService,
                               ApplicableModel = p.ApplicableModel,
                               PartsNoId = p.PartsNoID,
                               PartNo = part != null ? part.Description : null,
                               PLocationId = p.PLocationID,
                               LocationName = loc != null ? loc.Description : null
                           })
                           .ToListAsync(cancellationToken);

        return new PaginatedList<ProductListDto>(items, totalCount, pageNumber, pageSize);
    }
}
