using System.Data;
using eBike.Domain.Entities;

namespace eBike.Application.Common.Interfaces;

public interface ICurrentUser
{
    int? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    int? ConcernId { get; }
    int? EmployeeId { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
}

public interface ICryptographyService
{
    (string Hash, string Salt) CreatePasswordHash(string password);
    bool VerifyPassword(string password, string storedHash, string storedSalt);
}

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    (string RawToken, string TokenHash) GenerateRefreshToken();
    string HashToken(string rawToken);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public interface IRolePermissionCacheService
{
    Task<bool> HasPermissionAsync(int userId, string moduleName, string action, CancellationToken cancellationToken = default);
    Task InvalidateUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
}

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
}

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
