using eBike.Application.Common.Interfaces;
using MediatR;

namespace eBike.Application.Features.PurchaseReturns.Queries;

public record GetNextReturnChallanNoQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextReturnChallanNoQueryHandler : IRequestHandler<GetNextReturnChallanNoQuery, string>
{
    private readonly IPurchaseOrderExecutionService _poService;
    private readonly ICurrentUser _currentUser;

    public GetNextReturnChallanNoQueryHandler(
        IPurchaseOrderExecutionService poService,
        ICurrentUser currentUser)
    {
        _poService = poService;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextReturnChallanNoQuery request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        return await _poService.GetNextChallanNoAsync(targetConcernId, 2, cancellationToken);
    }
}
