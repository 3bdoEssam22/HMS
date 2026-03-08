using HMS.Shared.Messages;

namespace Hms.Services.Abstraction
{
    public interface IEmailService
    {
        Task SendEmail(Email email);
    }
}
