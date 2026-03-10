
using Microsoft.AspNetCore.Http;

namespace Hms.Services.Abstraction
{
public interface IAttachmentService
    {
        Task<string?> UploadFileAsync(IFormFile file, string folderName);

        bool DeleteFile(string fileName, string folderName);
    }
}
