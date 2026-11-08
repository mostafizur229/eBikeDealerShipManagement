using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.CreditSalesOrders.Commands;
using eBike.Application.Features.CreditSalesOrders.DTOs;
using eBike.Application.Features.CreditSalesOrders.Queries;
using eBike.Application.Features.SalesOrders.DTOs;
using eBike.Application.Features.SalesOrders.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
[Route("api/creditsalesorders")]
[Route("api/credit-sales-orders")]
[Route("api/credit-sales")]
public class CreditSalesOrdersController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.CreditSalesOrders, PermissionAction.View)]
    public async Task<ActionResult<List<CreditSalesOrderListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? customerId,
        [FromQuery] int? status,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCreditSalesOrdersQuery(
            FromDate: fromDate,
            ToDate: toDate,
            CustomerId: customerId,
            Status: status,
            Search: search,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.CreditSalesOrders, PermissionAction.View)]
    public async Task<ActionResult<CreditSalesOrderDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCreditSalesOrderByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Credit Sales Order with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("{id:int}/invoice")]
    [HasPermission(ModuleNames.CreditSalesOrders, PermissionAction.View)]
    public async Task<ActionResult<CreditSalesOrderInvoiceDto>> GetInvoice(
        int id, 
        [FromQuery] bool isInvoice2 = false, 
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetCreditSalesOrderInvoiceQuery(id, isInvoice2), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/invoice2")]
    [HasPermission(ModuleNames.CreditSalesOrders, PermissionAction.View)]
    public async Task<ActionResult<CreditSalesOrderInvoiceDto>> GetInvoice2(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCreditSalesOrderInvoiceQuery(id, true), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-invoice-no")]
    [HasPermission(ModuleNames.CreditSalesOrders, PermissionAction.View)]
    public async Task<ActionResult> GetNextInvoiceNo([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var invoiceNo = await Sender.Send(new GetNextCreditSalesInvoiceNoQuery(concernId), cancellationToken);
        return Ok(new { invoiceNo });
    }

    [HttpGet("in-stock-vehicles")]
    [HasPermission(ModuleNames.CreditSalesOrders, PermissionAction.View)]
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
            ConcernId: concernId,
            ForCreditSales: true
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPost("calculate-schedules")]
    [HasPermission(ModuleNames.CreditSalesOrders, PermissionAction.View)]
    public async Task<ActionResult<CalculateInstallmentScheduleResultDto>> CalculateSchedules(
        [FromBody] CalculateInstallmentScheduleQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.CreditSalesOrders, PermissionAction.Create)]
    public async Task<ActionResult<CreditSalesOrderDto>> Create(
        [FromBody] CreateCreditSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.CreditSalesID }, result);
    }

    [HttpPost("pay-installment")]
    [HasPermission(ModuleNames.CreditSalesOrders, PermissionAction.Update)]
    public async Task<ActionResult<CreditSalesOrderDto>> PayInstallment(
        [FromBody] PayInstallmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.CreditSalesOrders, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteCreditSalesOrderCommand(id), cancellationToken);
        return Ok(new { message = "Credit Sales Order deleted, installments removed, and stock reverted successfully." });
    }
}
