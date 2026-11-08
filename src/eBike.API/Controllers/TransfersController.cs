using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Transfers.Commands;
using eBike.Application.Features.Transfers.DTOs;
using eBike.Application.Features.Transfers.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
[Route("api/transfers")]
[Route("api/[controller]")]
public class TransfersController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Inventory, PermissionAction.View)]
    public async Task<ActionResult<List<TransferListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? concernId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetTransfersQuery(
            FromDate: fromDate,
            ToDate: toDate,
            ConcernId: concernId,
            Search: search
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Inventory, PermissionAction.View)]
    public async Task<ActionResult<TransferDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetTransferByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Transfer with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("concerns")]
    [HasPermission(ModuleNames.Inventory, PermissionAction.View)]
    public async Task<ActionResult<List<BranchConcernDto>>> GetConcerns(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetTransferConcernsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-transfer-no")]
    [HasPermission(ModuleNames.Inventory, PermissionAction.View)]
    public async Task<ActionResult<string>> GetNextTransferNo(
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetNextTransferNoQuery(concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("eligible-stock")]
    [HasPermission(ModuleNames.Inventory, PermissionAction.View)]
    public async Task<ActionResult<List<TransferEligibleStockDto>>> GetEligibleStock(
        [FromQuery] string? search,
        [FromQuery] int? godownId,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetTransferEligibleStockQuery(
            Search: search,
            GodownId: godownId,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("internal")]
    [HasPermission(ModuleNames.Inventory, PermissionAction.View)]
    public async Task<ActionResult<List<InternalTransferHistoryDto>>> GetInternalTransfers(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? concernId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetInternalTransfersQuery(
            FromDate: fromDate,
            ToDate: toDate,
            ConcernId: concernId,
            Search: search
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.Inventory, PermissionAction.Create)]
    public async Task<ActionResult<TransferDto>> Create(
        [FromBody] CreateTransferCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.TransferId }, result);
    }

    [HttpPost("internal")]
    [HasPermission(ModuleNames.Inventory, PermissionAction.Create)]
    public async Task<ActionResult<bool>> CreateInternal(
        [FromBody] CreateInternalTransferCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Inventory, PermissionAction.Delete)]
    public async Task<ActionResult<bool>> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteTransferCommand(id), cancellationToken);
        return Ok(result);
    }
}
