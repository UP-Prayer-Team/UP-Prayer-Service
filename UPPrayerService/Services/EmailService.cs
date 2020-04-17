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
            string times = "";
            string confirmationAddress = ConfirmationURL + confirmationCode;
            StringBuilder builder = new StringBuilder();


            foreach (Models.Reservation reservation in reservations) {
                DateTime currentReservation = reservation.GetStartTime();
                builder.AppendLine(currentReservation.ToString(@"hh\:mm\:ss tt") + ", ");
            }

            times = builder.ToString();

            string plaintextContent = "Thank you for signing up to pray for trafficking around the globe!\n\nTo confirm your reservation for these times: ," + times + " visit "+ confirmationAddress + "\n\nBest,\nThe UP Prayer Team";
            string htmlContent = plaintextContent; // TODO: Get HTML content from database

            builder.Clear();

            SendGridMessage message = MailHelper.CreateSingleEmail(fromAddress, toAddress, subject, plaintextContent, htmlContent);



            string summary = "Pray to End Human Trafficking";
            string location = "Anywhere";
            string description = "Pray to end human trafficking.";
            string organization = "Up Prayer Movement";

            foreach (Models.Reservation reservation in reservations) {
                //start the calendar item
                builder.AppendLine("BEGIN:VCALENDAR");
                builder.AppendLine("VERSION:2.0");
                builder.AppendLine("PRODID:stackoverflow.com");
                builder.AppendLine("CALSCALE:GREGORIAN");
                builder.AppendLine("METHOD:PUBLISH");

                //add the event
                builder.AppendLine("BEGIN:VEVENT");
                
                // Create a new date object with the current reservations info.
                DateTime currentDate = reservation.GetStartTime();

                builder.AppendLine("DTSTART:" + currentDate.ToString("yyyyMMddTHHmm00"));
                builder.AppendLine("DTEND:" + currentDate.AddMinutes(30).ToString("yyyyMMddTHHmm00"));
                builder.AppendLine(Guid.NewGuid().ToString());

                builder.AppendLine("SUBJECT:Reservation");
                builder.AppendLine("SUMMARY:" + summary + "");
                builder.AppendLine("LOCATION:" + location + "");
                builder.AppendLine("DESCRIPTION:" + description + "");
                builder.AppendLine("ORGANIZER;CN=" + organization + ":MAILTO:" + FromAddress);
                builder.AppendLine("PRIORITY:3");
                builder.AppendLine("END:VEVENT");

                builder.AppendLine("END:VCALENDAR");

                byte[] atachmentBytes = Encoding.ASCII.GetBytes(builder.ToString());
                string file = Convert.ToBase64String(atachmentBytes);

                message.AddAttachment("invite.ics", file);

                builder.Clear();
            }

            await client.SendEmailAsync(message);
        }

 
    }
}
