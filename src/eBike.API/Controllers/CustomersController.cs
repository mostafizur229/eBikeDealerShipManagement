using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Customers.Commands;
using eBike.Application.Features.Customers.DTOs;
using eBike.Application.Features.Customers.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class CustomersController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Customers, PermissionAction.View)]
    public async Task<ActionResult<List<CustomerListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? customerType,
        [FromQuery] int? districtId,
        [FromQuery] int? thanaId,
        [FromQuery] int? employeeId,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCustomersQuery(search, customerType, districtId, thanaId, employeeId, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Customers, PermissionAction.View)]
    public async Task<ActionResult<CustomerDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Customers, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextCustomerCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Customers, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { customerId = id, message = "Customer has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Customers, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        if (id != command.CustomerID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Customer has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Customers, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteCustomerCommand(id), cancellationToken);
        return Ok(new { message = "Customer has been deleted successfully." });
    }
}
