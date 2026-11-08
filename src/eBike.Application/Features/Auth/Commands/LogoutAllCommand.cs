using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eBike.Application.Features.Auth.Commands;

public record LogoutAllCommand(
    int UserId,
    string? IpAddress = null
) : IRequest<bool>;

public class LogoutAllCommandHandler : IRequestHandler<LogoutAllCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<LogoutAllCommandHandler> _logger;

    public LogoutAllCommandHandler(
        IApplicationDbContext context,
        ILogger<LogoutAllCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
    {
        var activeTokens = await _context.UserRefreshTokens
            .Where(t => t.UserId == request.UserId && !t.RevokedAt.HasValue)
            .ToListAsync(cancellationToken);

        if (!activeTokens.Any())
        {
            return true;
        }

        foreach (var t in activeTokens)
        {
            t.RevokedAt = DateTime.UtcNow;
            t.RevokedByIp = request.IpAddress;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Terminated all ({Count}) active sessions for user {UserId} from IP {IpAddress}", 
            activeTokens.Count, request.UserId, request.IpAddress);

        return true;
    }
}
