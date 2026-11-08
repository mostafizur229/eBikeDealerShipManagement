using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.ExpenseItems.Commands;
using eBike.Application.Features.ExpenseItems.DTOs;
using eBike.Application.Features.ExpenseItems.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class ExpenseItemsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.ExpenseItems, PermissionAction.View)]
    public async Task<ActionResult<List<ExpenseItemListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? status,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetExpenseItemsQuery(search, status, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.ExpenseItems, PermissionAction.View)]
    public async Task<ActionResult<ExpenseItemDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetExpenseItemByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.ExpenseItems, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextExpenseItemCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.ExpenseItems, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateExpenseItemCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { expenseItemId = id, message = "Expense/Income Head has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.ExpenseItems, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateExpenseItemCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ExpenseItemID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Expense/Income Head has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.ExpenseItems, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteExpenseItemCommand(id), cancellationToken);
        return Ok(new { message = "Expense/Income Head has been deleted successfully." });
    }
}
