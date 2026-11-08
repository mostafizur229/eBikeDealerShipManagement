using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.PurchaseReturns.Commands;
using eBike.Application.Features.PurchaseReturns.DTOs;
using eBike.Application.Features.PurchaseReturns.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
[Route("api/purchase-returns")]
[Route("api/[controller]")]
public class PurchaseReturnsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.View)]
    public async Task<ActionResult<List<PurchaseReturnListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? supplierId,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPurchaseReturnsQuery(
            FromDate: fromDate,
            ToDate: toDate,
            SupplierId: supplierId,
            Search: search,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.View)]
    public async Task<ActionResult<PurchaseReturnDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPurchaseReturnByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Purchase Return with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("next-challan-no")]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.View)]
    public async Task<ActionResult> GetNextChallanNo(
        [FromQuery] int? concernId,
        CancellationToken cancellationToken = default)
    {
        var challanNo = await Sender.Send(new GetNextReturnChallanNoQuery(concernId), cancellationToken);
        return Ok(new { challanNo });
    }

    [HttpPost]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.Create)]
    public async Task<ActionResult<PurchaseReturnDto>> Create(
        [FromBody] CreatePurchaseReturnCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.POrderID }, result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.PurchaseOrders, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeletePurchaseReturnCommand(id), cancellationToken);
        return Ok(new { message = "Purchase Return reverted and stock restored successfully." });
    }
}
