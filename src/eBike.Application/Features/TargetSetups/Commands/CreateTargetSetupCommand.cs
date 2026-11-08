using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.TargetSetups.Commands;

public class TargetSetupDetailInput
{
    public int ProductID { get; set; }
    public decimal Quantity { get; set; }
}

public record CreateTargetSetupCommand(
    int EmployeeID,
    DateTime TargetMonth,
    decimal Quantity,
    decimal Amount,
    int Status = 1,
    List<TargetSetupDetailInput>? Details = null
) : IRequest<int>;

public class CreateTargetSetupCommandValidator : AbstractValidator<CreateTargetSetupCommand>
{
    public CreateTargetSetupCommandValidator()
    {
        RuleFor(x => x.EmployeeID).GreaterThan(0).WithMessage("Employee is required.");
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).WithMessage("Amount must be >= 0.");
    }
}

public class CreateTargetSetupCommandHandler : IRequestHandler<CreateTargetSetupCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CreateTargetSetupCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(CreateTargetSetupCommand request, CancellationToken cancellationToken)
    {
        var targetMonthNorm = new DateTime(request.TargetMonth.Year, request.TargetMonth.Month, 1);

        var existing = await _context.TargetSetups
            .FirstOrDefaultAsync(t => t.EmployeeID == request.EmployeeID && t.TargetMonth == targetMonthNorm, cancellationToken);

        if (existing != null)
        {
            throw new BadRequestException($"Target setup for this employee in {targetMonthNorm:MMM yyyy} already exists.");
        }

        var target = new TargetSetup
        {
            EmployeeID = request.EmployeeID,
            TargetMonth = targetMonthNorm,
            Quantity = request.Quantity,
            Amount = request.Amount,
            Status = request.Status,
            ConcernID = _currentUser.ConcernId ?? 1,
            CreatedBy = _currentUser.UserId ?? 1,
            CreatedDate = DateTime.UtcNow
        };

        if (request.Details != null && request.Details.Any())
        {
            foreach (var d in request.Details)
            {
                target.TargetSetupDetails.Add(new TargetSetupDetail
                {
                    ProductID = d.ProductID,
                    Quantity = d.Quantity
                });
            }
        }

        _context.TargetSetups.Add(target);
        await _context.SaveChangesAsync(cancellationToken);

        return target.TID;
    }
}

public record UpdateTargetSetupCommand(
    int TID,
    int EmployeeID,
    DateTime TargetMonth,
    decimal Quantity,
    decimal Amount,
    int Status,
    List<TargetSetupDetailInput>? Details = null
) : IRequest<bool>;

public class UpdateTargetSetupCommandHandler : IRequestHandler<UpdateTargetSetupCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateTargetSetupCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(UpdateTargetSetupCommand request, CancellationToken cancellationToken)
    {
        var target = await _context.TargetSetups
            .Include(t => t.TargetSetupDetails)
            .FirstOrDefaultAsync(t => t.TID == request.TID, cancellationToken);

        if (target == null)
        {
            throw new NotFoundException("TargetSetup", request.TID.ToString());
        }

        target.EmployeeID = request.EmployeeID;
        target.TargetMonth = new DateTime(request.TargetMonth.Year, request.TargetMonth.Month, 1);
        target.Quantity = request.Quantity;
        target.Amount = request.Amount;
        target.Status = request.Status;
        target.ModifiedBy = _currentUser.UserId;
        target.ModifiedDate = DateTime.UtcNow;

        if (request.Details != null)
        {
            _context.TargetSetupDetails.RemoveRange(target.TargetSetupDetails);
            foreach (var d in request.Details)
            {
                target.TargetSetupDetails.Add(new TargetSetupDetail
                {
                    ProductID = d.ProductID,
                    Quantity = d.Quantity,
                    TID = target.TID
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record DeleteTargetSetupCommand(int Id) : IRequest<bool>;

public class DeleteTargetSetupCommandHandler : IRequestHandler<DeleteTargetSetupCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteTargetSetupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteTargetSetupCommand request, CancellationToken cancellationToken)
    {
        var target = await _context.TargetSetups
            .Include(t => t.TargetSetupDetails)
            .FirstOrDefaultAsync(t => t.TID == request.Id, cancellationToken);

        if (target == null)
        {
            throw new NotFoundException("TargetSetup", request.Id.ToString());
        }

        _context.TargetSetups.Remove(target);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
