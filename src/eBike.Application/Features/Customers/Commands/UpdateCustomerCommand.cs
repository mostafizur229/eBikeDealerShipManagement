using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Customers.Commands;

public record UpdateCustomerCommand(
    int CustomerID,
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
    decimal? TotalDue = null,
    string? EmailID = null,
    string? OthersContactNo = null,
    string? PhotoPath = null
) : IRequest;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerID)
            .GreaterThan(0).WithMessage("Customer ID is required.");

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

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public UpdateCustomerCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _currentUser = currentUser;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerID == request.CustomerID, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), request.CustomerID);
        }

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
                .AnyAsync(c => c.CustomerID != customer.CustomerID &&
                               c.ContactNo != null &&
                               c.ContactNo == contact &&
                               c.ConcernID == customer.ConcernID, cancellationToken);

            if (isDuplicateContact)
            {
                throw new ConflictException("A Customer with same contact no already exists in this concern. Please try with a different contact no.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim();
            var isDuplicateCode = await _context.Customers
                .AnyAsync(c => c.CustomerID != customer.CustomerID &&
                               c.Code == code &&
                               c.ConcernID == customer.ConcernID, cancellationToken);

            if (isDuplicateCode)
            {
                throw new ConflictException($"Customer A/C No '{code}' already exists in this concern.");
            }

            customer.Code = code;
        }

        if (customer.CustomerType != request.CustomerType)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();
            var txCount = await connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT 
                    (SELECT COUNT(1) FROM SOrders WHERE CustomerID = @id AND Status != 4) +
                    (SELECT COUNT(1) FROM CreditSales WHERE CustomerID = @id AND Status != 4);",
                new { id = customer.CustomerID });

            if (txCount > 0)
            {
                throw new ConflictException("Customer type cannot be changed because this customer has associated sales transactions.");
            }

            customer.CustomerType = request.CustomerType;
        }

        int userId = _currentUser.UserId ?? 1;

        customer.Name = request.Name.Trim();
        customer.FName = request.FName?.Trim();
        customer.MName = request.MName?.Trim();
        customer.HusbandOrWifeName = request.HusbandOrWifeName?.Trim();
        customer.ContactNo = request.ContactNo?.Trim();
        customer.OthersContactNo = request.OthersContactNo?.Trim();
        customer.EmailID = request.EmailID?.Trim();
        customer.NID = request.NID?.Trim();
        customer.Address = request.Address?.Trim();
        customer.PermanentAddress = request.PermanentAddress?.Trim();
        if (!string.IsNullOrWhiteSpace(request.PhotoPath))
        {
            customer.PhotoPath = request.PhotoPath.Trim();
        }
        if (request.TotalDue.HasValue)
        {
            customer.TotalDue = request.TotalDue.Value;
        }
        customer.RefName = request.RefName?.Trim();
        customer.RefFName = request.RefFName?.Trim();
        customer.RefContact = request.RefContact?.Trim();
        customer.RefAddress = request.RefAddress?.Trim();
        customer.EmployeeID = request.EmployeeId;
        customer.Remarks = request.Remarks?.Trim();
        customer.Dist = district.Name;
        customer.Thana = thana.Name;
        customer.Post = request.Post?.Trim();
        customer.Village = request.Village?.Trim();
        customer.SexType = request.SexType;
        customer.DOB = request.DOB;
        customer.DistrictID = request.DistrictId;
        customer.ThanaID = request.ThanaId;
        customer.ModifiedBy = userId;
        customer.ModifiedDate = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
