using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.PurchaseDOs.Commands;
using eBike.Application.Features.PurchaseDOs.DTOs;
using eBike.Application.Features.PurchaseDOs.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
[Route("api/purchase-do")]
[Route("api/purchase-does")]
public class PurchaseDOController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.PurchaseDO, PermissionAction.View)]
    public async Task<ActionResult<List<PurchaseDOListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? customerId,
        [FromQuery] int? supplierId,
        [FromQuery] int? status,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPurchaseDOsQuery(
            FromDate: fromDate,
            ToDate: toDate,
            CustomerId: customerId,
            SupplierId: supplierId,
            Status: status,
            Search: search,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.PurchaseDO, PermissionAction.View)]
    public async Task<ActionResult<PurchaseDODto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPurchaseDOByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Purchase DO with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("next-dono")]
    [HasPermission(ModuleNames.PurchaseDO, PermissionAction.View)]
    public async Task<ActionResult> GetNextDONo([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var doNo = await Sender.Send(new GetNextDONoQuery(concernId), cancellationToken);
        return Ok(new { doNo });
    }

    [HttpPost]
    [HasPermission(ModuleNames.PurchaseDO, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreatePurchaseDOCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { doid = id, message = "Purchase DO has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.PurchaseDO, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdatePurchaseDOCommand command, CancellationToken cancellationToken)
    {
        if (id != command.DOID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Purchase DO has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.PurchaseDO, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeletePurchaseDOCommand(id), cancellationToken);
        return Ok(new { message = "Purchase DO has been deleted successfully." });
    }
}
