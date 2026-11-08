using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.SalesReturns.Commands;
using eBike.Application.Features.SalesReturns.DTOs;
using eBike.Application.Features.SalesReturns.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
[Route("api/sales-returns")]
[Route("api/[controller]")]
public class SalesReturnsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<List<SalesReturnListDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? customerId,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSalesReturnsQuery(
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
    public async Task<ActionResult<SalesReturnDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSalesReturnByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Sales Return with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("next-invoice-no")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult> GetNextInvoiceNo(
        [FromQuery] int? concernId,
        CancellationToken cancellationToken = default)
    {
        var invoiceNo = await Sender.Send(new GetNextSalesReturnInvoiceNoQuery(concernId), cancellationToken);
        return Ok(new { invoiceNo });
    }

    [HttpGet("customer-sold-products")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.View)]
    public async Task<ActionResult<List<CustomerSoldProductDto>>> GetCustomerSoldProducts(
        [FromQuery] int customerId,
        [FromQuery] string? imei,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetCustomerSoldProductsQuery(customerId, imei, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.Create)]
    public async Task<ActionResult<SalesReturnDto>> Create(
        [FromBody] CreateSalesReturnCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.SOrderID }, result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.SalesOrders, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteSalesReturnCommand(id), cancellationToken);
        return Ok(new { message = "Sales Return reverted and inventory restored successfully." });
    }
}
