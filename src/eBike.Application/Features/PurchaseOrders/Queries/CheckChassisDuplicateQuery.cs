using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseOrders.DTOs;
using MediatR;

namespace eBike.Application.Features.PurchaseOrders.Queries;

public record CheckChassisDuplicateQuery(string IMENO, int? ConcernId = null) : IRequest<ChassisDuplicateCheckDto>;

public class CheckChassisDuplicateQueryHandler : IRequestHandler<CheckChassisDuplicateQuery, ChassisDuplicateCheckDto>
{
    private readonly IPurchaseOrderExecutionService _poService;
    private readonly ICurrentUser _currentUser;

    public CheckChassisDuplicateQueryHandler(
        IPurchaseOrderExecutionService poService,
        ICurrentUser currentUser)
    {
        _poService = poService;
        _currentUser = currentUser;
    }

    public async Task<ChassisDuplicateCheckDto> Handle(CheckChassisDuplicateQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IMENO))
        {
            return new ChassisDuplicateCheckDto
            {
                IMENO = request.IMENO,
                IsDuplicate = false,
                Message = "Chassis/IMEI cannot be empty."
            };
        }

        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        int existCount = await _poService.CheckIMENoDuplicacyByConcernIdAsync(targetConcernId, request.IMENO.Trim(), cancellationToken);

        bool isDuplicate = existCount > 0;
        return new ChassisDuplicateCheckDto
        {
            IMENO = request.IMENO.Trim(),
            IsDuplicate = isDuplicate,
            Message = isDuplicate
                ? $"Chassis / IMEI '{request.IMENO.Trim()}' is already registered in the system."
                : "Chassis / IMEI is available."
        };
    }
}
