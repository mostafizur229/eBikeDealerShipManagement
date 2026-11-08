using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Suppliers.Commands;
using eBike.Application.Features.Suppliers.DTOs;
using eBike.Application.Features.Suppliers.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class SuppliersController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Suppliers, PermissionAction.View)]
    public async Task<ActionResult<List<SupplierListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSuppliersQuery(search, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Suppliers, PermissionAction.View)]
    public async Task<ActionResult<SupplierDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSupplierByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Suppliers, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextSupplierCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Suppliers, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateSupplierCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { supplierId = id, message = "Supplier has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Suppliers, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateSupplierCommand command, CancellationToken cancellationToken)
    {
        if (id != command.SupplierID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Supplier has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Suppliers, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteSupplierCommand(id), cancellationToken);
        return Ok(new { message = "Supplier has been deleted successfully." });
    }
}
