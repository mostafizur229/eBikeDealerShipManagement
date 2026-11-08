using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Transfers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Transfers.Queries;

public record GetTransferByIdQuery(int Id) : IRequest<TransferDto>;

public class GetTransferByIdQueryHandler : IRequestHandler<GetTransferByIdQuery, TransferDto>
{
    private readonly IApplicationDbContext _context;

    public GetTransferByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransferDto> Handle(GetTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers
            .AsNoTracking()
            .Include(t => t.FromConcern)
            .Include(t => t.ToConcern)
            .Include(t => t.TransferDetails)
                .ThenInclude(d => d.Product)
            .Include(t => t.TransferDetails)
                .ThenInclude(d => d.ToGodown)
            .Include(t => t.TransferDetails)
                .ThenInclude(d => d.ToColor)
            .FirstOrDefaultAsync(t => t.TransferID == request.Id, cancellationToken);

        if (transfer == null)
            throw new NotFoundException($"Transfer with ID {request.Id} was not found.");

        var sDetailIds = transfer.TransferDetails.Select(d => d.SDetailID).Distinct().ToList();
        var stockDetails = await _context.StockDetails
            .AsNoTracking()
            .Include(sd => sd.Godown)
            .Where(sd => sDetailIds.Contains(sd.SDetailID))
            .ToDictionaryAsync(sd => sd.SDetailID, cancellationToken);

        var details = transfer.TransferDetails.Select(d =>
        {
            stockDetails.TryGetValue(d.SDetailID, out var stockDetail);

            return new TransferDetailDto
            {
                TDetailId = d.TDetailID,
                TransferId = d.TransferID,
                ProductId = d.ProductID,
                ProductCode = d.Product?.Code,
                ProductName = d.Product?.ProductName,
                SDetailId = d.SDetailID,
                IMEI = d.IMEI ?? stockDetail?.IMENO,
                EngineNo = stockDetail?.EngineNo,
                FromGodownId = stockDetail?.GodownID,
                FromGodownName = stockDetail?.Godown?.Name,
                ToGodownId = d.ToGodownID,
                ToGodownName = d.ToGodown?.Name,
                ColorId = d.ToColorID,
                ColorName = d.ToColor?.Name,
                PRate = d.PRate,
                Quantity = d.Quantity,
                UTAmount = d.UTAmount
            };
        }).ToList();

        return new TransferDto
        {
            TransferId = transfer.TransferID,
            TransferNo = transfer.TransferNo,
            TransferDate = transfer.TransferDate,
            FromConcernId = transfer.FromConcernID,
            FromConcernName = transfer.FromConcern?.Name ?? $"Branch #{transfer.FromConcernID}",
            ToConcernId = transfer.ToConcernID,
            ToConcernName = transfer.ToConcern?.Name ?? $"Branch #{transfer.ToConcernID}",
            TotalAmount = transfer.TotalAmount,
            Remarks = transfer.Remarks,
            Status = transfer.Status,
            CreateDate = transfer.CreateDate,
            Details = details
        };
    }
}
