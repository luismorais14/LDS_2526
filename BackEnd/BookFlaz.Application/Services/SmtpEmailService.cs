using BookFlaz.Application.Interfaces; // A sua interface
using Microsoft.Extensions.Configuration;
using MimeKit; // MailKit
using MailKit.Net.Smtp; // MailKit
using MailKit.Security; // MailKit
using System.Threading.Tasks;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;

    public SmtpEmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var settings = _config.GetSection("SmtpSettings");
        var host = settings["Host"];
        var port = int.Parse(settings["Port"]);
        var username = settings["Username"];
        var password = settings["Password"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("BookFlaz", username));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = htmlBody;
        message.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            var secureSocketOption = port == 465 
                ? SecureSocketOptions.SslOnConnect 
                : SecureSocketOptions.StartTls;
            
            await client.ConnectAsync(host, port, secureSocketOption);

            await client.AuthenticateAsync(username, password);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }
    }
}