using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Products.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Products.Queries;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.Products
            .Include(x => x.Category)
            .Include(x => x.Company)
            .Include(x => x.SisterConcern)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductID == request.Id, cancellationToken);

        if (p == null)
        {
            throw new NotFoundException(nameof(Product), request.Id);
        }

        string? partNo = null;
        if (p.PartsNoID > 0)
        {
            var part = await _context.Parts.AsNoTracking().FirstOrDefaultAsync(x => x.PartsNoID == p.PartsNoID, cancellationToken);
            partNo = part?.Description;
        }

        string? locationName = null;
        if (p.PLocationID > 0)
        {
            var loc = await _context.PLocations.AsNoTracking().FirstOrDefaultAsync(x => x.PLocationID == p.PLocationID, cancellationToken);
            locationName = loc?.Description;
        }

        return new ProductDto
        {
            Id = p.ProductID,
            Code = p.Code,
            ProductName = p.ProductName,
            PicturePath = p.PicturePath,
            CategoryId = p.CategoryID,
            CategoryName = p.Category != null ? p.Category.Description : null,
            CompanyId = p.CompanyID,
            CompanyName = p.Company != null ? p.Company.Name : null,
            ConcernId = p.ConcernID,
            ConcernName = p.SisterConcern != null ? p.SisterConcern.Name : null,

            PWDiscount = p.PWDiscount,
            DisDurationFDate = p.DisDurationFDate,
            DisDurationToDate = p.DisDurationToDate,
            UnitType = p.UnitType,
            ProductType = p.ProductType,

            CompressorWarrentyMonth = p.CompressorWarrentyMonth,
            PanelWarrentyMonth = p.PanelWarrentyMonth,
            MotorWarrentyMonth = p.MotorWarrentyMonth,
            SparePartsWarrentyMonth = p.SparePartsWarrentyMonth,
            ServiceWarrentyMonth = p.ServiceWarrentyMonth,

            MRP = p.MRP,
            VatAmount = p.VatAmount,
            PMRate = p.PMRate,
            CommissionRate = p.CommissionRate,
            PMushokRate = p.PMushokRate,

            Weight = p.Weight,
            SeatingCapacity = p.SeatingCapacity,
            TiyerSize = p.TiyerSize,
            RingSize = p.RingSize,
            NoofCylinder = p.NoofCylinder,
            VehicleSize = p.VehicleSize,
            HorsePower = p.HorsePower,
            RPM = p.RPM,
            CC = p.CC,
            UnLadenWeight = p.UnLadenWeight,
            MakersName = p.MakersName,
            MFGYear = p.MFGYear,
            ApplicableModel = p.ApplicableModel,
            PartsName = p.PartsName,
            MadeIn = p.MadeIn,
            PartsNoId = p.PartsNoID,
            PartNo = partNo,
            PLocationId = p.PLocationID,
            LocationName = locationName,
            IsPartsService = p.IsPartsService
        };
    }
}
