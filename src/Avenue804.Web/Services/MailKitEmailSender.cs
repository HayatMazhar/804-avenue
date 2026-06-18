using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Avenue804.Web.Services;

public class MailKitEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<MailKitEmailSender> _log;

    public MailKitEmailSender(IConfiguration cfg, ILogger<MailKitEmailSender> log)
    {
        _cfg = cfg;
        _log = log;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var host = _cfg["Smtp:Host"];
        var port = int.TryParse(_cfg["Smtp:Port"], out var p) ? p : 587;
        var user = _cfg["Smtp:User"];
        var pass = _cfg["Smtp:Password"];
        var from = _cfg["Smtp:FromAddress"] ?? user ?? "noreply@804avenue.ae";
        var fromName = _cfg["Smtp:FromName"] ?? "804 Avenue";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user))
        {
            _log.LogWarning("SMTP not configured — skipping email to {To} ({Subject}).", to, subject);
            return;
        }

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(fromName, from));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(user, pass, cancellationToken);
        await client.SendAsync(msg, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _log.LogInformation("Email sent to {To}: {Subject}", to, subject);
    }
}
