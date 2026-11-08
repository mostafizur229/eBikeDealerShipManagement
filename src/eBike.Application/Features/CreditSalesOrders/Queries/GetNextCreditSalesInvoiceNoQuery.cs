using eBike.Application.Common.Interfaces;
using MediatR;

namespace eBike.Application.Features.CreditSalesOrders.Queries;

public record GetNextCreditSalesInvoiceNoQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextCreditSalesInvoiceNoQueryHandler : IRequestHandler<GetNextCreditSalesInvoiceNoQuery, string>
{
    private readonly ICreditSalesOrderExecutionService _creditSalesOrderService;
    private readonly ICurrentUser _currentUser;

    public GetNextCreditSalesInvoiceNoQueryHandler(
        ICreditSalesOrderExecutionService creditSalesOrderService,
        ICurrentUser currentUser)
    {
        _creditSalesOrderService = creditSalesOrderService;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextCreditSalesInvoiceNoQuery request, CancellationToken cancellationToken)
    {
        var targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        return await _creditSalesOrderService.GetNextInvoiceNoAsync(targetConcernId, cancellationToken);
    }
}
