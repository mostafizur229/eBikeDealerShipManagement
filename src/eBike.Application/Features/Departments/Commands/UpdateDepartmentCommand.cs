using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Departments.Commands;

public record UpdateDepartmentCommand(
    int DepartmentId,
    string Code,
    string Description,
    int Status = 1
) : IRequest;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Valid Department ID is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Department Name is required.")
            .MaximumLength(150).WithMessage("Department Name must not exceed 150 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Department Code is required.")
            .MaximumLength(20).WithMessage("Department Code must not exceed 20 characters.");
    }
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateDepartmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == request.DepartmentId, cancellationToken);

        if (department == null)
        {
            throw new NotFoundException(nameof(Department), request.DepartmentId);
        }

        var isDuplicate = await _context.Departments
            .AnyAsync(d => d.DepartmentId != request.DepartmentId &&
                           d.DESCRIPTION.ToLower() == request.Description.Trim().ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Department with same name already exists in the system. Please try with a different name.");
        }

        int userId = _currentUser.UserId ?? 1;

        department.CODE = request.Code.Trim();
        department.DESCRIPTION = request.Description.Trim();
        department.Status = request.Status;
        department.ModifiedBy = userId;
        department.ModifiedDate = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
