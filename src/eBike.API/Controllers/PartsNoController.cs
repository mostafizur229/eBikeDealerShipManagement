using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Parts.Commands;
using eBike.Application.Features.Parts.DTOs;
using eBike.Application.Features.Parts.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
[Route("api/parts-no")]
[Route("api/parts")]
public class PartsNoController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.PartsNo, PermissionAction.View)]
    public async Task<ActionResult<List<PartDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPartsQuery(search, concernId, limit), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.PartsNo, PermissionAction.View)]
    public async Task<ActionResult<PartDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPartByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Part with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.PartsNo, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextPartCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.PartsNo, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreatePartCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { partsNoId = id, message = "Part's No has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.PartsNo, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdatePartCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Part's No has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.PartsNo, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeletePartCommand(id), cancellationToken);
        return Ok(new { message = "Part's No has been deleted successfully." });
    }
}
