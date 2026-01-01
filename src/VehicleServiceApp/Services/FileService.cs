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
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Dosya boş olamaz.");

            if (file.Length > MaxFileSize)
                throw new ArgumentException($"Dosya boyutu {MaxFileSize / 1024 / 1024} MB'dan büyük olamaz.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new ArgumentException($"İzin verilen dosya formatları: {string.Join(", ", _allowedExtensions)}");

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
            var physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (File.Exists(physicalPath))
            {
                try
                {
                    await Task.Run(() => File.Delete(physicalPath));
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public Task<string?> GetFilePathAsync(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return Task.FromResult<string?>(null);

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", folder, fileName);
            
            if (File.Exists(filePath))
                return Task.FromResult<string?>($"/uploads/{folder}/{fileName}");

            return Task.FromResult<string?>(null);
        }
    }
}
