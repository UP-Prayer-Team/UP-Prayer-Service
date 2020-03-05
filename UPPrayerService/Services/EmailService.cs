using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;


namespace UPPrayerService.Services
{
    public class EmailService
    {
        private const string FromAddress = "noReply@upmovement.org"; // TODO: Replace with actual from address
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

        public async Task SendConfirmationEmail(string recipientEmail, string confirmationCode, IEnumerable<Models.Reservation> reservations)
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




            StringBuilder sb = new StringBuilder();

            string summary = "Pray to End Human Trafficking";
            string location = "Anywhere";
            string description = "Pray to end human trafficking.";
            string organization = "Up Prayer Movement";

            foreach (Models.Reservation reservation in reservations) {
                //start the calendar item
                sb.AppendLine("BEGIN:VCALENDAR");
                sb.AppendLine("VERSION:2.0");
                sb.AppendLine("PRODID:stackoverflow.com");
                sb.AppendLine("CALSCALE:GREGORIAN");
                sb.AppendLine("METHOD:PUBLISH");

                //add the event
                sb.AppendLine("BEGIN:VEVENT");
                
                // Create a new date object with the current reservations info.
                DateTime currentDate = reservation.GetStartTime();

                sb.AppendLine("DTSTART:" + currentDate.ToString("yyyyMMddTHHmm00"));
                sb.AppendLine("DTEND:" + currentDate.AddMinutes(30).ToString("yyyyMMddTHHmm00"));
                sb.AppendLine(Guid.NewGuid().ToString());

                sb.AppendLine("SUBJECT:Reservation");
                sb.AppendLine("SUMMARY:" + summary + "");
                sb.AppendLine("LOCATION:" + location + "");
                sb.AppendLine("DESCRIPTION:" + description + "");
                sb.AppendLine("ORGANIZER;CN=" + organization + ":MAILTO:" + FromAddress);
                sb.AppendLine("PRIORITY:3");
                sb.AppendLine("END:VEVENT");

                sb.AppendLine("END:VCALENDAR");

                byte[] atachmentBytes = Encoding.ASCII.GetBytes(sb.ToString());
                string file = Convert.ToBase64String(atachmentBytes);

                message.AddAttachment("invite.ics", file);

                sb.Clear();
            }

            await client.SendEmailAsync(message);
        }

 
    }
}
