using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace eBike.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var secretKey = _configuration["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
        {
            throw new InvalidOperationException("Critical Security Failure: Jwt:SecretKey must be configured in environment variables or configuration with at least 256 bits (32 bytes).");
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "eBikeBackend";
        var audience = _configuration["Jwt:Audience"] ?? "eBikeClientPortals";
        
        // Short-lived access token: 15 minutes default (configurable via Jwt:ExpiryMinutes)
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var exp) ? exp : 15;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("userId", user.Id.ToString()),
            new("concernId", user.ConcernID.ToString()),
            new("employeeId", user.EmployeeID.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddSeconds(-5),
            expires: expiresAt,
            signingCredentials: credentials);

        var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
        return (encodedToken, expiresAt);
    }

    public (string RawToken, string TokenHash) GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var rawToken = Convert.ToBase64String(randomBytes);
        var tokenHash = HashToken(rawToken);
        return (rawToken, tokenHash);
    }

    public string HashToken(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            throw new ArgumentException("Token to hash cannot be null or empty.", nameof(rawToken));
        }

        byte[] inputBytes = Encoding.UTF8.GetBytes(rawToken);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
