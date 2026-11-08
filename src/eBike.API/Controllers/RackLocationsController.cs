using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.PLocations.Commands;
using eBike.Application.Features.PLocations.DTOs;
using eBike.Application.Features.PLocations.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
[Route("api/rack-locations")]
[Route("api/plocations")]
public class RackLocationsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.RackLocations, PermissionAction.View)]
    public async Task<ActionResult<List<PLocationDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPLocationsQuery(search, concernId, limit), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.RackLocations, PermissionAction.View)]
    public async Task<ActionResult<PLocationDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPLocationByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Rack Location with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.RackLocations, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextPLocationCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.RackLocations, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreatePLocationCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { pLocationId = id, message = "Rack Location has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.RackLocations, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdatePLocationCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Rack Location has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.RackLocations, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeletePLocationCommand(id), cancellationToken);
        return Ok(new { message = "Rack Location has been deleted successfully." });
    }
}
