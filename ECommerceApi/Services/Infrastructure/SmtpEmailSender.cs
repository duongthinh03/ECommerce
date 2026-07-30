using ECommerceApi.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ECommerceApi.Services;

// Gửi email THẬT qua SMTP (Gmail). Kích hoạt khi Smtp:Password có giá trị.
public class SmtpEmailSender(IOptions<SmtpSettings> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly SmtpSettings _s = options.Value;

    public async Task SendAsync(string to, string subject, string body)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_s.FromName, _s.FromEmail));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(_s.Host, _s.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_s.User, _s.Password);
        await client.SendAsync(msg);
        await client.DisconnectAsync(true);

        logger.LogInformation("📧 Đã gửi email tới {To}", to);
    }
}
