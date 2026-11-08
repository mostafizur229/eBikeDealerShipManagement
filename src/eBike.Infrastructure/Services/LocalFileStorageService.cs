using eBike.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace eBike.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName, CancellationToken cancellationToken = default)
    {
        // Sanitize folder name to prevent path traversal
        var safeFolder = Path.GetFileName(folderName);
        if (string.IsNullOrWhiteSpace(safeFolder))
        {
            safeFolder = "general";
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var uploadsDir = Path.Combine(webRoot, "uploads", safeFolder);
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var safeExtension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid():N}{safeExtension}";
        var filePath = Path.Combine(uploadsDir, uniqueFileName);

        await using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(file, cancellationToken);

        return $"/uploads/{safeFolder}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.CompletedTask;
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var relativePath = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(webRoot, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
