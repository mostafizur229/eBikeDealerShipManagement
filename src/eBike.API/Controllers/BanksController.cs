using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Banks.Commands;
using eBike.Application.Features.Banks.DTOs;
using eBike.Application.Features.Banks.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class BanksController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Banks, PermissionAction.View)]
    public async Task<ActionResult<List<BankListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetBanksQuery(search, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Banks, PermissionAction.View)]
    public async Task<ActionResult<BankDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetBankByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Banks, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextBankCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Banks, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateBankCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { bankId = id, message = "Bank has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Banks, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateBankCommand command, CancellationToken cancellationToken)
    {
        if (id != command.BankID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Bank has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Banks, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteBankCommand(id), cancellationToken);
        return Ok(new { message = "Bank has been deleted successfully." });
    }
}
