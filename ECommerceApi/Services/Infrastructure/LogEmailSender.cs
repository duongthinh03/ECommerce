namespace ECommerceApi.Services;

// Dev sender: ghi email (kèm OTP) ra console thay vì gửi thật.
// Production: thay bằng SmtpEmailSender (Gmail) và đổi đăng ký DI.
public class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string body)
    {
        logger.LogInformation("\n===== 📧 EMAIL (DEV) =====\nTo: {To}\nSubject: {Subject}\n{Body}\n==========================", to, subject, body);
        return Task.CompletedTask;
    }
}
