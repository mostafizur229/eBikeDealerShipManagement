using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Employees.Commands;
using eBike.Application.Features.Employees.DTOs;
using eBike.Application.Features.Employees.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class EmployeesController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Employees, PermissionAction.View)]
    public async Task<ActionResult<List<EmployeeListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? designationId,
        [FromQuery] int? departmentId,
        [FromQuery] int? status,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetEmployeesQuery(search, designationId, departmentId, status, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Employees, PermissionAction.View)]
    public async Task<ActionResult<EmployeeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetEmployeeByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Employees, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextEmployeeCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Employees, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { employeeId = id, message = "Employee has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Employees, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (id != command.EmployeeID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Employee has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Employees, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteEmployeeCommand(id), cancellationToken);
        return Ok(new { message = "Employee has been deleted successfully." });
    }
}
