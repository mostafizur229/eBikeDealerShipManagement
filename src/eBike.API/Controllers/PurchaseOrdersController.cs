using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.PurchaseOrders.Commands;
using eBike.Application.Features.PurchaseOrders.DTOs;
using eBike.Application.Features.PurchaseOrders.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class PurchaseOrdersController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.View)]
    public async Task<ActionResult<List<PurchaseOrderListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? supplierId,
        [FromQuery] int? status,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        [FromQuery] int? poType,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPurchaseOrdersQuery(
            FromDate: fromDate,
            ToDate: toDate,
            SupplierId: supplierId,
            Status: status,
            Search: search,
            ConcernId: concernId,
            POType: poType ?? 1
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.View)]
    public async Task<ActionResult<PurchaseOrderDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPurchaseOrderByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Purchase Order with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("{id:int}/invoice")]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.View)]
    public async Task<ActionResult<PurchaseOrderInvoiceDto>> GetInvoice(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPurchaseOrderInvoiceQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-challan-no")]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.View)]
    public async Task<ActionResult> GetNextChallanNo(
        [FromQuery] int? concernId, 
        [FromQuery] int poType = 1,
        CancellationToken cancellationToken = default)
    {
        var challanNo = await Sender.Send(new GetNextChallanNoQuery(concernId, poType), cancellationToken);
        return Ok(new { challanNo });
    }

    [HttpGet("check-chassis")]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.View)]
    public async Task<ActionResult<ChassisDuplicateCheckDto>> CheckChassis(
        [FromQuery] string imeno,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new CheckChassisDuplicateQuery(imeno, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.Create)]
    public async Task<ActionResult<PurchaseOrderDto>> Create(
        [FromBody] CreatePurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.POrderID }, result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.Update)]
    public async Task<ActionResult<PurchaseOrderDto>> Update(
        int id,
        [FromBody] UpdatePurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "URL ID does not match body ID." });
        }

        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeletePurchaseOrderCommand(id), cancellationToken);
        return Ok(new { message = "Purchase Order returned and stock reverted successfully." });
    }
}
