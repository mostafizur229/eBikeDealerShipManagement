using eBike.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class UploadsController : BaseApiController
{
    private readonly IFileStorageService _fileStorageService;
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp"
    };

    private const long MaxImageFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

    public UploadsController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> UploadImage(
        IFormFile? file,
        [FromQuery] string folder = "products",
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No image file was provided." });
        }

        if (file.Length > MaxImageFileSizeInBytes)
        {
            return BadRequest(new { message = "File size exceeds the maximum allowed limit of 5 MB." });
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedImageExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Invalid image file type. Allowed formats: JPG, PNG, WEBP, GIF, BMP." });
        }

        // Whitelist safe folder names
        var safeFolder = folder.ToLowerInvariant().Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(safeFolder, "^[a-z0-9_-]+$"))
        {
            safeFolder = "general";
        }

        await using var stream = file.OpenReadStream();
        var relativeUrl = await _fileStorageService.SaveFileAsync(stream, file.FileName, safeFolder, cancellationToken);

        return Ok(new
        {
            url = relativeUrl,
            fileName = Path.GetFileName(relativeUrl),
            size = file.Length,
            message = "Image uploaded successfully."
        });
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteFile([FromQuery] string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return BadRequest(new { message = "File path is required." });
        }

        // Only allow deleting files inside /uploads/
        if (!filePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Invalid file path." });
        }

        await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);
        return Ok(new { message = "File deleted successfully." });
    }
}
