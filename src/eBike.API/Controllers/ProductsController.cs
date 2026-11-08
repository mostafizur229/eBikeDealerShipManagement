using eBike.API.Authorization;
using eBike.Application.Common.DTOs;
using eBike.Application.Common.Security;
using eBike.Application.Features.Products.Commands;
using eBike.Application.Features.Products.DTOs;
using eBike.Application.Features.Products.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class ProductsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Products, PermissionAction.View)]
    public async Task<ActionResult<PaginatedList<ProductListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? companyId,
        [FromQuery] int? concernId,
        [FromQuery] bool? isParts,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetProductsQuery(search, categoryId, companyId, concernId, isParts, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Products, PermissionAction.View)]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetProductByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Products, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextProductCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Products, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { productId = id, message = "Product has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Products, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateProductCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Product has been updated successfully." });
    }

    [HttpPatch("{id:int}/mrp")]
    [HasPermission(ModuleNames.Products, PermissionAction.Update)]
    public async Task<ActionResult> UpdateMrp(int id, [FromBody] UpdateProductMrpRequest request, CancellationToken cancellationToken)
    {
        await Sender.Send(new UpdateProductMrpCommand(id, request.MrpRate), cancellationToken);
        return Ok(new { message = "Product MRP updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Products, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteProductCommand(id), cancellationToken);
        return Ok(new { message = "Product has been deleted successfully." });
    }
}

public record UpdateProductMrpRequest(decimal MrpRate);

