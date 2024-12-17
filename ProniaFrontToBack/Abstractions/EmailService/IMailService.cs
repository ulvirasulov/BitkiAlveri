using ProniaFrontToBack.Helpers.Email;

namespace ProniaFrontToBack.Abstractions.EmailService
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
