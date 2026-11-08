using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Services.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Queries;

public record GetServiceTicketsQuery(
    ServiceStatus? Status = null,
    int? CustomerId = null,
    int? TechnicianId = null,
    string? SearchTerm = null
) : IRequest<List<ServiceTicketDto>>;

public class GetServiceTicketsQueryHandler : IRequestHandler<GetServiceTicketsQuery, List<ServiceTicketDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServiceTicketsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceTicketDto>> Handle(GetServiceTicketsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ServiceTickets
            .Include(s => s.Customer)
            .Include(s => s.EBike)
            .Include(s => s.AssignedTechnician)
            .Include(s => s.UsedParts)
                .ThenInclude(u => u.SparePart)
            .AsNoTracking()
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(s => s.Status == request.Status.Value);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(s => s.CustomerId == request.CustomerId.Value);
        }

        if (request.TechnicianId.HasValue)
        {
            query = query.Where(s => s.AssignedTechnicianId == request.TechnicianId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(s =>
                s.TicketNumber.ToLower().Contains(search) ||
                s.Customer.Name.ToLower().Contains(search) ||
                (s.Customer.ContactNo != null && s.Customer.ContactNo.Contains(search)) ||
                (s.EBike != null && s.EBike.ChassisNumber.ToLower().Contains(search)));
        }

        var tickets = await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        var customerIds = tickets.Select(t => t.CustomerId).Where(id => id > 0).Distinct().ToList();
        var customerVehicles = await _context.CustomerVehicles
            .AsNoTracking()
            .Where(v => customerIds.Contains(v.CustomerID))
            .ToListAsync(cancellationToken);

        var productIds = customerVehicles.Select(v => v.ProductID).Distinct().ToList();
        var partProductIds = tickets.SelectMany(t => t.UsedParts).Select(p => p.SparePartId).Where(id => id > 0).Distinct().ToList();
        var allProductIds = productIds.Union(partProductIds).Distinct().ToList();

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => allProductIds.Contains(p.ProductID))
            .ToDictionaryAsync(p => p.ProductID, cancellationToken);

        var vehicleMap = customerVehicles
            .GroupBy(v => v.CustomerID)
            .ToDictionary(g => g.Key, g => g.First());

        var techIds = tickets
            .Select(t => t.AssignedTechnicianId)
            .Where(id => id.HasValue && id.Value > 0)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var employees = await _context.Employees
            .AsNoTracking()
            .Where(e => techIds.Contains(e.EmployeeID))
            .ToDictionaryAsync(e => e.EmployeeID, cancellationToken);

        var ticketIds = tickets.Select(t => (int?)t.Id).ToList();
        var allChassis = tickets
            .Select(s => s.EBike?.ChassisNumber ?? (vehicleMap.TryGetValue(s.CustomerId, out var v) ? v.ChassisNo : null))
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();

        var checklists = await _context.VehiclePeriodicChecklists
            .AsNoTracking()
            .Where(c => (c.JobCardID.HasValue && ticketIds.Contains(c.JobCardID)) ||
                         (c.ChassisNo != null && allChassis.Contains(c.ChassisNo)))
            .ToListAsync(cancellationToken);

        return tickets.Select(s =>
        {
            var cv = vehicleMap.TryGetValue(s.CustomerId, out var v) ? v : null;
            string? chassis = s.EBike?.ChassisNumber ?? cv?.ChassisNo;
            string? model = s.EBike?.Model ?? (cv != null && products.TryGetValue(cv.ProductID, out var cvProd) ? cvProd.ProductName : null);
            string? bay = null;

            if (!string.IsNullOrEmpty(s.WorkNotes))
            {
                var lines = s.WorkNotes.Split('\n');
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("Bay: "))
                    {
                        bay = trimmed.Substring(5).Trim();
                    }
                    else if (string.IsNullOrEmpty(chassis) && trimmed.Contains("Chassis: "))
                    {
                        var parts = trimmed.Split("Chassis: ");
                        if (parts.Length > 1) chassis = parts[1].Split(' ')[0].Trim();
                    }
                    else if (string.IsNullOrEmpty(model) && trimmed.Contains("Model: "))
                    {
                        var parts = trimmed.Split("Model: ");
                        if (parts.Length > 1) model = parts[1].Trim();
                    }
                }
            }

            return new ServiceTicketDto
            {
                Id = s.Id,
                TicketNumber = s.TicketNumber,
                EBikeId = s.EBikeId,
                EBikeChassisNumber = chassis ?? "VIN-PENDING",
                EBikeModel = model ?? "Royal Enfield Classic 350",
                CustomerId = s.CustomerId,
                CustomerName = s.Customer != null ? s.Customer.Name : "Walk-in Customer",
                CustomerPhone = s.Customer != null ? (s.Customer.ContactNo ?? s.Customer.Phone ?? "") : "",
                CustomerAddress = s.Customer != null ? s.Customer.Address : null,
                NationalId = s.Customer != null ? s.Customer.NID : null,
                AssignedTechnicianId = s.AssignedTechnicianId,
                TechnicianName = s.AssignedTechnicianId.HasValue && employees.TryGetValue(s.AssignedTechnicianId.Value, out var emp)
                    ? emp.Name
                    : (s.AssignedTechnician != null ? s.AssignedTechnician.FullName : null),
                Status = s.Status,
                Priority = s.Priority,
                CurrentMileageKm = s.CurrentMileageKm,
                IssueDescription = s.IssueDescription,
                DiagnosisReport = s.DiagnosisReport,
                WorkNotes = s.WorkNotes,
                BayName = bay ?? "Bay 01 (Hydraulic Ramp)",
                LaborCost = s.LaborCost,
                PartsCost = s.PartsCost,
                Discount = s.Discount,
                TotalAmount = s.TotalAmount,
                PaidAmount = s.PaidAmount,
                CreatedAt = s.CreatedAt,
                PromisedDeliveryDate = s.PromisedDeliveryDate,
                SalesOrderId = s.SalesOrderId,
                SalesInvoiceNo = s.SalesInvoiceNo,
                IsChecklistCompleted = (checklists.FirstOrDefault(c => (c.JobCardID.HasValue && c.JobCardID.Value == s.Id) || (!string.IsNullOrEmpty(chassis) && c.ChassisNo == chassis)) != null),
                ChecklistId = checklists.FirstOrDefault(c => (c.JobCardID.HasValue && c.JobCardID.Value == s.Id) || (!string.IsNullOrEmpty(chassis) && c.ChassisNo == chassis))?.ChecklistID,
                UsedParts = s.UsedParts.Select(p =>
                {
                    products.TryGetValue(p.SparePartId, out var partProd);
                    return new ServiceTicketItemDto
                    {
                        Id = p.Id,
                        SparePartId = p.SparePartId,
                        PartCode = p.SparePart != null ? p.SparePart.PartCode : partProd?.Code,
                        PartName = p.SparePart != null ? p.SparePart.PartName : (partProd?.ProductName ?? partProd?.PartsName),
                        Quantity = p.Quantity,
                        UnitPrice = p.UnitPrice
                    };
                }).ToList()
            };
        }).ToList();
    }
}
