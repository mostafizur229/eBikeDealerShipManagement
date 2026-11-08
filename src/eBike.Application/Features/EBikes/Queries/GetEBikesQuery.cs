using eBike.Application.Common.Interfaces;
using eBike.Application.Features.EBikes.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.EBikes.Queries;

public record GetEBikesQuery(
    EBikeStatus? Status = null,
    int? WarehouseId = null,
    string? SearchTerm = null
) : IRequest<List<EBikeDto>>;

public class GetEBikesQueryHandler : IRequestHandler<GetEBikesQuery, List<EBikeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEBikesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EBikeDto>> Handle(GetEBikesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.EBikes
            .Include(b => b.Warehouse)
            .AsNoTracking()
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(b => b.Status == request.Status.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(b => b.WarehouseId == request.WarehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(b =>
                b.ChassisNumber.ToLower().Contains(search) ||
                b.MotorNumber.ToLower().Contains(search) ||
                b.Brand.ToLower().Contains(search) ||
                b.Model.ToLower().Contains(search));
        }

        return await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new EBikeDto
            {
                Id = b.Id,
                ChassisNumber = b.ChassisNumber,
                MotorNumber = b.MotorNumber,
                Brand = b.Brand,
                Model = b.Model,
                Color = b.Color,
                BatteryCapacity = b.BatteryCapacity,
                MotorPower = b.MotorPower,
                PurchaseCost = b.PurchaseCost,
                SellingPrice = b.SellingPrice,
                Status = b.Status,
                CurrentOdometerKm = b.CurrentOdometerKm,
                WarehouseId = b.WarehouseId,
                WarehouseName = b.Warehouse.Name,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
