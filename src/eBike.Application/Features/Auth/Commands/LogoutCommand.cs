using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eBike.Application.Features.Auth.Commands;

public record LogoutCommand(
    string? RawRefreshToken,
    string? IpAddress = null
) : IRequest<bool>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<LogoutCommandHandler> logger)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RawRefreshToken))
        {
            return true;
        }

        var tokenHash = _jwtTokenGenerator.HashToken(request.RawRefreshToken);

        var storedToken = await _context.UserRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (storedToken == null)
        {
            return true;
        }

        // Revoke entire active session family
        var familyTokens = await _context.UserRefreshTokens
            .Where(t => t.TokenFamilyId == storedToken.TokenFamilyId && !t.RevokedAt.HasValue)
            .ToListAsync(cancellationToken);

        foreach (var t in familyTokens)
        {
            t.RevokedAt = DateTime.UtcNow;
            t.RevokedByIp = request.IpAddress;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully revoked session family {FamilyId} for user {UserId}", storedToken.TokenFamilyId, storedToken.UserId);

        return true;
    }
}
