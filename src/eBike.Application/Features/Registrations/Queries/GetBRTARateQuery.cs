using System.Globalization;
using System.Text.RegularExpressions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Registrations.Queries;

public record GetBRTARateQuery(int RegType, decimal? CC = null, int? ProductId = null) : IRequest<BRTARateDto?>;

public class GetBRTARateQueryHandler : IRequestHandler<GetBRTARateQuery, BRTARateDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetBRTARateQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BRTARateDto?> Handle(GetBRTARateQuery request, CancellationToken cancellationToken)
    {
        decimal targetCC = request.CC ?? 0;

        if (targetCC == 0 && request.ProductId.HasValue)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == request.ProductId.Value, cancellationToken);
            if (product != null)
            {
                targetCC = ParseCC(product.CC);
            }
        }

        int? concernId = _currentUser.ConcernId;

        var query = _context.BRTARates.AsQueryable();
        if (concernId.HasValue && concernId.Value > 0)
        {
            var concernExists = await query.AnyAsync(r => r.ConcernID == concernId.Value, cancellationToken);
            if (concernExists)
            {
                query = query.Where(r => r.ConcernID == concernId.Value);
            }
        }

        query = query.Where(r => r.RegType == request.RegType);

        var rates = await query.ToListAsync(cancellationToken);
        if (rates.Count == 0)
        {
            rates = await _context.BRTARates.Where(r => r.RegType == request.RegType).ToListAsync(cancellationToken);
        }

        if (rates.Count == 0) return null;

        var matchedRate = targetCC <= 100
            ? rates.FirstOrDefault(r => r.CC <= 100)
            : rates.FirstOrDefault(r => r.CC > 100);

        matchedRate ??= rates.FirstOrDefault();

        if (matchedRate == null) return null;

        return new BRTARateDto
        {
            ID = matchedRate.ID,
            RegType = matchedRate.RegType,
            RegTypeName = matchedRate.RegType == 1 ? "2 years" : matchedRate.RegType == 2 ? "10 years" : "BRTA",
            CC = matchedRate.CC,
            NewRegMF = matchedRate.NewRegMF,
            NewRegIF = matchedRate.NewRegIF,
            IssueTaxTokeMF = matchedRate.IssueTaxTokeMF,
            RegCertificateMF = matchedRate.RegCertificateMF,
            SupplementaryMF = matchedRate.SupplementaryMF,
            NamePlate = matchedRate.NamePlate,
            Contribution = matchedRate.Contribution,
            TotalAmount = matchedRate.TotalAmount
        };
    }

    private static decimal ParseCC(string? ccStr)
    {
        if (string.IsNullOrWhiteSpace(ccStr)) return 0;
        var match = Regex.Match(ccStr, @"\d+(\.\d+)?");
        if (match.Success && decimal.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var cc))
        {
            return cc;
        }
        return 0;
    }
}

