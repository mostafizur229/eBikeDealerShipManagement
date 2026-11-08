using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Designations.Commands;
using eBike.Application.Features.Designations.DTOs;
using eBike.Application.Features.Designations.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class DesignationsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Designations, PermissionAction.View)]
    public async Task<ActionResult<List<DesignationListDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDesignationsQuery(search), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Designations, PermissionAction.View)]
    public async Task<ActionResult<DesignationDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDesignationByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Designations, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode(CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextDesignationCodeQuery(), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Designations, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateDesignationCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { designationId = id, message = "Designation has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Designations, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateDesignationCommand command, CancellationToken cancellationToken)
    {
        if (id != command.DesignationID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Designation has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Designations, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteDesignationCommand(id), cancellationToken);
        return Ok(new { message = "Designation has been deleted successfully." });
    }
}
