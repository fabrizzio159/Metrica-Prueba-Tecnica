using MailKit.Net.Smtp;
using MimeKit;
using NotificacionService.Interfaces;

namespace NotificacionService.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sistema Carga Masiva", _configuration["SMTP:From"]));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();

        try
        {
            var host = _configuration["SMTP:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(_configuration["SMTP:Port"] ?? "587");
            var user = _configuration["SMTP:User"];
            var password = _configuration["SMTP:Password"];

            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);

            if (!string.IsNullOrEmpty(user))
                await client.AuthenticateAsync(user, password);

            await client.SendAsync(message);
            _logger.LogInformation("Correo enviado a {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo a {To}", to);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
