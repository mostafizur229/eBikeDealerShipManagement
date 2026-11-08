using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Auth.DTOs;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Auth.Commands;

public record LoginCommand(
    string Username, 
    string Password,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<AuthResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username or email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICryptographyService _cryptoService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IApplicationDbContext context,
        ICryptographyService cryptoService,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _cryptoService = cryptoService;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedUser = request.Username.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.SisterConcern)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => 
                u.UserName.ToLower() == normalizedUser || 
                (u.Email != null && u.Email.ToLower() == normalizedUser), 
                cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        // Check Account Lockout
        if (user.LockoutEnabled && user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc.Value > DateTime.UtcNow)
        {
            throw new ForbiddenException($"Account is temporarily locked until {user.LockoutEndDateUtc.Value:yyyy-MM-dd HH:mm} UTC.");
        }

        bool isPasswordValid = _cryptoService.VerifyPassword(
            request.Password, 
            user.PasswordHash ?? string.Empty, 
            user.SecurityStamp ?? string.Empty);

        if (!isPasswordValid)
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5 && user.LockoutEnabled)
            {
                user.LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(15);
            }
            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        // Reset failed login count
        user.AccessFailedCount = 0;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).Distinct().ToList();

        // Fetch user permitted menu hierarchy from DB
        var menuItems = await _context.ApplicationRoleMenus
            .Where(arm => roleIds.Contains(arm.RoleId))
            .Select(arm => arm.Menu)
            .Distinct()
            .OrderBy(m => m.Sequence)
            .ToListAsync(cancellationToken);

        var menuDtos = BuildMenuTree(menuItems);

        // Issue short-lived access token
        var (accessToken, accessExpiry) = _jwtTokenGenerator.GenerateAccessToken(user, roles);

        // Generate cryptographically secure refresh token & persist SHA-256 hash
        var (rawRefreshToken, tokenHash) = _jwtTokenGenerator.GenerateRefreshToken();
        var tokenFamilyId = Guid.NewGuid();
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        var refreshTokenEntity = new UserRefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            TokenFamilyId = tokenFamilyId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshExpiry,
            CreatedByIp = request.IpAddress,
            UserAgent = request.UserAgent
        };

        _context.UserRefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.UserName,
            Email = user.Email,
            ConcernId = user.ConcernID,
            ConcernName = user.SisterConcern?.Name,
            EmployeeId = user.EmployeeID,
            EmployeeName = user.Employee?.Name,
            Roles = roles,
            Menus = menuDtos,
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresAt = accessExpiry
        };
    }

    private static List<UserMenuDto> BuildMenuTree(List<ApplicationMenuItem> items)
    {
        var lookup = items.ToDictionary(m => m.Id, m => new UserMenuDto
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            ParentId = m.ParentId,
            Url = m.Url,
            Sequence = m.Sequence,
            Icon = m.Icon
        });

        var rootMenus = new List<UserMenuDto>();

        foreach (var item in items)
        {
            var dto = lookup[item.Id];
            if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parentDto))
            {
                parentDto.Children.Add(dto);
            }
            else
            {
                rootMenus.Add(dto);
            }
        }

        return rootMenus.OrderBy(m => m.Sequence).ToList();
    }
}
