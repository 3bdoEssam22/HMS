using Hms.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Services.Helpers
{
    public class AttachmentService(ILogger<AttachmentService> _logger) : IAttachmentService
    {
        private readonly string[] _allowedExtensions = { ".jpg", ".png", ".jpeg", ".svg" };
        private readonly long _maxSize = 5 * 1024 * 1024;

        public async Task<string?> UploadFileAsync(IFormFile file, string folderName)
        {
            try
            {
                if (file is null || file.Length == 0 || file.Length > _maxSize)
                    return null;

                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!_allowedExtensions.Contains(extension))
                    return null;

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folderName);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + file.Name + extension;

                var filePath = Path.Combine(folderPath, fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);

                await file.CopyToAsync(fileStream);

                return fileName;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while uploading the file.");
                return null;
            }
        }
        public bool DeleteFile(string fileName, string folderName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(folderName))
                    return false;

                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folderName, fileName);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while deleting the file");
                return false;
            }
        }
    }
}
