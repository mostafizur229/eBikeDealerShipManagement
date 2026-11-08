using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.EBikes.Commands;
using eBike.Application.Features.EBikes.DTOs;
using eBike.Application.Features.EBikes.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class EBikesController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.EBikes, PermissionAction.View)]
    public async Task<ActionResult<List<EBikeDto>>> GetAll(
        [FromQuery] EBikeStatus? status,
        [FromQuery] int? warehouseId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetEBikesQuery(status, warehouseId, search), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.EBikes, PermissionAction.Create)]
    public async Task<ActionResult<int>> Create([FromBody] CreateEBikeCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { eBikeId = id, message = "e-Bike registered into inventory successfully." });
    }
}
