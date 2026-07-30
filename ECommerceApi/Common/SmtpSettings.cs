namespace ECommerceApi.Common;

public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string FromName { get; set; } = "ShopViet";
    public string FromEmail { get; set; } = "";
    public string User { get; set; } = "";       // email Gmail
    public string Password { get; set; } = "";    // App Password 16 ký tự (user-secrets)
}
