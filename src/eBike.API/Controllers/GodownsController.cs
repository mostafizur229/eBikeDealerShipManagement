using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Godowns.Commands;
using eBike.Application.Features.Godowns.DTOs;
using eBike.Application.Features.Godowns.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class GodownsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Godowns, PermissionAction.View)]
    public async Task<ActionResult<List<GodownDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        [FromQuery] bool? onlyCommon,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetGodownsQuery(search, concernId, onlyCommon), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Godowns, PermissionAction.View)]
    public async Task<ActionResult<GodownDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetGodownByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Godowns, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextGodownCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Godowns, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateGodownCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { godownId = id, message = "Godown has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Godowns, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateGodownCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Godown has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Godowns, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteGodownCommand(id), cancellationToken);
        return Ok(new { message = "Godown has been deleted successfully." });
    }
}
