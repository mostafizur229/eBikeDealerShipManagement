using eBike.Application.Common.Interfaces;
using MediatR;

namespace eBike.Application.Features.SalesOrders.Queries;

public record GetNextSalesInvoiceNoQuery(int? ConcernId = null, int SOType = 1) : IRequest<string>;

public class GetNextSalesInvoiceNoQueryHandler : IRequestHandler<GetNextSalesInvoiceNoQuery, string>
{
    private readonly ISalesOrderExecutionService _salesOrderService;
    private readonly ICurrentUser _currentUser;

    public GetNextSalesInvoiceNoQueryHandler(
        ISalesOrderExecutionService salesOrderService,
        ICurrentUser currentUser)
    {
        _salesOrderService = salesOrderService;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextSalesInvoiceNoQuery request, CancellationToken cancellationToken)
    {
        var targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        return await _salesOrderService.GetNextInvoiceNoAsync(targetConcernId, request.SOType, cancellationToken);
    }
}
