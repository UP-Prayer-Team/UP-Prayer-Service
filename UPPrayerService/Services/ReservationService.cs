using System;
using System.Collections.Generic;
using UPPrayerService.Models;
using System.Linq;

namespace UPPrayerService.Services
{
    public class ReservationService
    {
        private List<Reservation> _testReservations = new List<Reservation>();
        private List<Confirmation> _testConfirmations = new List<Confirmation>();

        public ReservationService()
        {
            _testReservations.Add(new Reservation() { Country = "", DayIndex = 0, MonthIndex = 1, District = "", Email = "test1@example.com", Year = 2019, ID = "0409481", SlotIndex = 2, IsConfirmed = false });
            _testReservations.Add(new Reservation() { Country = "USA", DayIndex = 0, MonthIndex = 2, District = "OR", Email = "test2@example.com", Year = 2019, ID = "5018429", SlotIndex = 12, IsConfirmed = true });
            _testReservations.Add(new Reservation() { Country = "USA", DayIndex = 3, MonthIndex = 1, District = "WA", Email = "test3@example.com", Year = 2019, ID = "5918493", SlotIndex = 5, IsConfirmed = true });

            _testConfirmations.Add(new Confirmation() { ConfirmationID = "001", Email = "test1@example.com", Reservations = new List<string>() { "0409481" } });
        }

        public IEnumerable<Reservation> GetReservations(DateTime start, DateTime end, bool onlyConfirmed)
        {
            return _testReservations.Where(reservation =>
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
            _testReservations.Add(reservation);
        }

        public string GenerateConfirmationID()
        {
            return Guid.NewGuid().ToString();
        }

        public void AddConfirmation(Confirmation confirmation)
        {
            _testConfirmations.Add(confirmation);
        }

        public void Confirm(string confirmationID)
        {
            Confirmation confirmation = _testConfirmations.FirstOrDefault(conf => conf.ConfirmationID == confirmationID);
            if (confirmation == null)
            {
                throw new KeyNotFoundException();
            }
            else
            {
                foreach (Reservation r in _testReservations.Where(r => confirmation.Reservations.Contains(r.ID)))
                {
                    r.IsConfirmed = true;
                }
                _testConfirmations.Remove(confirmation);
            }
        }
    }
}
