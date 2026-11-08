using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.BankTransactions.Commands;
using eBike.Application.Features.BankTransactions.DTOs;
using eBike.Application.Features.BankTransactions.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class BankTransactionsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.BankTransactions, PermissionAction.View)]
    public async Task<ActionResult<List<BankTransactionListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? transactionType,
        [FromQuery] int? bankId,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetBankTransactionsQuery(fromDate, toDate, transactionType, bankId, search, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.BankTransactions, PermissionAction.View)]
    public async Task<ActionResult<BankTransactionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetBankTransactionByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-no")]
    [HasPermission(ModuleNames.BankTransactions, PermissionAction.View)]
    public async Task<ActionResult> GetNextNo([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextBankTransactionNoQuery(concernId), cancellationToken);
        return Ok(new { transactionNo = code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.BankTransactions, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateBankTransactionCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { bankTranId = id, message = "Bank Transaction has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.BankTransactions, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateBankTransactionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.BankTranID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Bank Transaction has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.BankTransactions, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteBankTransactionCommand(id), cancellationToken);
        return Ok(new { message = "Bank Transaction has been deleted successfully." });
    }
}
