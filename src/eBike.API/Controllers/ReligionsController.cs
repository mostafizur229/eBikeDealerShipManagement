using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Religions.Commands;
using eBike.Application.Features.Religions.DTOs;
using eBike.Application.Features.Religions.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class ReligionsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Religions, PermissionAction.View)]
    public async Task<ActionResult<List<ReligionListDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetReligionsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Religions, PermissionAction.View)]
    public async Task<ActionResult<ReligionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetReligionByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.Religions, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateReligionCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { religionId = id, message = "Religion has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Religions, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateReligionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ReligionID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Religion has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Religions, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteReligionCommand(id), cancellationToken);
        return Ok(new { message = "Religion has been deleted successfully." });
    }
}
