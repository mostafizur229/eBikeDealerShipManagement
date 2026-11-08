using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.ReplacementOrders.Commands;
using eBike.Application.Features.ReplacementOrders.DTOs;
using eBike.Application.Features.ReplacementOrders.Queries;
using Microsoft.AspNetCore.Authorization;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
[Route("api/replacement-orders")]
[Route("api/[controller]")]
public class ReplacementOrdersController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<List<ReplacementOrderListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? customerId,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetReplacementOrdersQuery(
            FromDate: fromDate,
            ToDate: toDate,
            CustomerId: customerId,
            Search: search,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<ReplacementOrderDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetReplacementOrderByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Replacement order with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("next-invoice-no")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<string>> GetNextInvoiceNo(
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetNextReplacementInvoiceNoQuery(concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("customer-eligible-products")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<List<CustomerEligibleProductDto>>> GetCustomerEligibleProducts(
        [FromQuery] int customerId,
        [FromQuery] string? imei,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCustomerEligibleProductsQuery(
            CustomerId: customerId,
            Imei: imei,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("available-stock-products")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<List<AvailableStockProductDto>>> GetAvailableStockProducts(
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAvailableStockProductsQuery(
            Search: search,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.Create)]
    public async Task<ActionResult<ReplacementOrderDto>> Create(
        [FromBody] CreateReplacementOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.SOrderId }, result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.Delete)]
    public async Task<ActionResult<bool>> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteReplacementOrderCommand(id), cancellationToken);
        return Ok(result);
    }
}
