using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Companies.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Companies.Queries;

public record GetCompanyByIdQuery(int Id) : IRequest<CompanyDto>;

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
{
    private readonly IApplicationDbContext _context;

    public GetCompanyByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .Include(c => c.SisterConcern)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CompanyID == request.Id, cancellationToken);

        if (company == null)
        {
            throw new NotFoundException(nameof(Company), request.Id);
        }

        return new CompanyDto
        {
            Id = company.CompanyID,
            Code = company.Code,
            Name = company.Name,
            ConcernId = company.ConcernID,
            ConcernName = company.SisterConcern != null ? company.SisterConcern.Name : null
        };
    }
}
