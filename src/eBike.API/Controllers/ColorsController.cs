using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Colors.Commands;
using eBike.Application.Features.Colors.DTOs;
using eBike.Application.Features.Colors.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class ColorsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Colors, PermissionAction.View)]
    public async Task<ActionResult<List<ColorDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetColorsQuery(search, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Colors, PermissionAction.View)]
    public async Task<ActionResult<ColorDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetColorByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Colors, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextColorCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Colors, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateColorCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { colorId = id, message = "Color has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Colors, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateColorCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Color has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Colors, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteColorCommand(id), cancellationToken);
        return Ok(new { message = "Color has been deleted successfully." });
    }
}
