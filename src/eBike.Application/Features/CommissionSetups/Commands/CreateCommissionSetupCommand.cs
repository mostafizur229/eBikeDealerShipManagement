using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CommissionSetups.Commands;

public record CreateCommissionSetupCommand(
    int EmployeeID,
    decimal AchievedPercentStart,
    decimal AchievedPercentEnd,
    DateTime CommissionMonth,
    decimal CommisssionAmt,
    decimal CommissionPercent,
    int Status = 1
) : IRequest<int>;

public class CreateCommissionSetupCommandValidator : AbstractValidator<CreateCommissionSetupCommand>
{
    public CreateCommissionSetupCommandValidator()
    {
        RuleFor(x => x.EmployeeID).GreaterThan(0).WithMessage("Employee is required.");
        RuleFor(x => x.AchievedPercentStart).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AchievedPercentEnd).GreaterThanOrEqualTo(x => x.AchievedPercentStart)
            .WithMessage("End percent must be greater than or equal to start percent.");
    }
}

public class CreateCommissionSetupCommandHandler : IRequestHandler<CreateCommissionSetupCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CreateCommissionSetupCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(CreateCommissionSetupCommand request, CancellationToken cancellationToken)
    {
        var monthNorm = new DateTime(request.CommissionMonth.Year, request.CommissionMonth.Month, 1);

        var commission = new CommissionSetup
        {
            EmployeeID = request.EmployeeID,
            AchievedPercentStart = request.AchievedPercentStart,
            AchievedPercentEnd = request.AchievedPercentEnd,
            CommissionMonth = monthNorm,
            CommisssionAmt = request.CommisssionAmt,
            CommissionPercent = request.CommissionPercent,
            Status = request.Status,
            ConcernID = _currentUser.ConcernId ?? 1,
            CreatedBy = _currentUser.UserId ?? 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.CommissionSetups.Add(commission);
        await _context.SaveChangesAsync(cancellationToken);

        return commission.CSID;
    }
}

public record UpdateCommissionSetupCommand(
    int CSID,
    int EmployeeID,
    decimal AchievedPercentStart,
    decimal AchievedPercentEnd,
    DateTime CommissionMonth,
    decimal CommisssionAmt,
    decimal CommissionPercent,
    int Status
) : IRequest<bool>;

public class UpdateCommissionSetupCommandHandler : IRequestHandler<UpdateCommissionSetupCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateCommissionSetupCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(UpdateCommissionSetupCommand request, CancellationToken cancellationToken)
    {
        var commission = await _context.CommissionSetups.FirstOrDefaultAsync(c => c.CSID == request.CSID, cancellationToken);
        if (commission == null)
        {
            throw new NotFoundException("CommissionSetup", request.CSID.ToString());
        }

        commission.EmployeeID = request.EmployeeID;
        commission.AchievedPercentStart = request.AchievedPercentStart;
        commission.AchievedPercentEnd = request.AchievedPercentEnd;
        commission.CommissionMonth = new DateTime(request.CommissionMonth.Year, request.CommissionMonth.Month, 1);
        commission.CommisssionAmt = request.CommisssionAmt;
        commission.CommissionPercent = request.CommissionPercent;
        commission.Status = request.Status;
        commission.ModifiedBy = _currentUser.UserId;
        commission.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record DeleteCommissionSetupCommand(int Id) : IRequest<bool>;

public class DeleteCommissionSetupCommandHandler : IRequestHandler<DeleteCommissionSetupCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCommissionSetupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCommissionSetupCommand request, CancellationToken cancellationToken)
    {
        var commission = await _context.CommissionSetups.FirstOrDefaultAsync(c => c.CSID == request.Id, cancellationToken);
        if (commission == null)
        {
            throw new NotFoundException("CommissionSetup", request.Id.ToString());
        }

        _context.CommissionSetups.Remove(commission);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record UpsertDesWiseCommissionCommand(
    int DesignationID,
    decimal CommissionPercent
) : IRequest<int>;

public class UpsertDesWiseCommissionCommandHandler : IRequestHandler<UpsertDesWiseCommissionCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpsertDesWiseCommissionCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(UpsertDesWiseCommissionCommand request, CancellationToken cancellationToken)
    {
        var concernId = _currentUser.ConcernId ?? 1;

        var existing = await _context.DesWiseCommissions
            .FirstOrDefaultAsync(d => d.DesignationID == request.DesignationID && d.ConcernID == concernId, cancellationToken);

        if (existing != null)
        {
            existing.CommissionPercent = request.CommissionPercent;
            existing.ModifiedBy = _currentUser.UserId;
            existing.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return existing.ID;
        }

        var desCommission = new DesWiseCommission
        {
            DesignationID = request.DesignationID,
            CommissionPercent = request.CommissionPercent,
            ConcernID = concernId,
            CreatedBy = _currentUser.UserId ?? 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.DesWiseCommissions.Add(desCommission);
        await _context.SaveChangesAsync(cancellationToken);
        return desCommission.ID;
    }
}
