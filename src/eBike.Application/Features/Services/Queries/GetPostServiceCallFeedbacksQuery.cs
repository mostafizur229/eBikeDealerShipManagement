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

public record GetPostServiceCallFeedbacksQuery(
    string? Search = null,
    bool? OnlyEscalated = null,
    string? ResolutionStatus = null,
    int Limit = 100
) : IRequest<List<PostServiceCallFeedbackDto>>;

public class GetPostServiceCallFeedbacksQueryHandler : IRequestHandler<GetPostServiceCallFeedbacksQuery, List<PostServiceCallFeedbackDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPostServiceCallFeedbacksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PostServiceCallFeedbackDto>> Handle(GetPostServiceCallFeedbacksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PostServiceCallFeedbacks
            .AsNoTracking()
            .OrderByDescending(f => f.CallDateTime)
            .AsQueryable();

        if (request.OnlyEscalated.HasValue && request.OnlyEscalated.Value)
        {
            query = query.Where(f => f.IsEscalatedToManager || f.ManagerResolutionStatus == "Open" || f.ManagerResolutionStatus == "Under Investigation");
        }

        if (!string.IsNullOrWhiteSpace(request.ResolutionStatus))
        {
            query = query.Where(f => f.ManagerResolutionStatus == request.ResolutionStatus);
        }

        var rawList = await query.Take(request.Limit).ToListAsync(cancellationToken);

        if (rawList.Count == 0)
        {
            return new List<PostServiceCallFeedbackDto>();
        }

        var jobCardIds = rawList.Select(f => f.JobCardID).Distinct().ToList();
        var tickets = await _context.ServiceTickets
            .Include(t => t.Customer)
            .Include(t => t.EBike)
            .AsNoTracking()
            .Where(t => jobCardIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t, cancellationToken);

        var custIds = rawList.Select(f => f.CustomerID).Distinct().ToList();
        var customers = await _context.Customers
            .AsNoTracking()
            .Where(c => custIds.Contains(c.CustomerID))
            .ToDictionaryAsync(c => c.CustomerID, c => c, cancellationToken);

        var search = request.Search?.Trim().ToLower();

        var result = new List<PostServiceCallFeedbackDto>();

        foreach (var f in rawList)
        {
            tickets.TryGetValue(f.JobCardID, out var ticket);
            customers.TryGetValue(f.CustomerID, out var customer);

            var ticketNumber = ticket?.TicketNumber ?? $"JC-RE-2026-{f.JobCardID:D4}";
            var customerName = customer?.Name ?? ticket?.Customer?.Name ?? "Customer";
            var customerPhone = customer?.ContactNo ?? ticket?.Customer?.ContactNo ?? "";
            var bikeModel = ticket?.EBike?.Model ?? "Classic 350";
            var serviceDate = ticket?.CreatedAt;

            if (!string.IsNullOrWhiteSpace(search))
            {
                bool matches = ticketNumber.ToLower().Contains(search) ||
                               customerName.ToLower().Contains(search) ||
                               customerPhone.Contains(search) ||
                               bikeModel.ToLower().Contains(search) ||
                               (f.DetailedCustomerVerbatim != null && f.DetailedCustomerVerbatim.ToLower().Contains(search)) ||
                               (f.ManagerEscalationReason != null && f.ManagerEscalationReason.ToLower().Contains(search));

                if (!matches) continue;
            }

            result.Add(new PostServiceCallFeedbackDto
            {
                FeedbackId = f.FeedbackID,
                JobCardId = f.JobCardID,
                TicketNumber = ticketNumber,
                CustomerId = f.CustomerID,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                BikeModel = bikeModel,
                ServiceDate = serviceDate,
                CallerEmployeeId = f.CallerEmployeeID,
                CallDateTime = f.CallDateTime,
                CallStatus = f.CallStatus,
                IsBikeRunningSmoothly = f.IsBikeRunningSmoothly,
                IsReportedIssueResolved = f.IsReportedIssueResolved,
                WasDeliveredOnTimeAndClean = f.WasDeliveredOnTimeAndClean,
                AnyUnofficialChargeDemanded = f.AnyUnofficialChargeDemanded,
                CsiScore = f.CSI_Score,
                DetailedCustomerVerbatim = f.DetailedCustomerVerbatim,
                IsEscalatedToManager = f.IsEscalatedToManager,
                ManagerEscalationReason = f.ManagerEscalationReason,
                ManagerResolutionStatus = f.ManagerResolutionStatus,
                ManagerActionNotes = f.ManagerActionNotes,
                ResolvedDateTime = f.ResolvedDateTime
            });
        }

        return result;
    }
}
