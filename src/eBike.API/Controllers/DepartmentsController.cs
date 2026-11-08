using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Departments.Commands;
using eBike.Application.Features.Departments.DTOs;
using eBike.Application.Features.Departments.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class DepartmentsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Departments, PermissionAction.View)]
    public async Task<ActionResult<List<DepartmentListDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDepartmentsQuery(search), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Departments, PermissionAction.View)]
    public async Task<ActionResult<DepartmentDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDepartmentByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Departments, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode(CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextDepartmentCodeQuery(), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Departments, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { departmentId = id, message = "Department has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Departments, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.DepartmentId)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Department has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Departments, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteDepartmentCommand(id), cancellationToken);
        return Ok(new { message = "Department has been deleted successfully." });
    }
}
