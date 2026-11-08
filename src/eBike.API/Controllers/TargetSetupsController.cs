using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.TargetSetups.Commands;
using eBike.Application.Features.TargetSetups.DTOs;
using eBike.Application.Features.TargetSetups.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class TargetSetupsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.TargetSetups, PermissionAction.View)]
    public async Task<ActionResult<List<TargetSetupListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? employeeId,
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetTargetSetupsQuery(search, employeeId, year, month), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.TargetSetups, PermissionAction.View)]
    public async Task<ActionResult<TargetSetupDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetTargetSetupByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.TargetSetups, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateTargetSetupCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { targetId = id, message = "Target Setup has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.TargetSetups, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateTargetSetupCommand command, CancellationToken cancellationToken)
    {
        if (id != command.TID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Target Setup has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.TargetSetups, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteTargetSetupCommand(id), cancellationToken);
        return Ok(new { message = "Target Setup has been deleted successfully." });
    }
}
