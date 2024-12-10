using ProniaFrontToBack.Helpers.Email;

namespace ProniaFrontToBack.Services.Abstractions;

public interface IMailService
{
    Task SendEmailAsync(MailRequest mailRequest);
}