using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Suppliers.Commands;

public record CreateSupplierCommand(
    string? Code,
    string Name,
    string? OwnerName = null,
    string? ContactNo = null,
    string? Address = null,
    string? PhotoPath = null,
    decimal? OpeningDue = 0,
    string? Remarks = null,
    int? ConcernId = null
) : IRequest<int>;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier Name is required.");

        RuleFor(x => x.ContactNo)
            .MaximumLength(150).WithMessage("Contact No must not exceed 150 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Supplier Code must not exceed 50 characters.");
    }
}

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateSupplierCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId 
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a supplier.");

        // Duplicate Name check within same concern
        var isDuplicateName = await _context.Suppliers
            .AnyAsync(s => s.Name != null && s.Name.ToLower() == request.Name.Trim().ToLower() && s.ConcernID == concernId, cancellationToken);

        if (isDuplicateName)
        {
            throw new ConflictException("A Supplier with same name already exists in the system. Please try with a different name.");
        }

        // Duplicate ContactNo check within same concern (if contact no is provided)
        if (!string.IsNullOrWhiteSpace(request.ContactNo))
        {
            var contact = request.ContactNo.Trim();
            var isDuplicateContact = await _context.Suppliers
                .AnyAsync(s => s.ContactNo != null && s.ContactNo == contact && s.ConcernID == concernId, cancellationToken);

            if (isDuplicateContact)
            {
                throw new ConflictException("A Supplier with same contact no. already exists in the system. Please try with a different contact no.");
            }
        }

        // Auto-generate Code if empty
        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextSupplierCodeQuery(concernId), cancellationToken);
        }

        var openingDue = request.OpeningDue ?? 0m;

        var supplier = new Supplier
        {
            Code = code,
            Name = request.Name.Trim(),
            OwnerName = request.OwnerName?.Trim(),
            ContactNo = request.ContactNo?.Trim(),
            Address = request.Address?.Trim(),
            PhotoPath = request.PhotoPath?.Trim(),
            TotalDue = openingDue,
            OpeningDue = openingDue,
            Remarks = request.Remarks?.Trim(),
            ConcernID = concernId
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken);

        return supplier.SupplierID;
    }
}
