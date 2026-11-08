using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Employees.Commands;

public record CreateEmployeeCommand(
    string? Code,
    string Name,
    int DesignationId,
    string? FName = null,
    string? MName = null,
    string? ContactNo = null,
    string? EmailID = null,
    string? NID = null,
    string? BloodGroup = null,
    DateTime? JoiningDate = null,
    string? PresentAdd = null,
    string? PermanentAdd = null,
    decimal GrossSalary = 0,
    decimal? BasicSalary = null,
    string? PhotoPath = null,
    DateTime? DOB = null,
    decimal SRDueLimit = 0,
    int? DepartmentId = null,
    int? ReligionId = null,
    int MachineEMPID = 0,
    int Status = 1,
    int? ConcernId = null
) : IRequest<int>;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Employee Name is required.");

        RuleFor(x => x.DesignationId)
            .GreaterThan(0).WithMessage("Designation is required.");

        RuleFor(x => x.ContactNo)
            .MaximumLength(150).WithMessage("Contact No must not exceed 150 characters.");

        RuleFor(x => x.EmailID)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.EmailID))
            .WithMessage("Invalid Email address format.");

        RuleFor(x => x.Code)
            .MaximumLength(150).WithMessage("Employee Code must not exceed 150 characters.");
    }
}

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateEmployeeCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create an employee.");

        var designationExists = await _context.Designations
            .AnyAsync(d => d.DesignationID == request.DesignationId, cancellationToken);

        if (!designationExists)
        {
            throw new NotFoundException(nameof(Designation), request.DesignationId);
        }

        if (request.DepartmentId.HasValue && request.DepartmentId > 0)
        {
            var deptExists = await _context.Departments
                .AnyAsync(d => d.DepartmentId == request.DepartmentId.Value, cancellationToken);

            if (!deptExists)
            {
                throw new NotFoundException(nameof(Department), request.DepartmentId.Value);
            }
        }

        if (request.ReligionId.HasValue && request.ReligionId > 0)
        {
            var religionExists = await _context.Religions
                .AnyAsync(r => r.ReligionID == request.ReligionId.Value, cancellationToken);

            if (!religionExists)
            {
                throw new NotFoundException(nameof(Religion), request.ReligionId.Value);
            }
        }

        var isDuplicateName = await _context.Employees
            .AnyAsync(e => e.Name.ToLower() == request.Name.Trim().ToLower() && e.ConcernID == concernId, cancellationToken);

        if (isDuplicateName)
        {
            throw new ConflictException("An Employee with same name already exists in this concern. Please try with a different name.");
        }

        if (!string.IsNullOrWhiteSpace(request.ContactNo))
        {
            var contact = request.ContactNo.Trim();
            var isDuplicateContact = await _context.Employees
                .AnyAsync(e => e.ContactNo != null && e.ContactNo == contact && e.ConcernID == concernId, cancellationToken);

            if (isDuplicateContact)
            {
                throw new ConflictException("An Employee with same contact no. already exists in this concern.");
            }
        }

        if (request.MachineEMPID > 0)
        {
            var isDuplicateAccount = await _context.Employees
                .AnyAsync(e => e.MachineEMPID == request.MachineEMPID && e.ConcernID == concernId, cancellationToken);

            if (isDuplicateAccount)
            {
                throw new ConflictException($"An Employee with Account/Machine No '{request.MachineEMPID}' already exists in this concern.");
            }
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextEmployeeCodeQuery(concernId), cancellationToken);
        }

        decimal basicSalary = request.BasicSalary ?? request.GrossSalary;

        var employee = new Employee
        {
            Code = code,
            Name = request.Name.Trim(),
            FName = request.FName?.Trim(),
            MName = request.MName?.Trim(),
            ContactNo = request.ContactNo?.Trim(),
            EmailID = request.EmailID?.Trim(),
            NID = request.NID?.Trim(),
            BloodGroup = request.BloodGroup?.Trim(),
            JoiningDate = request.JoiningDate,
            PresentAdd = request.PresentAdd?.Trim(),
            PermanentAdd = request.PermanentAdd?.Trim(),
            DesignationID = request.DesignationId,
            GrossSalary = request.GrossSalary,
            BasicSalary = basicSalary,
            PhotoPath = request.PhotoPath?.Trim(),
            DOB = request.DOB,
            SRDueLimit = request.SRDueLimit,
            ConcernID = concernId,
            DepartmentID = request.DepartmentId > 0 ? request.DepartmentId : null,
            ReligionID = request.ReligionId > 0 ? request.ReligionId : null,
            MachineEMPID = request.MachineEMPID,
            Status = request.Status
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        return employee.EmployeeID;
    }
}
