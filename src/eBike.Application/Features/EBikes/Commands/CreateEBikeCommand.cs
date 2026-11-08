using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.EBikes.Commands;

public record CreateEBikeCommand(
    string ChassisNumber,
    string MotorNumber,
    string Brand,
    string Model,
    string Color,
    string BatteryCapacity,
    string MotorPower,
    decimal PurchaseCost,
    decimal SellingPrice,
    int WarehouseId,
    int? SupplierId
) : IRequest<int>;

public class CreateEBikeCommandValidator : AbstractValidator<CreateEBikeCommand>
{
    public CreateEBikeCommandValidator()
    {
        RuleFor(x => x.ChassisNumber).NotEmpty().WithMessage("Chassis / VIN number is required.");
        RuleFor(x => x.MotorNumber).NotEmpty().WithMessage("Motor number is required.");
        RuleFor(x => x.Brand).NotEmpty().WithMessage("Brand is required.");
        RuleFor(x => x.Model).NotEmpty().WithMessage("Model is required.");
        RuleFor(x => x.PurchaseCost).GreaterThanOrEqualTo(0).WithMessage("Purchase cost cannot be negative.");
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0).WithMessage("Selling price cannot be negative.");
        RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("Valid warehouse is required.");
    }
}

public class CreateEBikeCommandHandler : IRequestHandler<CreateEBikeCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateEBikeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateEBikeCommand request, CancellationToken cancellationToken)
    {
        // Check uniqueness of Chassis Number
        var exists = await _context.EBikes
            .AnyAsync(b => b.ChassisNumber == request.ChassisNumber, cancellationToken);

        if (exists)
        {
            throw new ConflictException($"An e-bike with Chassis Number '{request.ChassisNumber}' already exists.");
        }

        var warehouseExists = await _context.Warehouses
            .AnyAsync(w => w.Id == request.WarehouseId, cancellationToken);

        if (!warehouseExists)
        {
            throw new NotFoundException(nameof(Warehouse), request.WarehouseId);
        }

        var eBike = new EBikeMaster
        {
            ChassisNumber = request.ChassisNumber,
            MotorNumber = request.MotorNumber,
            Brand = request.Brand,
            Model = request.Model,
            Color = request.Color,
            BatteryCapacity = request.BatteryCapacity,
            MotorPower = request.MotorPower,
            PurchaseCost = request.PurchaseCost,
            SellingPrice = request.SellingPrice,
            WarehouseId = request.WarehouseId,
            SupplierId = request.SupplierId,
            Status = EBikeStatus.Available
        };

        _context.EBikes.Add(eBike);

        // Record stock movement (PurchaseIn)
        _context.StockMovements.Add(new StockMovement
        {
            TransactionType = StockTransactionType.PurchaseIn,
            EBike = eBike,
            Quantity = 1,
            UnitCost = request.PurchaseCost,
            DestinationWarehouseId = request.WarehouseId,
            ReferenceNumber = $"INIT-{request.ChassisNumber}",
            Remarks = "New e-bike received into inventory"
        });

        await _context.SaveChangesAsync(cancellationToken);
        return eBike.Id;
    }
}
