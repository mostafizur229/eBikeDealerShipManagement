using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Auth.DTOs;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eBike.Application.Features.Auth.Commands;

public record RefreshTokenCommand(
    string RawRefreshToken,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<RefreshTokenResponseDto>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RawRefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<RefreshTokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var incomingHash = _jwtTokenGenerator.HashToken(request.RawRefreshToken);

        // Find token record by hash
        var storedToken = await _context.UserRefreshTokens
            .Include(t => t.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(t => t.TokenHash == incomingHash, cancellationToken);

        if (storedToken == null)
        {
            _logger.LogWarning("Security Alert: Refresh attempt with non-existent token hash from IP: {IpAddress}", request.IpAddress);
            throw new UnauthorizedAccessException("Invalid or unrecognized refresh token.");
        }

        // REPLAY DETECTION WITH CONCURRENT GRACE PERIOD:
        // If a token that has ALREADY been revoked or replaced is presented again:
        if (storedToken.RevokedAt.HasValue || !string.IsNullOrEmpty(storedToken.ReplacedByTokenHash))
        {
            // Concurrent Request Grace Period (30 seconds):
            // In high-concurrency browser environments (multiple parallel HTTP calls firing upon page load),
            // a client may legitimately resend the same refresh token within a few seconds of rotation.
            // In this case, issue a new access token without terminating the user session.
            if (storedToken.RevokedAt.HasValue && storedToken.RevokedAt.Value > DateTime.UtcNow.AddSeconds(-30))
            {
                _logger.LogInformation("Refresh token concurrent grace period matched for User: {UserId}, Family: {FamilyId}",
                    storedToken.UserId, storedToken.TokenFamilyId);

                var graceUser = storedToken.User;
                var graceRoles = graceUser.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
                var (graceAccessToken, graceExpiry) = _jwtTokenGenerator.GenerateAccessToken(graceUser, graceRoles);

                return new RefreshTokenResponseDto
                {
                    AccessToken = graceAccessToken,
                    RefreshToken = null, // Client already received the active refresh token in the initial request
                    ExpiresAt = graceExpiry
                };
            }

            _logger.LogCritical("CRITICAL SECURITY VIOLATION: Refresh token replay attack detected! User: {UserId}, Family: {FamilyId}, IP: {IpAddress}",
                storedToken.UserId, storedToken.TokenFamilyId, request.IpAddress);

            // Invalidate ALL tokens in this compromised family immediately
            var familyTokens = await _context.UserRefreshTokens
                .Where(t => t.TokenFamilyId == storedToken.TokenFamilyId && !t.RevokedAt.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var token in familyTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = request.IpAddress;
            }

            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Security violation: Session compromised. Re-authentication required.");
        }

        // Expiration check
        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = request.IpAddress;
            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Refresh token has expired. Please log in again.");
        }

        // Account status check
        var user = storedToken.User;
        if (user.LockoutEnabled && user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc.Value > DateTime.UtcNow)
        {
            throw new ForbiddenException("Account is temporarily locked.");
        }

        // ROTATION: Atomically revoke current token and issue new token pair within the same TokenFamilyId
        var (newRawRefreshToken, newTokenHash) = _jwtTokenGenerator.GenerateRefreshToken();
        
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = request.IpAddress;
        storedToken.ReplacedByTokenHash = newTokenHash;

        var newRefreshTokenEntity = new UserRefreshToken
        {
            UserId = user.Id,
            TokenHash = newTokenHash,
            TokenFamilyId = storedToken.TokenFamilyId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = request.IpAddress,
            UserAgent = request.UserAgent
        };

        _context.UserRefreshTokens.Add(newRefreshTokenEntity);

        // Generate new short-lived access token
        var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
        var (accessToken, accessExpiry) = _jwtTokenGenerator.GenerateAccessToken(user, roles);

        await _context.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRawRefreshToken,
            ExpiresAt = accessExpiry
        };
    }
}
