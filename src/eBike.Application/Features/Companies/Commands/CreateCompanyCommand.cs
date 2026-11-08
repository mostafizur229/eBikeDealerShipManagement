using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Companies.Commands;

public record CreateCompanyCommand(
    string? Code,
    string Name,
    int? ConcernId
) : IRequest<int>;

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company Name is required.")
            .MaximumLength(350).WithMessage("Company Name must not exceed 350 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(250).WithMessage("Company Code must not exceed 250 characters.");
    }
}

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateCompanyCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId 
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a company.");

        // Duplicate name check (case-insensitive) within the same concern
        var isDuplicate = await _context.Companies
            .AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower() && c.ConcernID == concernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Company with same name already exists in the system. Please try with a different name.");
        }

        // Auto-generate code if empty
        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextCompanyCodeQuery(concernId), cancellationToken);
        }

        var company = new Company
        {
            Code = code,
            Name = request.Name.Trim(),
            ConcernID = concernId
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);

        // Auto-replicate to connected family concerns (matching MCSRMIMSWEB GetFamilyTree)
        var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
            .GetConnectedFamilyConcernsAsync(_context, concernId, cancellationToken);

        foreach (var familyConcern in familyConcerns)
        {
            var existsInOther = await _context.Companies
                .AnyAsync(c => c.ConcernID == familyConcern.ConcernID && c.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

            if (!existsInOther)
            {
                var otherCode = await _sender.Send(new Queries.GetNextCompanyCodeQuery(familyConcern.ConcernID), cancellationToken);
                _context.Companies.Add(new Company
                {
                    Code = otherCode,
                    Name = request.Name.Trim(),
                    ConcernID = familyConcern.ConcernID
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return company.CompanyID;
    }
}
