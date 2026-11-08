using eBike.Application.Features.Auth.Commands;
using eBike.Application.Features.Auth.DTOs;
using eBike.Application.Features.Auth.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

public class AuthController : BaseApiController
{
    private const string RefreshTokenCookieName = "ebike_refresh_token";

    /// <summary>
    /// User Login validating against ebikeimsweb database Users table.
    /// Sets secure HttpOnly refresh-token cookie and returns short-lived JWT.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var commandWithMeta = command with
        {
            IpAddress = GetClientIpAddress(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await Sender.Send(commandWithMeta, cancellationToken);

        if (!string.IsNullOrEmpty(result.RefreshToken))
        {
            SetRefreshTokenCookie(result.RefreshToken);
        }

        return Ok(result);
    }

    /// <summary>
    /// Rotates the refresh token atomically and issues a fresh short-lived JWT.
    /// Replay attacks on old tokens trigger automatic session termination.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshTokenResponseDto>> Refresh(
        [FromBody] RefreshTokenRequestDto? body, 
        CancellationToken cancellationToken)
    {
        // Accept refresh token from either HttpOnly cookie (preferred) or request body (fallback)
        var rawToken = Request.Cookies[RefreshTokenCookieName] ?? body?.RefreshToken;

        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return Unauthorized(new { message = "No refresh token provided." });
        }

        var command = new RefreshTokenCommand(
            RawRefreshToken: rawToken,
            IpAddress: GetClientIpAddress(),
            UserAgent: Request.Headers.UserAgent.ToString()
        );

        var result = await Sender.Send(command, cancellationToken);

        if (!string.IsNullOrEmpty(result.RefreshToken))
        {
            SetRefreshTokenCookie(result.RefreshToken);
        }

        return Ok(result);
    }

    /// <summary>
    /// Logs out current session, revoking the active refresh-token family and clearing cookie.
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Logout([FromBody] RefreshTokenRequestDto? body, CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName] ?? body?.RefreshToken;

        if (!string.IsNullOrWhiteSpace(rawToken))
        {
            await Sender.Send(new LogoutCommand(rawToken, GetClientIpAddress()), cancellationToken);
        }

        ClearRefreshTokenCookie();
        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>
    /// Terminates all active sessions across all devices for the authenticated user.
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<ActionResult<object>> LogoutAll(CancellationToken cancellationToken)
    {
        await Sender.Send(new LogoutAllCommand(CurrentUserId, GetClientIpAddress()), cancellationToken);
        ClearRefreshTokenCookie();
        return Ok(new { message = "All user sessions terminated successfully." });
    }

    /// <summary>
    /// Returns current authenticated user's profile and roles.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> GetCurrentUser()
    {
        return Ok(new
        {
            userId = CurrentUserId,
            username = CurrentUser.Username,
            email = CurrentUser.Email,
            concernId = CurrentUser.ConcernId,
            employeeId = CurrentUser.EmployeeId,
            roles = CurrentUser.Roles
        });
    }

    /// <summary>
    /// Returns the user's permitted menu tree based on their database role assignments.
    /// </summary>
    [HttpGet("menus")]
    [Authorize]
    public async Task<ActionResult<List<UserMenuDto>>> GetUserMenus(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetUserMenusQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Allows the authenticated user to change their password.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<object>> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var commandWithUser = command with { UserId = CurrentUserId };
        await Sender.Send(commandWithUser, cancellationToken);
        return Ok(new { message = "Password updated successfully." });
    }

    #region Helper Methods

    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps, // Strict SSL in production
            SameSite = SameSiteMode.None, // Cross-origin cookie exchange between frontend (http://localhost:4200) and backend (https://localhost:7001)
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/api/auth"
        };

        Response.Cookies.Append(RefreshTokenCookieName, token, cookieOptions);
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.None,
            Path = "/api/auth"
        });
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    #endregion
}

public class RefreshTokenRequestDto
{
    public string? RefreshToken { get; set; }
}
