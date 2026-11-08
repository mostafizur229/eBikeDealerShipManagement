using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Thanas.Commands;
using eBike.Application.Features.Thanas.DTOs;
using eBike.Application.Features.Thanas.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class ThanasController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Thanas, PermissionAction.View)]
    public async Task<ActionResult<List<ThanaListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? districtId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetThanasQuery(search, districtId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Thanas, PermissionAction.View)]
    public async Task<ActionResult<ThanaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetThanaByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Thanas, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode(CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextThanaCodeQuery(), cancellationToken);
        return Ok(new { code });
    }

    [HttpGet("by-district/{districtId:int}")]
    [HasPermission(ModuleNames.Thanas, PermissionAction.View)]
    public async Task<ActionResult<List<ThanaListDto>>> GetByDistrict(int districtId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetThanasByDistrictIdQuery(districtId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.Thanas, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateThanaCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { thanaId = id, message = "Thana has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Thanas, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateThanaCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ThanaID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Thana has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Thanas, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteThanaCommand(id), cancellationToken);
        return Ok(new { message = "Thana has been deleted successfully." });
    }
}
