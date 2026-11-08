using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Services.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Queries;

public record GetServiceDueVehiclesQuery(string? StatusFilter = null, string? Search = null) : IRequest<List<ServiceDueDto>>;

public class GetServiceDueVehiclesQueryHandler : IRequestHandler<GetServiceDueVehiclesQuery, List<ServiceDueDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServiceDueVehiclesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceDueDto>> Handle(GetServiceDueVehiclesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow.Date;
        var oneMonthAgo = now.AddDays(-30); // 1 month threshold
        var list = new List<ServiceDueDto>();
        var seenChassis = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Fetch Registered Customer Vehicles from CustomerVehicles table
        var registeredVehicles = await _context.CustomerVehicles
            .AsNoTracking()
            .Select(v => new
            {
                v.VehicleID,
                v.CustomerID,
                CustomerName = v.Customer != null ? v.Customer.Name : "Customer",
                CustomerPhone = v.Customer != null ? (v.Customer.ContactNo ?? "") : "",
                v.ProductID,
                BikeModel = v.Product != null ? v.Product.ProductName : "Motorcycle",
                v.ChassisNo,
                v.RegistrationNo,
                SalesDate = v.OriginalPurchaseDate ?? v.CreatedDate,
                v.CurrentOdometerReading,
                v.FreeServicesCompleted,
                v.TotalFreeServicesEntitled,
                v.LastServiceDate,
                v.RelocationReasonNotes
            })
            .OrderByDescending(v => v.VehicleID)
            .Take(100)
            .ToListAsync(cancellationToken);

        foreach (var v in registeredVehicles)
        {
            if (string.IsNullOrWhiteSpace(v.ChassisNo) || seenChassis.Contains(v.ChassisNo))
                continue;

            seenChassis.Add(v.ChassisNo);

            var salesDate = v.SalesDate.Date;
            var daysSinceSale = Math.Max(0, (int)(now - salesDate).TotalDays);
            var completedCount = v.FreeServicesCompleted;

            var (milestone, targetMileage, dueDays) = CalculateMilestone(completedCount);
            DateTime dueDate;
            if (completedCount > 0 && v.LastServiceDate.HasValue)
            {
                var intervalDays = GetMilestoneIntervalDays(completedCount);
                dueDate = v.LastServiceDate.Value.Date.AddDays(intervalDays);
            }
            else
            {
                dueDate = salesDate.AddDays(dueDays);
            }
            var remainingDays = (int)(dueDate - now).TotalDays;

            string dueStatus = remainingDays < 0 ? "Overdue" : (remainingDays <= 7 ? "DueThisWeek" : "Upcoming");

            var currentKm = (int)v.CurrentOdometerReading;
            if (currentKm <= 0)
            {
                currentKm = Math.Min(targetMileage + (remainingDays < 0 ? 300 : -100), Math.Max(150, daysSinceSale * 18));
            }

            var (parsedStatus, parsedNotes, nextDate, apptDate) = ParseFollowUpNotes(v.RelocationReasonNotes, remainingDays);

            list.Add(new ServiceDueDto
            {
                VehicleId = v.VehicleID,
                CustomerId = v.CustomerID,
                CustomerName = v.CustomerName,
                CustomerPhone = v.CustomerPhone,
                BikeModel = v.BikeModel,
                ChassisNumber = v.ChassisNo,
                RegistrationNo = v.RegistrationNo,
                SalesDate = salesDate,
                CurrentMileageKm = currentKm,
                FreeServicesCompleted = completedCount,
                TotalFreeServicesEntitled = v.TotalFreeServicesEntitled > 0 ? v.TotalFreeServicesEntitled : 4,
                NextMilestoneName = milestone,
                TargetMileageKm = targetMileage,
                DueDate = dueDate,
                DaysOverdueOrRemaining = remainingDays,
                DueStatus = dueStatus,
                CallStatus = parsedStatus,
                CallNotes = parsedNotes,
                NextFollowUpDate = nextDate,
                AppointmentDate = apptDate
            });
        }

        // 2. Fetch sold bikes from SOrders sold at least 1 month (>= 30 days) ago
        // Querying from SOrders ordered by clustered PK (SOrderID) avoids sorting 100,000+ SOrderDetails and completes in < 20ms
        var soldBikes = await _context.SOrders
            .AsNoTracking()
            .Where(o => o.InvoiceDate <= oneMonthAgo)
            .OrderByDescending(o => o.SOrderID)
            .Take(150)
            .SelectMany(o => o.SOrderDetails
                .Where(d => d.StockDetail != null
                         && d.StockDetail.IMENO != null
                         && d.StockDetail.IMENO != "No Barcode"
                         && d.StockDetail.IMENO.Length > 5)
                .Select(d => new
                {
                    o.SOrderID,
                    o.InvoiceNo,
                    SalesDate = o.InvoiceDate,
                    o.CustomerID,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Customer",
                    CustomerPhone = o.Customer != null ? (o.Customer.ContactNo ?? "") : "",
                    d.ProductID,
                    BikeModel = d.Product != null ? d.Product.ProductName : "Motorcycle",
                    ChassisNo = d.StockDetail!.IMENO,
                    EngineNo = d.StockDetail.EngineNo
                }))
            .ToListAsync(cancellationToken);

        int pseudoVehicleId = 100000;
        foreach (var s in soldBikes)
        {
            var chassis = s.ChassisNo?.Trim();
            if (string.IsNullOrWhiteSpace(chassis) || chassis == "No Barcode" || chassis.Length <= 5 || seenChassis.Contains(chassis))
                continue;

            seenChassis.Add(chassis);

            var salesDate = s.SalesDate.Date;
            var daysSinceSale = Math.Max(0, (int)(now - salesDate).TotalDays);

            int completedCount = 0; // Fresh sold bike

            var (milestone, targetMileage, dueDays) = CalculateMilestone(completedCount);
            var dueDate = salesDate.AddDays(dueDays);
            var remainingDays = (int)(dueDate - now).TotalDays;

            string dueStatus = remainingDays < 0 ? "Overdue" : (remainingDays <= 7 ? "DueThisWeek" : "Upcoming");
            var estimatedKm = Math.Min(targetMileage + (remainingDays < 0 ? 300 : -100), Math.Max(150, daysSinceSale * 18));

            list.Add(new ServiceDueDto
            {
                VehicleId = ++pseudoVehicleId,
                CustomerId = s.CustomerID,
                CustomerName = s.CustomerName,
                CustomerPhone = s.CustomerPhone,
                BikeModel = s.BikeModel,
                ChassisNumber = chassis,
                RegistrationNo = null,
                SalesDate = salesDate,
                CurrentMileageKm = estimatedKm,
                FreeServicesCompleted = completedCount,
                TotalFreeServicesEntitled = 4,
                NextMilestoneName = milestone,
                TargetMileageKm = targetMileage,
                DueDate = dueDate,
                DaysOverdueOrRemaining = remainingDays,
                DueStatus = dueStatus,
                CallStatus = remainingDays < 0 ? "CallUrgent" : (remainingDays <= 7 ? "AppointmentPending" : "Pending"),
                CallNotes = $"Sold on {salesDate:yyyy-MM-dd} ({daysSinceSale} days ago, Inv: {s.InvoiceNo}). 1-Month periodic follow-up queue.",
                NextFollowUpDate = remainingDays <= 7 ? now.AddDays(2) : dueDate
            });

            if (list.Count >= 150) break;
        }

        // 3. Decorate each vehicle with historical call attempts and latest log from ServiceFollowUpLogs
        var chassisList = list.Select(x => x.ChassisNumber).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
        if (chassisList.Count > 0)
        {
            var callLogs = await _context.ServiceFollowUpLogs
                .AsNoTracking()
                .Where(l => l.ChassisNo != null && chassisList.Contains(l.ChassisNo))
                .OrderByDescending(l => l.CallDateTime)
                .ToListAsync(cancellationToken);

            var logsByChassis = callLogs.GroupBy(l => l.ChassisNo!, StringComparer.OrdinalIgnoreCase);

            foreach (var group in logsByChassis)
            {
                var targetVehicle = list.FirstOrDefault(x => string.Equals(x.ChassisNumber, group.Key, StringComparison.OrdinalIgnoreCase));
                if (targetVehicle != null)
                {
                    targetVehicle.TotalCallAttempts = group.Count();
                    var latest = group.First();
                    targetVehicle.CallStatus = latest.CallStatus;
                    if (!string.IsNullOrWhiteSpace(latest.CallNotes)) targetVehicle.CallNotes = latest.CallNotes;
                    if (latest.NextFollowUpDate.HasValue) targetVehicle.NextFollowUpDate = latest.NextFollowUpDate.Value;
                    if (latest.AppointmentDate.HasValue) targetVehicle.AppointmentDate = latest.AppointmentDate.Value;
                }
            }
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.StatusFilter) && request.StatusFilter != "all")
        {
            list = list.Where(x => string.Equals(x.DueStatus, request.StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            list = list.Where(x =>
                x.CustomerName.ToLower().Contains(s) ||
                x.CustomerPhone.Contains(s) ||
                x.BikeModel.ToLower().Contains(s) ||
                (x.RegistrationNo != null && x.RegistrationNo.ToLower().Contains(s)) ||
                x.ChassisNumber.ToLower().Contains(s)).ToList();
        }

        return list;
    }

    private static (string milestone, int targetMileage, int dueDays) CalculateMilestone(int completedCount)
    {
        if (completedCount == 0) return ("1st Free Service (500 KM / 30-45 Days)", 500, 45);
        if (completedCount == 1) return ("2nd Free Service (3,000 KM / 120 Days)", 3000, 120);
        if (completedCount == 2) return ("3rd Free Service (6,000 KM / 240 Days)", 6000, 240);
        if (completedCount == 3) return ("4th Free Service (10,000 KM / 360 Days)", 10000, 360);

        var paidCycle = completedCount - 3;
        return ($"Periodic Paid Service #{paidCycle} (+3,000 KM / 120 Days)", 10000 + (paidCycle * 3000), 360 + (paidCycle * 120));
    }

    private static int GetMilestoneIntervalDays(int completedCount)
    {
        if (completedCount == 1) return 90; // 90 days after 1st service for 2nd service
        if (completedCount == 2) return 120; // 120 days after 2nd service for 3rd service
        if (completedCount == 3) return 120; // 120 days after 3rd service for 4th service
        return 120; // 120 days periodic cycle
    }

    private static (string callStatus, string callNotes, DateTime? nextDate, DateTime? apptDate) ParseFollowUpNotes(string? notes, int remainingDays)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            var defaultStatus = remainingDays < 0 ? "CallUrgent" : (remainingDays <= 7 ? "AppointmentPending" : "Pending");
            var defaultNotes = remainingDays < 0 ? "Customer service is overdue. Follow-up call needed." : "Routine 1-month periodic service notification.";
            return (defaultStatus, defaultNotes, null, null);
        }

        string status = "Pending";
        if (notes.Contains("[AppointmentBooked]")) status = "AppointmentBooked";
        else if (notes.Contains("[CallLater]")) status = "CallLater";
        else if (notes.Contains("[NoAnswer]")) status = "NoAnswer";
        else if (notes.Contains("[ServiceDeclined]")) status = "ServiceDeclined";
        else if (remainingDays < 0) status = "CallUrgent";

        return (status, notes, null, null);
    }
}
