using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.SalesOrders.Commands;
using eBike.Application.Features.SalesOrders.DTOs;
using eBike.Application.Features.SalesOrders.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class SalesOrdersController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<List<SalesOrderListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? customerId,
        [FromQuery] int? status,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        [FromQuery] int? soType,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSalesOrdersQuery(
            FromDate: fromDate,
            ToDate: toDate,
            CustomerId: customerId,
            Status: status,
            Search: search,
            ConcernId: concernId,
            SOType: soType ?? 1
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<SalesOrderDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSalesOrderByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Sales Order with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("{id:int}/invoice")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<SalesOrderInvoiceDto>> GetInvoice(
        int id, 
        [FromQuery] bool isInvoice2 = false, 
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetSalesOrderInvoiceQuery(id, isInvoice2), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/invoice2")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<SalesOrderInvoiceDto>> GetInvoice2(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSalesOrderInvoiceQuery(id, true), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/challan")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<SalesOrderChallanDto>> GetChallan(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSalesOrderChallanQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/money-receipt")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<SalesOrderMoneyReceiptDto>> GetMoneyReceipt(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSalesOrderMoneyReceiptQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-invoice-no")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult> GetNextInvoiceNo(
        [FromQuery] int? concernId,
        [FromQuery] int soType = 1,
        CancellationToken cancellationToken = default)
    {
        var invoiceNo = await Sender.Send(new GetNextSalesInvoiceNoQuery(concernId, soType), cancellationToken);
        return Ok(new { invoiceNo });
    }

    [HttpGet("in-stock-vehicles")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<List<InStockVehicleDto>>> GetInStockVehicles(
        [FromQuery] string? search,
        [FromQuery] int? productId,
        [FromQuery] int? godownId,
        [FromQuery] int? limit,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetInStockVehiclesQuery(
            Search: search,
            ProductId: productId,
            GodownId: godownId,
            Limit: limit,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("in-stock-parts")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<List<InStockPartDto>>> GetInStockParts(
        [FromQuery] string? search,
        [FromQuery] int? productId,
        [FromQuery] int? godownId,
        [FromQuery] int? limit,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetInStockPartsQuery(
            Search: search,
            ProductId: productId,
            GodownId: godownId,
            Limit: limit,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.Create)]
    public async Task<ActionResult<SalesOrderDto>> Create(
        [FromBody] CreateSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.SOrderID }, result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.Update)]
    public async Task<ActionResult<SalesOrderDto>> Update(
        int id,
        [FromBody] UpdateSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.SOrderID)
        {
            return BadRequest(new { message = "URL ID does not match body ID." });
        }

        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteSalesOrderCommand(id), cancellationToken);
        return Ok(new { message = "Sales Order deleted and stock reverted successfully." });
    }
}
