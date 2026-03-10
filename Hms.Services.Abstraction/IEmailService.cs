using HMS.Shared.Messages;

namespace Hms.Services.Abstraction
{
    public interface IEmailService
    {
        Task SendEmailAsync(Email email);
    }
}
