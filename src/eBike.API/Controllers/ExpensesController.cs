using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Expenditures.Commands;
using eBike.Application.Features.Expenditures.DTOs;
using eBike.Application.Features.Expenditures.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class ExpensesController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Expenses, PermissionAction.View)]
    public async Task<ActionResult<List<ExpenditureListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? expenseItemId,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetExpendituresQuery(
            Type: (int)EnumCompanyTransaction.Expense,
            FromDate: fromDate,
            ToDate: toDate,
            ExpenseItemId: expenseItemId,
            Search: search,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Expenses, PermissionAction.View)]
    public async Task<ActionResult<ExpenditureDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetExpenditureByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-voucher-no")]
    [HasPermission(ModuleNames.Expenses, PermissionAction.View)]
    public async Task<ActionResult> GetNextVoucherNo([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextVoucherNoQuery(concernId), cancellationToken);
        return Ok(new { voucherNo = code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Expenses, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateExpenditureCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { expenditureId = id, message = "Expense voucher has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Expenses, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateExpenditureCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ExpenditureID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Expense voucher has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Expenses, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteExpenditureCommand(id), cancellationToken);
        return Ok(new { message = "Expense voucher has been deleted successfully." });
    }
}
