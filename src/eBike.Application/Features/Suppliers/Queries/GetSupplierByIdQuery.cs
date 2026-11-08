using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Suppliers.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Suppliers.Queries;

public record GetSupplierByIdQuery(int Id) : IRequest<SupplierDto>;

public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, SupplierDto>
{
    private readonly IApplicationDbContext _context;

    public GetSupplierByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierDto> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Include(s => s.SisterConcern)
            .FirstOrDefaultAsync(s => s.SupplierID == request.Id, cancellationToken);

        if (supplier == null)
        {
            throw new NotFoundException(nameof(Supplier), request.Id);
        }

        return new SupplierDto
        {
            SupplierID = supplier.SupplierID,
            Code = supplier.Code,
            Name = supplier.Name,
            OwnerName = supplier.OwnerName,
            ContactNo = supplier.ContactNo,
            Address = supplier.Address,
            PhotoPath = supplier.PhotoPath,
            TotalDue = supplier.TotalDue,
            ConcernID = supplier.ConcernID,
            ConcernName = supplier.SisterConcern?.Name,
            OpeningDue = supplier.OpeningDue,
            Remarks = supplier.Remarks
        };
    }
}
