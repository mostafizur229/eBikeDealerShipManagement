using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Categories.Commands;
using eBike.Application.Features.Categories.DTOs;
using eBike.Application.Features.Categories.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class CategoriesController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Categories, PermissionAction.View)]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCategoriesQuery(search, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Categories, PermissionAction.View)]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCategoryByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Categories, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextCategoryCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Categories, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { categoryId = id, message = "Category has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Categories, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Category has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Categories, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteCategoryCommand(id), cancellationToken);
        return Ok(new { message = "Category has been deleted successfully." });
    }
}
