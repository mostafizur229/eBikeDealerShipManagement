using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.SalesOrders.DTOs;
using eBike.Application.Features.SalesOrders.Queries;
using eBike.Application.Features.Services.Commands;
using eBike.Application.Features.Services.DTOs;
using eBike.Application.Features.Services.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class ServicesController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.View)]
    public async Task<ActionResult<List<ServiceTicketDto>>> GetAll(
        [FromQuery] ServiceStatus? status,
        [FromQuery] int? customerId,
        [FromQuery] int? technicianId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetServiceTicketsQuery(status, customerId, technicianId, search), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Create)]
    public async Task<ActionResult<int>> Create([FromBody] CreateServiceTicketCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { serviceTicketId = id, message = "Service ticket opened successfully." });
    }

    [HttpPost("assign-parts")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Update)]
    public async Task<ActionResult<AssignTechnicianAndPartsResult>> AssignTechnicianAndParts(
        [FromBody] AssignTechnicianAndPartsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("sold-bike-customers")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.View)]
    public async Task<ActionResult<List<SoldBikeCustomerDto>>> GetSoldBikeCustomers(
        [FromQuery] string? search,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetSoldBikeCustomersQuery(search, limit), cancellationToken);
        return Ok(result);
    }

    [HttpPost("call-followup")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Update)]
    public async Task<ActionResult<bool>> LogCallFollowUp(
        [FromBody] LogCallFollowUpCommand command,
        CancellationToken cancellationToken)
    {
        var success = await Sender.Send(command, cancellationToken);
        return Ok(new { success, message = "Call follow-up and next schedule date saved successfully." });
    }

    [HttpGet("call-history")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.View)]
    public async Task<ActionResult<List<ServiceFollowUpLogDto>>> GetCallHistory(
        [FromQuery] int? vehicleId,
        [FromQuery] int? customerId,
        [FromQuery] string? chassisNo,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCustomerCallHistoryQuery(vehicleId, customerId, chassisNo), cancellationToken);
        return Ok(result);
    }

    [HttpPost("vehicle-schedule")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Create)]
    public async Task<ActionResult<int>> RegisterVehicleSchedule(
        [FromBody] RegisterCustomerVehicleCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { vehicleId = id, message = "Vehicle service schedule registered successfully." });
    }

    [HttpPost("advance-milestone")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Update)]
    public async Task<ActionResult<bool>> AdvanceMilestone(
        [FromBody] AdvanceVehicleMilestoneCommand command,
        CancellationToken cancellationToken)
    {
        var success = await Sender.Send(command, cancellationToken);
        return Ok(new { success, message = "Service milestone completed and next periodic service scheduled successfully." });
    }

    [HttpGet("due-schedules")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.View)]
    public async Task<ActionResult<List<ServiceDueDto>>> GetDueSchedules(
        [FromQuery] string? statusFilter,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetServiceDueVehiclesQuery(statusFilter, search), cancellationToken);
        return Ok(result);
    }

    [HttpGet("in-stock-parts")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.View)]
    public async Task<ActionResult<List<InStockPartDto>>> GetInStockParts(
        [FromQuery] string? search,
        [FromQuery] int? productId,
        [FromQuery] int? godownId,
        [FromQuery] int? limit,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetInStockPartsQuery(
            Search: search,
            ProductId: productId,
            GodownId: godownId,
            Limit: limit,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Update)]
    public async Task<ActionResult<bool>> UpdateStatus(
        int id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken)
    {
        var success = await Sender.Send(new UpdateServiceTicketStatusCommand(id, request.Status, request.WorkNotes), cancellationToken);
        return Ok(new { success, message = "Service ticket status updated successfully." });
    }

    [HttpPost("{id}/payment")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Update)]
    public async Task<ActionResult<decimal>> CollectPayment(
        int id,
        [FromBody] CollectPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var paid = await Sender.Send(new CollectServicePaymentCommand(
            id,
            request.Amount,
            request.UpdatedLaborCost,
            request.UpdatedDiscount,
            request.PaymentMethod ?? "Cash",
            request.Remarks
        ), cancellationToken);

        return Ok(new { paidAmount = paid, message = "Payment collected and recorded successfully." });
    }

    [HttpGet("checklists")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.View)]
    public async Task<ActionResult<List<VehiclePeriodicChecklistDto>>> GetChecklists(
        [FromQuery] string? chassisNo,
        [FromQuery] int? customerId,
        [FromQuery] int? jobCardId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetVehiclePeriodicChecklistsQuery(chassisNo, customerId, jobCardId, search), cancellationToken);
        return Ok(result);
    }

    [HttpPost("checklists")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Create)]
    public async Task<ActionResult<int>> SaveChecklist(
        [FromBody] SaveVehiclePeriodicChecklistCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { checklistId = id, message = "Royal Enfield 29-Point Digital Periodic Checklist saved successfully." });
    }

    [HttpGet("bays")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.View)]
    public async Task<ActionResult<List<WorkshopBayDto>>> GetWorkshopBays(
        [FromQuery] bool onlyActive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetWorkshopBaysQuery(onlyActive), cancellationToken);
        return Ok(result);
    }

    [HttpPost("bays")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Update)]
    public async Task<ActionResult<int>> SaveWorkshopBay(
        [FromBody] SaveWorkshopBayCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { bayId = id, message = "Workshop Bay saved successfully." });
    }

    [HttpDelete("bays/{id}")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Delete)]
    public async Task<ActionResult<bool>> DeleteWorkshopBay(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteWorkshopBayCommand(id), cancellationToken);
        return Ok(new { success = result });
    }

    [HttpGet("slots")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.View)]
    public async Task<ActionResult<List<WorkshopTimeSlotDto>>> GetWorkshopTimeSlots(
        [FromQuery] bool onlyActive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetWorkshopTimeSlotsQuery(onlyActive), cancellationToken);
        return Ok(result);
    }

    [HttpPost("slots")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Update)]
    public async Task<ActionResult<int>> SaveWorkshopTimeSlot(
        [FromBody] SaveWorkshopTimeSlotCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { slotId = id, message = "Workshop Time Slot & Capacity saved successfully." });
    }

    [HttpDelete("slots/{id}")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Delete)]
    public async Task<ActionResult<bool>> DeleteWorkshopTimeSlot(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteWorkshopTimeSlotCommand(id), cancellationToken);
        return Ok(new { success = result });
    }

    [HttpGet("customer-ratings")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.View)]
    public async Task<ActionResult<List<CustomerServiceRatingDto>>> GetCustomerRatings(
        [FromQuery] string? search,
        [FromQuery] int? minRating,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetCustomerServiceRatingsQuery(search, minRating, limit), cancellationToken);
        return Ok(result);
    }

    [HttpPost("customer-ratings")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Create)]
    public async Task<ActionResult<int>> SubmitCustomerRating(
        [FromBody] SubmitCustomerServiceRatingCommand command,
        CancellationToken cancellationToken)
    {
        var ratingId = await Sender.Send(command, cancellationToken);
        return Ok(new { ratingId, message = "Customer rating submitted successfully." });
    }

    [HttpGet("call-feedbacks")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.View)]
    public async Task<ActionResult<List<PostServiceCallFeedbackDto>>> GetCallFeedbacks(
        [FromQuery] string? search,
        [FromQuery] bool? onlyEscalated,
        [FromQuery] string? resolutionStatus,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPostServiceCallFeedbacksQuery(search, onlyEscalated, resolutionStatus, limit), cancellationToken);
        return Ok(result);
    }

    [HttpPost("call-feedbacks")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Create)]
    public async Task<ActionResult<int>> LogCallFeedback(
        [FromBody] LogPostServiceCallFeedbackCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { feedbackId = id, message = "Call feedback recorded successfully." });
    }

    [HttpPost("call-feedbacks/{id}/resolve")]
    [HasPermission(ModuleNames.ServiceTickets, PermissionAction.Update)]
    public async Task<ActionResult<bool>> ResolveManagerEscalation(
        int id,
        [FromBody] ResolveManagerEscalationCommand command,
        CancellationToken cancellationToken)
    {
        command.FeedbackId = id;
        var result = await Sender.Send(command, cancellationToken);
        return Ok(new { success = result, message = "Escalation resolved successfully." });
    }
}
public class UpdateStatusRequest
{
    public ServiceStatus Status { get; set; }
    public string? WorkNotes { get; set; }
}

public class CollectPaymentRequest
{
    public decimal Amount { get; set; }
    public decimal? UpdatedLaborCost { get; set; }
    public decimal? UpdatedDiscount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Remarks { get; set; }
}
