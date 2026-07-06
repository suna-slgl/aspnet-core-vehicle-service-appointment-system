using VehicleServiceApp.Services.Interfaces;

namespace VehicleServiceApp.Services
{
    /// <summary>
    /// File Service Implementation - Transient Lifetime
    /// Handles file uploads for profile images and vehicle photos
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
        private readonly string[] _allowedFolders = { "profiles", "vehicles", "technicians" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Dosya boş olamaz.");

            if (!_allowedFolders.Contains(folder))
                throw new ArgumentException("GeÃ§ersiz yÃ¼kleme klasÃ¶rÃ¼.");

            if (file.Length > MaxFileSize)
                throw new ArgumentException($"Dosya boyutu {MaxFileSize / 1024 / 1024} MB'dan büyük olamaz.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new ArgumentException($"İzin verilen dosya formatları: {string.Join(", ", _allowedExtensions)}");

            if (!_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                throw new ArgumentException("Dosya iÃ§eriÄŸi desteklenen bir resim formatÄ± deÄŸil.");

            if (!await IsAllowedImageContentAsync(file, extension))
                throw new ArgumentException("Dosya iÃ§eriÄŸi uzantÄ±sÄ± ile uyumlu deÄŸil.");

            // Create unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Create directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for storing in database
            return $"/uploads/{folder}/{uniqueFileName}";
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            // Convert relative path to physical path
            var uploadsRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "uploads"));
            var physicalPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, filePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())));

            if (!physicalPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
                return false;

            if (File.Exists(physicalPath))
            {
                try
                {
                    await Task.Run(() => File.Delete(physicalPath));
                    return true;
                }
                catch
                {
                    _logger.LogWarning("Could not delete uploaded file: {FilePath}", filePath);
                    return false;
                }
            }

            return false;
        }

        public Task<string?> GetFilePathAsync(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return Task.FromResult<string?>(null);

            if (!_allowedFolders.Contains(folder))
                return Task.FromResult<string?>(null);

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", folder, fileName);
            
            if (File.Exists(filePath))
                return Task.FromResult<string?>($"/uploads/{folder}/{fileName}");

            return Task.FromResult<string?>(null);
        }

        private static async Task<bool> IsAllowedImageContentAsync(IFormFile file, string extension)
        {
            var header = new byte[12];
            await using var stream = file.OpenReadStream();
            var bytesRead = await stream.ReadAsync(header);

            return extension switch
            {
                ".jpg" or ".jpeg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => bytesRead >= 8 &&
                          header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                          header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
                ".gif" => bytesRead >= 6 &&
                          header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 &&
                          header[3] == 0x38 && (header[4] == 0x37 || header[4] == 0x39) && header[5] == 0x61,
                ".webp" => bytesRead >= 12 &&
                           header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                           header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
                _ => false
            };
        }
    }
}
