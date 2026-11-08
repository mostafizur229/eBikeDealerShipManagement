using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Products.Commands;

public record UpdateProductCommand(
    int Id,
    string Code,
    string ProductName,
    int CategoryId,
    int CompanyId,
    int? ConcernId = null,
    string? PicturePath = null,
    decimal PWDiscount = 0,
    DateTime? DisDurationFDate = null,
    DateTime? DisDurationToDate = null,
    EnumUnitType? UnitType = EnumUnitType.Piece,
    EnumProductType ProductType = EnumProductType.ExistingBC,
    string? CompressorWarrentyMonth = null,
    string? PanelWarrentyMonth = null,
    string? MotorWarrentyMonth = null,
    string? SparePartsWarrentyMonth = null,
    string? ServiceWarrentyMonth = null,
    decimal? MRP = null,
    decimal VatAmount = 0,
    decimal PMRate = 0,
    decimal CommissionRate = 0,
    decimal PMushokRate = 0,
    string? Weight = null,
    string? SeatingCapacity = null,
    string? TiyerSize = null,
    string? RingSize = null,
    string? NoofCylinder = null,
    string? VehicleSize = null,
    string? HorsePower = null,
    string? RPM = null,
    string? CC = null,
    string? UnLadenWeight = null,
    string? MakersName = null,
    string? MFGYear = null,
    string? ApplicableModel = null,
    string? PartsName = null,
    string? MadeIn = null,
    int PartsNoId = 0,
    int PLocationId = 0,
    int IsPartsService = 0
) : IRequest;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid Product ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Product Code is required.")
            .MaximumLength(50).WithMessage("Product Code must not exceed 50 characters.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product Name is required.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required.");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("Company is required.");
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductID == request.Id, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), request.Id);
        }

        int targetConcernId = request.ConcernId ?? product.ConcernID;

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyID == request.CompanyId, cancellationToken);
        if (company == null)
        {
            throw new NotFoundException(nameof(Company), request.CompanyId);
        }

        if (request.IsPartsService == 1)
        {
            var isDuplicate = await _context.Products
                .AnyAsync(p => p.ProductID != request.Id &&
                               p.ConcernID == targetConcernId &&
                               p.ProductName.ToLower() == request.ProductName.Trim().ToLower() &&
                               p.PartsNoID == request.PartsNoId &&
                               p.CategoryID == request.CategoryId &&
                               p.CompanyID == request.CompanyId &&
                               p.PLocationID == request.PLocationId, cancellationToken);

            if (isDuplicate)
            {
                throw new ConflictException("This Auto Part product already exists in the system.");
            }
        }
        else
        {
            var isDuplicate = await _context.Products
                .AnyAsync(p => p.ProductID != request.Id &&
                               p.ProductName.ToLower() == request.ProductName.Trim().ToLower() &&
                               p.ConcernID == targetConcernId, cancellationToken);

            if (isDuplicate)
            {
                throw new ConflictException("A Product with same name already exists in the system. Please try with a different name.");
            }
        }

        var oldProductName = product.ProductName.Trim();

        product.Code = request.Code.Trim();
        product.ProductName = request.ProductName.Trim();
        product.CategoryID = request.CategoryId;
        product.CompanyID = request.CompanyId;
        product.ConcernID = targetConcernId;
        product.PicturePath = request.PicturePath?.Trim();
        product.PWDiscount = request.PWDiscount;
        product.DisDurationFDate = request.DisDurationFDate;
        product.DisDurationToDate = request.DisDurationToDate;
        product.UnitType = request.UnitType;
        product.ProductType = request.ProductType;

        product.CompressorWarrentyMonth = request.CompressorWarrentyMonth?.Trim();
        product.PanelWarrentyMonth = request.PanelWarrentyMonth?.Trim();
        product.MotorWarrentyMonth = request.MotorWarrentyMonth?.Trim();
        product.SparePartsWarrentyMonth = request.SparePartsWarrentyMonth?.Trim();
        product.ServiceWarrentyMonth = request.ServiceWarrentyMonth?.Trim();

        product.MRP = request.MRP;
        product.VatAmount = request.VatAmount;
        product.PMRate = request.PMRate;
        product.CommissionRate = request.CommissionRate;
        product.PMushokRate = request.PMushokRate;

        product.Weight = request.Weight?.Trim();
        product.SeatingCapacity = request.SeatingCapacity?.Trim();
        product.TiyerSize = request.TiyerSize?.Trim();
        product.RingSize = request.RingSize?.Trim();
        product.NoofCylinder = request.NoofCylinder?.Trim();
        product.VehicleSize = request.VehicleSize?.Trim();
        product.HorsePower = request.HorsePower?.Trim();
        product.RPM = request.RPM?.Trim();
        product.CC = request.CC?.Trim();
        product.UnLadenWeight = request.UnLadenWeight?.Trim();
        product.MakersName = request.MakersName?.Trim();
        product.MFGYear = request.MFGYear?.Trim();
        product.ApplicableModel = request.ApplicableModel?.Trim();
        product.PartsName = request.PartsName?.Trim();
        product.MadeIn = request.MadeIn?.Trim();
        product.PartsNoID = request.PartsNoId;
        product.PLocationID = request.PLocationId;
        product.IsPartsService = request.IsPartsService;

        
        var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
            .GetConnectedFamilyConcernsAsync(_context, targetConcernId, cancellationToken);

        foreach (var familyConcern in familyConcerns)
        {
            var other = await _context.Products
                .FirstOrDefaultAsync(p => p.ConcernID == familyConcern.ConcernID && p.ProductName.ToLower() == oldProductName.ToLower(), cancellationToken);

            if (other != null)
            {
                var targetCompany = await _context.Companies
                    .FirstOrDefaultAsync(c => c.ConcernID == familyConcern.ConcernID && c.Name.ToLower() == company.Name.Trim().ToLower(), cancellationToken);

                if (targetCompany == null)
                {
                    targetCompany = new Company
                    {
                        Name = company.Name.Trim(),
                        ConcernID = familyConcern.ConcernID,
                        Code = company.Code
                    };
                    _context.Companies.Add(targetCompany);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                other.CompanyID = targetCompany.CompanyID;

                var targetCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.ConcernID == familyConcern.ConcernID && c.Description.ToLower() == category.Description.Trim().ToLower(), cancellationToken);

                if (targetCategory == null)
                {
                    targetCategory = new Category
                    {
                        Description = category.Description.Trim(),
                        ConcernID = familyConcern.ConcernID,
                        Code = category.Code
                    };
                    _context.Categories.Add(targetCategory);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                other.CategoryID = targetCategory.CategoryID;

            other.ProductName = request.ProductName.Trim();
            other.PicturePath = request.PicturePath?.Trim();
            other.PWDiscount = request.PWDiscount;
            other.DisDurationFDate = request.DisDurationFDate;
            other.DisDurationToDate = request.DisDurationToDate;
            other.UnitType = request.UnitType;
            other.ProductType = request.ProductType;

            other.CompressorWarrentyMonth = request.CompressorWarrentyMonth?.Trim();
            other.PanelWarrentyMonth = request.PanelWarrentyMonth?.Trim();
            other.MotorWarrentyMonth = request.MotorWarrentyMonth?.Trim();
            other.SparePartsWarrentyMonth = request.SparePartsWarrentyMonth?.Trim();
            other.ServiceWarrentyMonth = request.ServiceWarrentyMonth?.Trim();

            other.MRP = request.MRP;
            other.VatAmount = request.VatAmount;
            other.PMRate = request.PMRate;
            other.CommissionRate = request.CommissionRate;
            other.PMushokRate = request.PMushokRate;

            other.Weight = request.Weight?.Trim();
            other.SeatingCapacity = request.SeatingCapacity?.Trim();
            other.TiyerSize = request.TiyerSize?.Trim();
            other.RingSize = request.RingSize?.Trim();
            other.NoofCylinder = request.NoofCylinder?.Trim();
            other.VehicleSize = request.VehicleSize?.Trim();
            other.HorsePower = request.HorsePower?.Trim();
            other.RPM = request.RPM?.Trim();
            other.CC = request.CC?.Trim();
            other.UnLadenWeight = request.UnLadenWeight?.Trim();
            other.MakersName = request.MakersName?.Trim();
            other.MFGYear = request.MFGYear?.Trim();
            other.ApplicableModel = request.ApplicableModel?.Trim();
            other.PartsName = request.PartsName?.Trim();
            other.MadeIn = request.MadeIn?.Trim();
            other.PartsNoID = request.PartsNoId;
            other.PLocationID = request.PLocationId;
            other.IsPartsService = request.IsPartsService;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
