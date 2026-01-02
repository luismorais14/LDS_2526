namespace BookFlaz.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string htmlBody);
}