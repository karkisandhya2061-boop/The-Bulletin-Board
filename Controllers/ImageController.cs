using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    // Handles image uploads and deletions for article content
    [ApiController]
    [Route("images")]
    public class ImageController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        // Only these file types are accepted
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        // 5MB max upload size
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public ImageController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        // Upload an image and return its accessible URL
        [Authorize(Roles = "admin")]
        [HttpPost("upload")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            // Reject if no file was sent
            if (image == null || image.Length == 0)
                return BadRequest(new { message = "No image file provided." });

            // Reject if file is too large
            if (image.Length > MaxFileSizeBytes)
                return BadRequest(new { message = "File size exceeds the 5MB limit." });

            // Reject unsupported file types
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return BadRequest(new
                {
                    message = $"Invalid file type. Allowed types: {string.Join(", ", AllowedExtensions)}"
                });

            // Set the upload folder path inside wwwroot/images
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "images");

            // Create the folder if it doesn't exist yet
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Give the file a unique name to avoid conflicts
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            // Return the full URL so the frontend can store it
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var imageUrl = $"{baseUrl}/images/{uniqueFileName}";

            return Ok(new
            {
                message = "Image uploaded successfully.",
                imageUrl,
                fileName = uniqueFileName
            });
        }

        // Delete an uploaded image by filename
        [Authorize(Roles = "admin")]
        [HttpDelete("{fileName}")]
        public IActionResult DeleteImage(string fileName)
        {
            // Sanitise the filename to block path traversal attacks
            var sanitised = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(sanitised))
                return BadRequest(new { message = "Invalid filename." });

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "images");
            var filePath = Path.Combine(uploadsFolder, sanitised);

            // Return 404 if the file doesn't exist
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "Image not found." });

            System.IO.File.Delete(filePath);
            return Ok(new { message = "Image deleted successfully." });
        }
    }
}
