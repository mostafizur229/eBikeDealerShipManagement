using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Suppliers.Commands;

public record UpdateSupplierCommand(
    int SupplierID,
    string? Code,
    string Name,
    string? OwnerName = null,
    string? ContactNo = null,
    string? Address = null,
    string? PhotoPath = null,
    decimal? OpeningDue = null,
    decimal? TotalDue = null,
    string? Remarks = null,
    int? ConcernId = null
) : IRequest;

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.SupplierID)
            .GreaterThan(0).WithMessage("Valid Supplier ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier Name is required.");

        RuleFor(x => x.ContactNo)
            .MaximumLength(150).WithMessage("Contact No must not exceed 150 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Supplier Code must not exceed 50 characters.");
    }
}

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateSupplierCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierID == request.SupplierID, cancellationToken);

        if (supplier == null)
        {
            throw new NotFoundException(nameof(Supplier), request.SupplierID);
        }

        int concernId = request.ConcernId ?? supplier.ConcernID;

        // Duplicate Name check within same concern (excluding current supplier)
        var isDuplicateName = await _context.Suppliers
            .AnyAsync(s => s.SupplierID != request.SupplierID &&
                           s.Name != null &&
                           s.Name.ToLower() == request.Name.Trim().ToLower() &&
                           s.ConcernID == concernId, cancellationToken);

        if (isDuplicateName)
        {
            throw new ConflictException("A Supplier with same name already exists in the system. Please try with a different name.");
        }

        // Duplicate ContactNo check within same concern (excluding current supplier)
        if (!string.IsNullOrWhiteSpace(request.ContactNo))
        {
            var contact = request.ContactNo.Trim();
            var isDuplicateContact = await _context.Suppliers
                .AnyAsync(s => s.SupplierID != request.SupplierID &&
                               s.ContactNo != null &&
                               s.ContactNo == contact &&
                               s.ConcernID == concernId, cancellationToken);

            if (isDuplicateContact)
            {
                throw new ConflictException("A Supplier with same contact no. already exists in the system. Please try with a different contact no.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            supplier.Code = request.Code.Trim();
        }

        supplier.Name = request.Name.Trim();
        supplier.OwnerName = request.OwnerName?.Trim();
        supplier.ContactNo = request.ContactNo?.Trim();
        supplier.Address = request.Address?.Trim();
        supplier.Remarks = request.Remarks?.Trim();
        supplier.ConcernID = concernId;

        if (request.PhotoPath != null)
        {
            supplier.PhotoPath = request.PhotoPath.Trim();
        }

        if (request.OpeningDue.HasValue)
        {
            supplier.OpeningDue = request.OpeningDue.Value;
        }

        if (request.TotalDue.HasValue)
        {
            supplier.TotalDue = request.TotalDue.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
