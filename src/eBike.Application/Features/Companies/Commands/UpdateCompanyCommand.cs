using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Companies.Commands;

public record UpdateCompanyCommand(
    int Id,
    string Code,
    string Name,
    int? ConcernId
) : IRequest;

public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid Company ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Company Code is required.")
            .MaximumLength(250).WithMessage("Company Code must not exceed 250 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company Name is required.")
            .MaximumLength(350).WithMessage("Company Name must not exceed 350 characters.");
    }
}

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCompanyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.CompanyID == request.Id, cancellationToken);

        if (company == null)
        {
            throw new NotFoundException(nameof(Company), request.Id);
        }

        int targetConcernId = request.ConcernId ?? company.ConcernID;

        // Duplicate name check for other companies in the same concern
        var isDuplicate = await _context.Companies
            .AnyAsync(c => c.CompanyID != request.Id &&
                           c.Name.ToLower() == request.Name.Trim().ToLower() &&
                           c.ConcernID == targetConcernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Company with same name already exists in the system. Please try with a different name.");
        }

        var oldName = company.Name.Trim();
        var newName = request.Name.Trim();

        company.Code = request.Code.Trim();
        company.Name = newName;
        company.ConcernID = targetConcernId;

        // Auto-replicate update to connected family concerns where company name matches
        if (!string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
        {
            var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
                .GetConnectedFamilyConcernsAsync(_context, company.ConcernID, cancellationToken);
            var familyConcernIds = familyConcerns.Select(f => f.ConcernID).ToList();

            var matchingOtherCompanies = await _context.Companies
                .Where(c => familyConcernIds.Contains(c.ConcernID) && c.Name.ToLower() == oldName.ToLower())
                .ToListAsync(cancellationToken);

            foreach (var other in matchingOtherCompanies)
            {
                other.Name = newName;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
