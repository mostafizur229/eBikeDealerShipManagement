using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Districts.Commands;
using eBike.Application.Features.Districts.DTOs;
using eBike.Application.Features.Districts.Queries;
using eBike.Application.Features.Thanas.DTOs;
using eBike.Application.Features.Thanas.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class DistrictsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Districts, PermissionAction.View)]
    public async Task<ActionResult<List<DistrictListDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDistrictsQuery(search), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Districts, PermissionAction.View)]
    public async Task<ActionResult<DistrictDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDistrictByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Districts, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode(CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextDistrictCodeQuery(), cancellationToken);
        return Ok(new { code });
    }

    [HttpGet("{id:int}/thanas")]
    [HasPermission(ModuleNames.Districts, PermissionAction.View)]
    public async Task<ActionResult<List<ThanaListDto>>> GetThanasByDistrict(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetThanasByDistrictIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.Districts, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateDistrictCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { districtId = id, message = "District has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Districts, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateDistrictCommand command, CancellationToken cancellationToken)
    {
        if (id != command.DistrictID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "District has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Districts, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteDistrictCommand(id), cancellationToken);
        return Ok(new { message = "District has been deleted successfully." });
    }
}
