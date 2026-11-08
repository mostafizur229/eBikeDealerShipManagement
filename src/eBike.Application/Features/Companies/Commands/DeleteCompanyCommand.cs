using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Companies.Commands;

public record DeleteCompanyCommand(int Id) : IRequest;

public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCompanyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.CompanyID == request.Id, cancellationToken);

        if (company == null)
        {
            throw new NotFoundException(nameof(Company), request.Id);
        }

        var productCount = await _context.Products
            .CountAsync(p => p.CompanyID == request.Id, cancellationToken);

        if (productCount > 0)
        {
            throw new ConflictException($"Cannot delete company '{company.Name}' because {productCount} Product(s) are associated with it. Please reassign or delete the associated products first.");
        }

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
