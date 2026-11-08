using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Products.Commands;

public record CreateProductCommand(
    string? Code,
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
) : IRequest<int>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product Name is required.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required.");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("Company is required.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Product Code must not exceed 50 characters.");
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateProductCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId 
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a product.");

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
                .AnyAsync(p => p.ConcernID == concernId &&
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
                .AnyAsync(p => p.ProductName.ToLower() == request.ProductName.Trim().ToLower() && p.ConcernID == concernId, cancellationToken);

            if (isDuplicate)
            {
                throw new ConflictException("A Product with same name already exists in the system. Please try with a different name.");
            }
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextProductCodeQuery(concernId), cancellationToken);
        }

        var product = new Product
        {
            Code = code,
            ProductName = request.ProductName.Trim(),
            PicturePath = request.PicturePath?.Trim(),
            CategoryID = request.CategoryId,
            CompanyID = request.CompanyId,
            ConcernID = concernId,
            PWDiscount = request.PWDiscount,
            DisDurationFDate = request.DisDurationFDate,
            DisDurationToDate = request.DisDurationToDate,
            UnitType = request.UnitType,
            ProductType = request.ProductType,

            CompressorWarrentyMonth = request.CompressorWarrentyMonth?.Trim(),
            PanelWarrentyMonth = request.PanelWarrentyMonth?.Trim(),
            MotorWarrentyMonth = request.MotorWarrentyMonth?.Trim(),
            SparePartsWarrentyMonth = request.SparePartsWarrentyMonth?.Trim(),
            ServiceWarrentyMonth = request.ServiceWarrentyMonth?.Trim(),

            MRP = request.MRP,
            VatAmount = request.VatAmount,
            PMRate = request.PMRate,
            CommissionRate = request.CommissionRate,
            PMushokRate = request.PMushokRate,

            Weight = request.Weight?.Trim(),
            SeatingCapacity = request.SeatingCapacity?.Trim(),
            TiyerSize = request.TiyerSize?.Trim(),
            RingSize = request.RingSize?.Trim(),
            NoofCylinder = request.NoofCylinder?.Trim(),
            VehicleSize = request.VehicleSize?.Trim(),
            HorsePower = request.HorsePower?.Trim(),
            RPM = request.RPM?.Trim(),
            CC = request.CC?.Trim(),
            UnLadenWeight = request.UnLadenWeight?.Trim(),
            MakersName = request.MakersName?.Trim(),
            MFGYear = request.MFGYear?.Trim(),
            ApplicableModel = request.ApplicableModel?.Trim(),
            PartsName = request.PartsName?.Trim(),
            MadeIn = request.MadeIn?.Trim(),
            PartsNoID = request.PartsNoId,
            PLocationID = request.PLocationId,
            IsPartsService = request.IsPartsService
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
            .GetConnectedFamilyConcernsAsync(_context, concernId, cancellationToken);

        foreach (var familyConcern in familyConcerns)
        {
            var otherConcernId = familyConcern.ConcernID;
            var existsInOther = await _context.Products
                .AnyAsync(p => p.ConcernID == otherConcernId && p.ProductName.ToLower() == request.ProductName.Trim().ToLower(), cancellationToken);

            if (!existsInOther)
            {
                var targetCompany = await _context.Companies
                    .FirstOrDefaultAsync(c => c.ConcernID == otherConcernId && c.Name.ToLower() == company.Name.Trim().ToLower(), cancellationToken);

                if (targetCompany == null)
                {
                    var targetCompanyCode = !string.IsNullOrWhiteSpace(company.Code)
                        ? company.Code
                        : await _sender.Send(new Features.Companies.Queries.GetNextCompanyCodeQuery(otherConcernId), cancellationToken);

                    targetCompany = new Company
                    {
                        Code = targetCompanyCode,
                        Name = company.Name.Trim(),
                        ConcernID = otherConcernId
                    };
                    _context.Companies.Add(targetCompany);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                var targetCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.ConcernID == otherConcernId && c.Description.ToLower() == category.Description.Trim().ToLower(), cancellationToken);

                if (targetCategory == null)
                {
                    var targetCategoryCode = !string.IsNullOrWhiteSpace(category.Code)
                        ? category.Code
                        : await _sender.Send(new Features.Categories.Queries.GetNextCategoryCodeQuery(otherConcernId), cancellationToken);

                    targetCategory = new Category
                    {
                        Code = targetCategoryCode,
                        Description = category.Description.Trim(),
                        ConcernID = otherConcernId
                    };
                    _context.Categories.Add(targetCategory);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                var targetProductCode = await _sender.Send(new Queries.GetNextProductCodeQuery(otherConcernId), cancellationToken);

                var replicatedProduct = new Product
                {
                    Code = targetProductCode,
                    ProductName = request.ProductName.Trim(),
                    PicturePath = request.PicturePath?.Trim(),
                    CategoryID = targetCategory.CategoryID,
                    CompanyID = targetCompany.CompanyID,
                    ConcernID = otherConcernId,
                    PWDiscount = request.PWDiscount,
                    DisDurationFDate = request.DisDurationFDate,
                    DisDurationToDate = request.DisDurationToDate,
                    UnitType = request.UnitType,
                    ProductType = request.ProductType,

                    CompressorWarrentyMonth = request.CompressorWarrentyMonth?.Trim(),
                    PanelWarrentyMonth = request.PanelWarrentyMonth?.Trim(),
                    MotorWarrentyMonth = request.MotorWarrentyMonth?.Trim(),
                    SparePartsWarrentyMonth = request.SparePartsWarrentyMonth?.Trim(),
                    ServiceWarrentyMonth = request.ServiceWarrentyMonth?.Trim(),

                    MRP = request.MRP,
                    VatAmount = request.VatAmount,
                    PMRate = request.PMRate,
                    CommissionRate = request.CommissionRate,
                    PMushokRate = request.PMushokRate,

                    Weight = request.Weight?.Trim(),
                    SeatingCapacity = request.SeatingCapacity?.Trim(),
                    TiyerSize = request.TiyerSize?.Trim(),
                    RingSize = request.RingSize?.Trim(),
                    NoofCylinder = request.NoofCylinder?.Trim(),
                    VehicleSize = request.VehicleSize?.Trim(),
                    HorsePower = request.HorsePower?.Trim(),
                    RPM = request.RPM?.Trim(),
                    CC = request.CC?.Trim(),
                    UnLadenWeight = request.UnLadenWeight?.Trim(),
                    MakersName = request.MakersName?.Trim(),
                    MFGYear = request.MFGYear?.Trim(),
                    ApplicableModel = request.ApplicableModel?.Trim(),
                    PartsName = request.PartsName?.Trim(),
                    MadeIn = request.MadeIn?.Trim(),
                    IsPartsService = request.IsPartsService
                };

                _context.Products.Add(replicatedProduct);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return product.ProductID;
    }
}
