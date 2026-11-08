using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.SaleOffers.Commands;
using eBike.Application.Features.SaleOffers.DTOs;
using eBike.Application.Features.SaleOffers.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class SaleOffersController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.SaleOffers, PermissionAction.View)]
    public async Task<ActionResult<List<SaleOfferListDto>>> GetAll([FromQuery] string? search, [FromQuery] int? productId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSaleOffersQuery(search, productId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.SaleOffers, PermissionAction.View)]
    public async Task<ActionResult<SaleOfferDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSaleOfferByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.SaleOffers, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode(CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextSaleOfferCodeQuery(), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.SaleOffers, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateSaleOfferCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { offerId = id, message = "Sales Offer has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.SaleOffers, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateSaleOfferCommand command, CancellationToken cancellationToken)
    {
        if (id != command.OfferID)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Sales Offer has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.SaleOffers, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteSaleOfferCommand(id), cancellationToken);
        return Ok(new { message = "Sales Offer has been deleted successfully." });
    }
}
