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

            string Summary = "Pray to End Human Trafficking";
            string Location = "Anywhere";
            string Description = "Pray to end human trafficking.";
            string Organization = "Up Prayer Movement";

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
                DateTime currentDate = parseDateTimeObject(reservation);

                sb.AppendLine("DTSTART:" + currentDate.ToString("yyyyMMddTHHmm00"));
                sb.AppendLine("DTEND:" + currentDate.AddMinutes(30).ToString("yyyyMMddTHHmm00"));
                sb.AppendLine(Guid.NewGuid().ToString());

                sb.AppendLine("SUBJECT:TEST");
                sb.AppendLine("SUMMARY:" + Summary + "");
                sb.AppendLine("LOCATION:" + Location + "");
                sb.AppendLine("DESCRIPTION:" + Description + "");
                sb.AppendLine("ORGANIZER;CN=" + Organization + ":MAILTO:jsmith@host1.com");
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

        /*
         * Takes the reservation info and converts it to a DataTime object.
        */
        private DateTime parseDateTimeObject(Models.Reservation reservation)
        {
            int hour;
            int minute;
            DateTime currentReservationDate;

            // Convert the slot index to military time.
            hour = Convert.ToInt32(Math.Floor((double) reservation.SlotIndex / 2));
            minute = 30 * (reservation.SlotIndex % 2);

            // Creates a new date object. Note the DayIndex and MonthIndex are both 0 base so you have to add
            // one to get the correct date and month.
           currentReservationDate = new DateTime(reservation.Year, reservation.MonthIndex + 1, 
               reservation.DayIndex + 1, hour, minute, 0);

            return currentReservationDate;
        }
    }
}
