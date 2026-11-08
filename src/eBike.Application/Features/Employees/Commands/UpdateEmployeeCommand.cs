using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Employees.Commands;

public record UpdateEmployeeCommand(
    int EmployeeID,
    string Code,
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
) : IRequest;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeID)
            .GreaterThan(0).WithMessage("Valid Employee ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Employee Name is required.");

        RuleFor(x => x.DesignationId)
            .GreaterThan(0).WithMessage("Designation is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Employee Code is required.")
            .MaximumLength(150).WithMessage("Employee Code must not exceed 150 characters.");

        RuleFor(x => x.ContactNo)
            .MaximumLength(150).WithMessage("Contact No must not exceed 150 characters.");

        RuleFor(x => x.EmailID)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.EmailID))
            .WithMessage("Invalid Email address format.");
    }
}

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeID == request.EmployeeID, cancellationToken);

        if (employee == null)
        {
            throw new NotFoundException(nameof(Employee), request.EmployeeID);
        }

        int targetConcernId = request.ConcernId ?? employee.ConcernID;

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
            .AnyAsync(e => e.EmployeeID != request.EmployeeID &&
                           e.Name.ToLower() == request.Name.Trim().ToLower() &&
                           e.ConcernID == targetConcernId, cancellationToken);

        if (isDuplicateName)
        {
            throw new ConflictException("An Employee with same name already exists in this concern. Please try with a different name.");
        }

        if (!string.IsNullOrWhiteSpace(request.ContactNo))
        {
            var contact = request.ContactNo.Trim();
            var isDuplicateContact = await _context.Employees
                .AnyAsync(e => e.EmployeeID != request.EmployeeID &&
                               e.ContactNo != null && e.ContactNo == contact &&
                               e.ConcernID == targetConcernId, cancellationToken);

            if (isDuplicateContact)
            {
                throw new ConflictException("An Employee with same contact no. already exists in this concern.");
            }
        }

        if (request.MachineEMPID > 0)
        {
            var isDuplicateAccount = await _context.Employees
                .AnyAsync(e => e.EmployeeID != request.EmployeeID &&
                               e.MachineEMPID == request.MachineEMPID &&
                               e.ConcernID == targetConcernId, cancellationToken);

            if (isDuplicateAccount)
            {
                throw new ConflictException($"An Employee with Account/Machine No '{request.MachineEMPID}' already exists in this concern.");
            }
        }

        employee.Code = request.Code.Trim();
        employee.Name = request.Name.Trim();
        employee.FName = request.FName?.Trim();
        employee.MName = request.MName?.Trim();
        employee.ContactNo = request.ContactNo?.Trim();
        employee.EmailID = request.EmailID?.Trim();
        employee.NID = request.NID?.Trim();
        employee.BloodGroup = request.BloodGroup?.Trim();
        employee.JoiningDate = request.JoiningDate;
        employee.PresentAdd = request.PresentAdd?.Trim();
        employee.PermanentAdd = request.PermanentAdd?.Trim();
        employee.DesignationID = request.DesignationId;
        employee.GrossSalary = request.GrossSalary;
        employee.BasicSalary = request.BasicSalary ?? request.GrossSalary;
        employee.DOB = request.DOB;
        employee.SRDueLimit = request.SRDueLimit;
        employee.ConcernID = targetConcernId;
        employee.DepartmentID = request.DepartmentId > 0 ? request.DepartmentId : null;
        employee.ReligionID = request.ReligionId > 0 ? request.ReligionId : null;
        employee.MachineEMPID = request.MachineEMPID;
        employee.Status = request.Status;

        if (request.PhotoPath != null)
        {
            employee.PhotoPath = request.PhotoPath.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
