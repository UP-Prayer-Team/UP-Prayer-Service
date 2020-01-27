using System;
using System.Collections.Generic;
using UPPrayerService.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace UPPrayerService.Services
{
    public class ReservationService
    {
        //private List<Reservation> _testReservations = new List<Reservation>();
        //private List<Confirmation> _testConfirmations = new List<Confirmation>();
        private EmailService EmailService { get; }
        private DataContext Context { get; }
        private ILogger Logger { get; }

        public ReservationService(EmailService emailService, DataContext dataContext, ILogger<ReservationService> logger)
        {
            this.EmailService = emailService;
            this.Context = dataContext;
            this.Logger = logger;
            //_testReservations.Add(new Reservation() { Country = "", DayIndex = 0, MonthIndex = 1, District = "", Email = "test1@example.com", Year = 2019, ID = "0409481", SlotIndex = 2, IsConfirmed = false });
            //_testReservations.Add(new Reservation() { Country = "USA", DayIndex = 0, MonthIndex = 2, District = "OR", Email = "test2@example.com", Year = 2019, ID = "5018429", SlotIndex = 12, IsConfirmed = true });
            //_testReservations.Add(new Reservation() { Country = "USA", DayIndex = 3, MonthIndex = 1, District = "WA", Email = "test3@example.com", Year = 2019, ID = "5918493", SlotIndex = 5, IsConfirmed = true });

            //_testConfirmations.Add(new Confirmation() { ID = "001", Email = "test1@example.com", Reservations = new List<Reservation>() { _testReservations[0] } });
        }

        public IEnumerable<Reservation> GetReservations(DateTime start, DateTime end, bool onlyConfirmed)
        {
            //return _testReservations.Where(reservation =>
            return Context.Reservations.Where(reservation => reservation.IsConfirmed || !onlyConfirmed).ToArray().Where(reservation =>
            {
                DateTime date = new DateTime(reservation.Year, reservation.MonthIndex + 1, reservation.DayIndex + 1, reservation.SlotIndex / 2, 30 * (reservation.SlotIndex % 2), 0);
                return (reservation.IsConfirmed || !onlyConfirmed) && start <= date && end > date;
            });
        }

        public string GenerateReservationID()
        {
            return Guid.NewGuid().ToString();
        }

        public void AddReservation(Reservation reservation)
        {
            //_testReservations.Add(reservation);
            Context.Reservations.Add(reservation);
            Context.SaveChanges();
        }

        public string GenerateConfirmationID()
        {
            return Guid.NewGuid().ToString();
        }

        public bool DoesEmailHavePendingConfirmation(string email)
        {
            return Context.Confirmations.Any(confirmation => confirmation.Email == email);
        }

        public async Task SendConfirmationCode(string email, string confirmationCode)
        {
            Logger.LogInformation("\n\nConfirmation code for '" + email + "': " + confirmationCode + "\n\n");

            await EmailService.SendConfirmationEmail(email, confirmationCode);
        }

        public void AddConfirmation(Confirmation confirmation)
        {
            //_testConfirmations.Add(confirmation);
            Context.Confirmations.Add(confirmation);
            Context.SaveChanges();
        }

        public List<Reservation> Confirm(string confirmationID)
        {
            Confirmation confirmation = Context.Confirmations.Include(conf => conf.Reservations).FirstOrDefault(conf => conf.ID == confirmationID);
            List<Reservation> confirmedReservations = null;
            if (confirmation == null)
            {
                throw new KeyNotFoundException();
            }
            else
            {
                foreach (Reservation r in confirmation.Reservations)
                {
                    r.IsConfirmed = true;
                }
                confirmedReservations = new List<Reservation>(confirmation.Reservations);
                //_testConfirmations.Remove(confirmation);
                Context.Confirmations.Remove(confirmation);
                Context.SaveChanges();
            }
            return confirmedReservations;
        }
    }
}
