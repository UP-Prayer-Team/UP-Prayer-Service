using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using UPPrayerService.Models;
using UPPrayerService.Services;

namespace UPPrayerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController()]
    public class ReservationsController : Controller
    {
        private ReservationService ReservationService { get; set; }

        public ReservationsController(ReservationService reservationService)
        {
            this.ReservationService = reservationService;
        }

        private class ReservationsResponseLocation
        {
            public string Country { get; set; }
            public string District { get; set; }
        }

        private class ReservationsSummaryResponseDay
        {
            public int Count { get; set; }
            public List<ReservationsResponseLocation> Locations { get; set; } = new List<ReservationsResponseLocation>();
        }

        // GET: api/reservations/summary?year=<year>&month=<month>
        [HttpGet("summary")]
        public IActionResult Summary(int year, int month)
        {
            DateTime start = new DateTime(year, month + 1, 1);
            DateTime end = new DateTime(year + (month + 1) / 12, ((month + 1) % 12) + 1, 1);
            int monthCount = (end - start).Days;

            ReservationsSummaryResponseDay[] results = new ReservationsSummaryResponseDay[monthCount];
            for (int i = 0; i < monthCount; i++)
            {
                results[i] = new ReservationsSummaryResponseDay();
            }

            IEnumerable<Reservation> reservations = ReservationService.GetReservations(start, end, true);

            foreach (Reservation reservation in reservations)
            {
                results[reservation.DayIndex].Count++;
                if (!String.IsNullOrEmpty(reservation.Country))
                {
                    results[reservation.DayIndex].Locations.Add(new ReservationsResponseLocation() { Country = reservation.Country, District = reservation.District });
                }
            }

            return this.MakeSuccess(results);
        }

        // GET: api/reservations/day?year=<year>&month=<month>&day=<day>
        [HttpGet("day")]
        public IActionResult Day(int year, int month, int day)
        {
            DateTime start = new DateTime(year, month + 1, day + 1);
            DateTime end = new DateTime(year, month + 1, day + 2);

            List<ReservationsResponseLocation>[] results = new List<ReservationsResponseLocation>[30];
            IEnumerable<Reservation> reservations = ReservationService.GetReservations(start, end, true);

            foreach (Reservation reservation in reservations)
            {
                results[reservation.DayIndex].Add(new ReservationsResponseLocation() { Country = reservation.Country, District = reservation.District });
            }

            return this.MakeSuccess(results);
        }

        public class CreateReservationsRequest
        {
            public class Slot
            {
                public int Year { get; set; }
                public int MonthIndex { get; set; }
                public int DayIndex { get; set; }
                public int SlotIndex { get; set; }
            }

            public string Email { get; set; }
            public string Country { get; set; }
            public string District { get; set; }
            public Slot[] Slots { get; set; }
        }

        // POST: api/reservations/create
        [HttpPost("create")]
        public IActionResult Create(CreateReservationsRequest request)
        {
            foreach (CreateReservationsRequest.Slot slot in request.Slots)
            {
                // TODO: Validate that slot is in the future
                if (!this.SlotIsValid(slot.Year, slot.MonthIndex, slot.DayIndex, slot.SlotIndex))
                {
                    return this.MakeFailure("Date or slot is not valid.", StatusCodes.Status400BadRequest);
                }
            }

            // TODO: Validate the country & district

            Confirmation confirmation = new Confirmation() { ConfirmationID = ReservationService.GenerateConfirmationID(), Email = request.Email, Reservations = new List<string>() };
            foreach (CreateReservationsRequest.Slot slot in request.Slots)
            {
                Reservation reservation = new Reservation() { ID = ReservationService.GenerateReservationID(), Email = request.Email, IsConfirmed = false, Country = request.Country, District = request.District, Year = slot.Year, MonthIndex = slot.MonthIndex, DayIndex = slot.DayIndex, SlotIndex = slot.SlotIndex };
                ReservationService.AddReservation(reservation);
                confirmation.Reservations.Add(reservation.ID);
            }
            ReservationService.AddConfirmation(confirmation);
            return this.MakeSuccess();
        }

        public class ConfirmReservationsRequest
        {
            public string ConfirmationID { get; set; }
        }

        // POST: api/reservations/confirm
        [HttpPost("confirm")]
        public IActionResult Confirm(ConfirmReservationsRequest request)
        {
            try
            {
                ReservationService.Confirm(request.ConfirmationID);
            }
            catch (KeyNotFoundException)
            {
                return this.MakeFailure("No such confirmation found.", StatusCodes.Status404NotFound);
            }
            return this.MakeSuccess();
        }
    }
}
