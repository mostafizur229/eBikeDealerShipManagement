using eBike.Application.Common.Interfaces;
using MediatR;

namespace eBike.Application.Features.PurchaseOrders.Queries;

public record GetNextChallanNoQuery(int? ConcernId = null, int POType = 1) : IRequest<string>;

public class GetNextChallanNoQueryHandler : IRequestHandler<GetNextChallanNoQuery, string>
{
    private readonly IPurchaseOrderExecutionService _poService;
    private readonly ICurrentUser _currentUser;

    public GetNextChallanNoQueryHandler(
        IPurchaseOrderExecutionService poService,
        ICurrentUser currentUser)
    {
        _poService = poService;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextChallanNoQuery request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        return await _poService.GetNextChallanNoAsync(targetConcernId, request.POType, cancellationToken);
    }
}
