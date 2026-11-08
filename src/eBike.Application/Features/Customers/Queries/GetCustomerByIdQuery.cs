using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Customers.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Customers.Queries;

public record GetCustomerByIdQuery(int Id) : IRequest<CustomerDto>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.District)
            .Include(c => c.ThanaObj)
            .Include(c => c.Employee)
            .Include(c => c.SisterConcern)
            .FirstOrDefaultAsync(c => c.CustomerID == request.Id, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), request.Id);
        }

        return new CustomerDto
        {
            CustomerID = customer.CustomerID,
            Code = customer.Code,
            Name = customer.Name,
            FName = customer.FName,
            MName = customer.MName,
            HusbandOrWifeName = customer.HusbandOrWifeName,
            CompanyName = customer.CompanyName,
            ContactNo = customer.ContactNo,
            OthersContactNo = customer.OthersContactNo,
            EmailID = customer.EmailID,
            NID = customer.NID,
            Address = customer.Address,
            PermanentAddress = customer.PermanentAddress,
            PhotoPath = customer.PhotoPath,
            OpeningDue = customer.OpeningDue,
            TotalDue = customer.TotalDue,
            CreditDue = customer.CreditDue,
            CusDueLimit = customer.CusDueLimit,
            RefName = customer.RefName,
            RefFName = customer.RefFName,
            RefContact = customer.RefContact,
            RefAddress = customer.RefAddress,
            CustomerType = customer.CustomerType,
            CustomerTypeName = customer.CustomerType == 1 ? "Retail" :
                               customer.CustomerType == 2 ? "Dealer" :
                               customer.CustomerType == 3 ? "Credit" : "Branch",
            EmployeeID = customer.EmployeeID,
            EmployeeName = customer.Employee?.Name,
            EmployeeCode = customer.Employee?.Code,
            ConcernID = customer.ConcernID,
            ConcernName = customer.SisterConcern?.Name,
            Remarks = customer.Remarks,
            Village = customer.Village,
            Post = customer.Post,
            SexType = customer.SexType,
            SexTypeName = customer.SexType == 1 ? "Male" : customer.SexType == 2 ? "Female" : "Other",
            DOB = customer.DOB,
            DistrictID = customer.DistrictID,
            DistrictName = customer.District?.Name,
            ThanaID = customer.ThanaID,
            ThanaName = customer.ThanaObj?.Name
        };
    }
}
