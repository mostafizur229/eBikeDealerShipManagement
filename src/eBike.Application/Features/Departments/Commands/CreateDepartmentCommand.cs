using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Departments.Commands;

public record CreateDepartmentCommand(
    string? Code,
    string Description,
    int Status = 1
) : IRequest<int>;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Department Name is required.")
            .MaximumLength(150).WithMessage("Department Name must not exceed 150 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(20).WithMessage("Department Code must not exceed 20 characters.");
    }
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateDepartmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var isDuplicate = await _context.Departments
            .AnyAsync(d => d.DESCRIPTION.ToLower() == request.Description.Trim().ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Department with same name already exists in the system. Please try with a different name.");
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextDepartmentCodeQuery(), cancellationToken);
        }

        int userId = _currentUser.UserId ?? 1;

        var department = new Department
        {
            CODE = code,
            DESCRIPTION = request.Description.Trim(),
            Status = request.Status,
            CreatedBy = userId,
            CreateDate = DateTime.Now
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        return department.DepartmentId;
    }
}
