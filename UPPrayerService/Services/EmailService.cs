using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace UPPrayerService.Services
{
    public class EmailService
    {
        private const string FromAddress = "test@example.com"; // TODO: Replace with actual from address
        private const string FromName = "UP Prayer Team"; // TODO: Replace with actual from name

        private DataContext Context { get; }
        private ILogger<EmailService> Logger { get; }
        private string SendGridAPIKey { get; }
        public bool IsEnabled { get; private set; }
        private string ConfirmationURL { get; }

        public EmailService(IConfiguration configuration, DataContext context, ILogger<EmailService> logger)
        {
            this.Context = context;
            this.Logger = logger;
            this.SendGridAPIKey = configuration.GetSection("APIKeys")["SendGrid"];
            this.IsEnabled = configuration.GetSection(nameof(EmailService)).GetValue<bool>("Enabled");
            this.ConfirmationURL = configuration.GetSection(nameof(EmailService))["ConfirmationURL"];
        }

        public async Task SendConfirmationEmail(string recipientEmail, string confirmationCode)
        {
            if (!IsEnabled)
                return;

            SendGridClient client = new SendGridClient(SendGridAPIKey);
            EmailAddress fromAddress = new EmailAddress(FromAddress, FromName);
            EmailAddress toAddress = new EmailAddress(recipientEmail);
            string subject = "Confirm your prayer date"; // TODO: Final copy
            string confirmationAddress = ConfirmationURL + confirmationCode;
            string plaintextContent = "Thank you for signing up to pray for trafficking around the globe!\n\nTo confirm your reservation, visit " + confirmationAddress + "\n\nBest,\nThe UP Prayer Team"; // TODO: Get plaintext content from database
            string htmlContent = plaintextContent; // TODO: Get HTML content from database

            SendGridMessage message = MailHelper.CreateSingleEmail(fromAddress, toAddress, subject, plaintextContent, htmlContent);

            await client.SendEmailAsync(message);
        }
    }
}
