using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Customers.Commands;

public record CreateCustomerCommand(
    string? Code,
    string Name,
    int DistrictId,
    int ThanaId,
    int EmployeeId,
    string? FName = null,
    string? MName = null,
    string? HusbandOrWifeName = null,
    string? ContactNo = null,
    string? Address = null,
    string? PermanentAddress = null,
    int CustomerType = 1,
    string? NID = null,
    DateTime? DOB = null,
    string? Village = null,
    string? Post = null,
    int SexType = 1,
    string? RefName = null,
    string? RefFName = null,
    string? RefContact = null,
    string? RefAddress = null,
    string? Remarks = null,
    decimal? OpeningDue = 0,
    string? EmailID = null,
    string? OthersContactNo = null,
    string? PhotoPath = null,
    int? ConcernId = null
) : IRequest<int>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer Name is required.");

        RuleFor(x => x.DistrictId)
            .GreaterThan(0).WithMessage("District is required.");

        RuleFor(x => x.ThanaId)
            .GreaterThan(0).WithMessage("Thana is required.");

        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("Employee is required.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("A/C No must not exceed 50 characters.");

        RuleFor(x => x.ContactNo)
            .MaximumLength(150).WithMessage("Contact No must not exceed 150 characters.");

        RuleFor(x => x.EmailID)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.EmailID))
            .WithMessage("Invalid Email address format.");
    }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateCustomerCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a customer.");

        var district = await _context.Districts
            .FirstOrDefaultAsync(d => d.DistrictID == request.DistrictId, cancellationToken);

        if (district == null)
        {
            throw new NotFoundException(nameof(District), request.DistrictId);
        }

        var thana = await _context.Thanas
            .FirstOrDefaultAsync(t => t.ThanaID == request.ThanaId && t.DistrictID == request.DistrictId, cancellationToken);

        if (thana == null)
        {
            throw new NotFoundException($"Thana with ID {request.ThanaId} not found under District {district.Name}.");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeID == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            throw new NotFoundException(nameof(Employee), request.EmployeeId);
        }

        if (!string.IsNullOrWhiteSpace(request.ContactNo))
        {
            var contact = request.ContactNo.Trim();
            var isDuplicateContact = await _context.Customers
                .AnyAsync(c => c.ContactNo != null && c.ContactNo == contact && c.ConcernID == concernId, cancellationToken);

            if (isDuplicateContact)
            {
                throw new ConflictException("A Customer with same contact no already exists in this concern. Please try with a different contact no.");
            }
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextCustomerCodeQuery(concernId), cancellationToken);
        }
        else
        {
            var isDuplicateCode = await _context.Customers
                .AnyAsync(c => c.Code == code && c.ConcernID == concernId, cancellationToken);

            if (isDuplicateCode)
            {
                code = await _sender.Send(new Queries.GetNextCustomerCodeQuery(concernId), cancellationToken);
            }
        }

        decimal openingDue = request.OpeningDue ?? 0m;
        int userId = _currentUser.UserId ?? 1;

        var customer = new Customer
        {
            Code = code,
            Name = request.Name.Trim(),
            FName = request.FName?.Trim(),
            MName = request.MName?.Trim(),
            HusbandOrWifeName = request.HusbandOrWifeName?.Trim(),
            ContactNo = request.ContactNo?.Trim(),
            OthersContactNo = request.OthersContactNo?.Trim(),
            EmailID = request.EmailID?.Trim(),
            NID = request.NID?.Trim(),
            Address = request.Address?.Trim(),
            PermanentAddress = request.PermanentAddress?.Trim(),
            PhotoPath = request.PhotoPath?.Trim(),
            OpeningDue = openingDue,
            TotalDue = openingDue,
            CreditDue = 0m,
            CusDueLimit = 0m,
            RefName = request.RefName?.Trim(),
            RefFName = request.RefFName?.Trim(),
            RefContact = request.RefContact?.Trim(),
            RefAddress = request.RefAddress?.Trim(),
            CustomerType = request.CustomerType,
            EmployeeID = request.EmployeeId,
            ConcernID = concernId,
            Remarks = request.Remarks?.Trim(),
            Dist = district.Name,
            Thana = thana.Name,
            Post = request.Post?.Trim(),
            Village = request.Village?.Trim(),
            SexType = request.SexType,
            DOB = request.DOB,
            DistrictID = request.DistrictId,
            ThanaID = request.ThanaId,
            CreatedBy = userId,
            CreatedDate = DateTime.Now
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return customer.CustomerID;
    }
}
