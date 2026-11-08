using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.CashDeliveries.Commands;
using eBike.Application.Features.CashDeliveries.DTOs;
using eBike.Application.Features.CashDeliveries.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class CashDeliveriesController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.CashDeliveries, PermissionAction.View)]
    public async Task<ActionResult<List<CashDeliveryListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? supplierId,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCashDeliveriesQuery(
            FromDate: fromDate,
            ToDate: toDate,
            SupplierId: supplierId,
            Search: search,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.CashDeliveries, PermissionAction.View)]
    public async Task<ActionResult<CashDeliveryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCashDeliveryByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Cash Delivery with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("next-receipt-no")]
    [HasPermission(ModuleNames.CashDeliveries, PermissionAction.View)]
    public async Task<ActionResult> GetNextReceiptNo([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var receiptNo = await Sender.Send(new GetNextCashDeliveryReceiptNoQuery(concernId), cancellationToken);
        return Ok(new { receiptNo });
    }

    [HttpPost]
    [HasPermission(ModuleNames.CashDeliveries, PermissionAction.Create)]
    public async Task<ActionResult<CashDeliveryDto>> Create(
        [FromBody] CreateCashDeliveryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CashCollectionID },
            new { data = result, message = "Cash delivery has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.CashDeliveries, PermissionAction.Update)]
    public async Task<ActionResult<CashDeliveryDto>> Update(
        int id,
        [FromBody] UpdateCashDeliveryCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        var result = await Sender.Send(command, cancellationToken);
        return Ok(new { data = result, message = "Cash delivery has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.CashDeliveries, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteCashDeliveryCommand(id), cancellationToken);
        return Ok(new { message = "Cash delivery has been deleted successfully." });
    }
}
