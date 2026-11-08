using eBike.Application.Common.Interfaces;
using eBike.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace eBike.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning("SMTP Host is not configured. Email to {To} with Subject '{Subject}' was not sent.", to, subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.Auto, cancellationToken);
            
            if (!string.IsNullOrEmpty(_settings.UserName))
            {
                await client.AuthenticateAsync(_settings.UserName, _settings.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }
}
