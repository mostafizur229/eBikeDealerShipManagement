using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.CashCollections.Commands;
using eBike.Application.Features.CashCollections.DTOs;
using eBike.Application.Features.CashCollections.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class CashCollectionsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.CashCollections, PermissionAction.View)]
    public async Task<ActionResult<List<CashCollectionListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? customerId,
        [FromQuery] int? transactionType,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        [FromQuery] int? projectType,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCashCollectionsQuery(
            FromDate: fromDate,
            ToDate: toDate,
            CustomerId: customerId,
            TransactionType: transactionType,
            Search: search,
            ConcernId: concernId,
            ProjectType: projectType
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.CashCollections, PermissionAction.View)]
    public async Task<ActionResult<CashCollectionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCashCollectionByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Cash Collection with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("{id:int}/money-receipt")]
    [HasPermission(ModuleNames.CashCollections, PermissionAction.View)]
    public async Task<ActionResult<CashCollectionMoneyReceiptDto>> GetMoneyReceipt(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCashCollectionMoneyReceiptQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-receipt-no")]
    [HasPermission(ModuleNames.CashCollections, PermissionAction.View)]
    public async Task<ActionResult> GetNextReceiptNo(
        [FromQuery] int? concernId,
        [FromQuery] int? projectType,
        CancellationToken cancellationToken)
    {
        var receiptNo = await Sender.Send(new GetNextCashCollectionReceiptNoQuery(concernId), cancellationToken);
        return Ok(new { receiptNo });
    }

    [HttpPost]
    [HasPermission(ModuleNames.CashCollections, PermissionAction.Create)]
    public async Task<ActionResult<CashCollectionDto>> Create(
        [FromBody] CreateCashCollectionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CashCollectionID },
            new { data = result, message = "Cash collection has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.CashCollections, PermissionAction.Update)]
    public async Task<ActionResult<CashCollectionDto>> Update(
        int id,
        [FromBody] UpdateCashCollectionCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        var result = await Sender.Send(command, cancellationToken);
        return Ok(new { data = result, message = "Cash collection has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.CashCollections, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteCashCollectionCommand(id), cancellationToken);
        return Ok(new { message = "Cash collection has been deleted successfully." });
    }
}
