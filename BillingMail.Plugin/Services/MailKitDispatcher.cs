using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using BillingMail.Plugin.Options;

namespace BillingMail.Plugin.Services;

public class MailKitDispatcher : IEmailDispatcher
{
    private readonly BillingMailOptions _options;
    private readonly ILogger<MailKitDispatcher> _logger;

    public MailKitDispatcher(IOptions<BillingMailOptions> options, ILogger<MailKitDispatcher> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendEmailAsync(string toAddress, string? ccAddresses, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toAddress))
        {
            throw new ArgumentException("Recipient 'toAddress' cannot be null or empty.", nameof(toAddress));
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));

        // Add Recipients (To) - Handle multiple comma/semicolon-separated emails
        foreach (var email in toAddress.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            message.To.Add(MailboxAddress.Parse(email));
        }

        // Add CC Recipients if available
        if (!string.IsNullOrWhiteSpace(ccAddresses))
        {
            foreach (var email in ccAddresses.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                message.Cc.Add(MailboxAddress.Parse(email));
            }
        }

        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        if (!string.IsNullOrWhiteSpace(_options.EmailCapturePath))
        {
            Directory.CreateDirectory(_options.EmailCapturePath);
            var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid()}.eml";
            var filePath = Path.Combine(_options.EmailCapturePath, fileName);
            message.WriteTo(filePath);
            _logger.LogInformation("Captured email to file: {FilePath} (To: {ToAddress})", filePath, toAddress);
            return;
        }

        using var client = new SmtpClient();

        try
        {
            var secureSocketOptions = _options.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            
            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, secureSocketOptions, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Successfully sent statement email to {ToAddress}", toAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToAddress} via SMTP {Host}:{Port}", toAddress, _options.SmtpHost, _options.SmtpPort);
            throw;
        }
    }
}
