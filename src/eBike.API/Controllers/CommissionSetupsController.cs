using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.CommissionSetups.Commands;
using eBike.Application.Features.CommissionSetups.DTOs;
using eBike.Application.Features.CommissionSetups.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class CommissionSetupsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.CommissionSetups, PermissionAction.View)]
    public async Task<ActionResult<List<CommissionSetupListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? employeeId,
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCommissionSetupsQuery(search, employeeId, year, month), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.CommissionSetups, PermissionAction.View)]
    public async Task<ActionResult<CommissionSetupDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCommissionSetupByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("designation-wise")]
    [HasPermission(ModuleNames.CommissionSetups, PermissionAction.View)]
    public async Task<ActionResult<List<DesWiseCommissionDto>>> GetDesignationWise(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDesWiseCommissionsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("designation-wise")]
    [HasPermission(ModuleNames.CommissionSetups, PermissionAction.Create)]
    public async Task<ActionResult> UpsertDesignationWise([FromBody] UpsertDesWiseCommissionCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { id, message = "Designation Commission Setup saved successfully." });
    }

    [HttpPost]
    [HasPermission(ModuleNames.CommissionSetups, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateCommissionSetupCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { commissionId = id, message = "Commission Setup has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.CommissionSetups, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateCommissionSetupCommand command, CancellationToken cancellationToken)
    {
        if (id != command.CSID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Commission Setup has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.CommissionSetups, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteCommissionSetupCommand(id), cancellationToken);
        return Ok(new { message = "Commission Setup has been deleted successfully." });
    }
}
