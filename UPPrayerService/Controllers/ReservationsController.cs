﻿using System;
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
    public class ReservationsController : ControllerBase
    {
        private ReservationService ReservationService { get; set; }
        private DataContext DataContext { get; set; }
        public ReservationsController(ReservationService reservationService, DataContext dataContext)
        {
            this.ReservationService = reservationService;
            this.DataContext = dataContext;
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

            List<ReservationsResponseLocation>[] results = new List<ReservationsResponseLocation>[24 * 2];
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
            public string EndorsementID { get; set; }
            public string Country { get; set; }
            public string District { get; set; }
            public Slot[] Slots { get; set; }
        }

        // POST: api/reservations/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync(CreateReservationsRequest request)
        {
            // Validate that the email is not already awaiting confirmation
            if (ReservationService.DoesEmailHavePendingConfirmation(request.Email))
            {
                return this.MakeFailure("Email already has pending confirmations.", StatusCodes.Status400BadRequest);
            }

            // if ID is present && invalid
            if (!String.IsNullOrEmpty(request.EndorsementID) && !DataContext.Endorsements.Any((Endorsement e) => e.ID == request.EndorsementID))
            {
                return this.MakeFailure("Invalid Endorsement ID.", StatusCodes.Status400BadRequest);
            }
            

            foreach (CreateReservationsRequest.Slot slot in request.Slots)
            {
                // Validate that the slot is valid
                if (!this.SlotIsValid(slot.Year, slot.MonthIndex, slot.DayIndex, slot.SlotIndex))
                {
                    return this.MakeFailure("Date or slot is not valid.", StatusCodes.Status400BadRequest);
                }

                // TODO: Validate that slot is in the future

                // TODO: Validate that slot is not already reserved by the email
            }

            // TODO: Validate the country & district

            Confirmation confirmation = new Confirmation() { ID = ReservationService.GenerateConfirmationID(), Email = request.Email, Reservations = new List<Reservation>() };
            foreach (CreateReservationsRequest.Slot slot in request.Slots)
            {
                Reservation reservation = new Reservation() { ID = ReservationService.GenerateReservationID(), Email = request.Email, IsConfirmed = false, EndorsementID = request.EndorsementID, Country = request.Country, District = request.District, Year = slot.Year, MonthIndex = slot.MonthIndex, DayIndex = slot.DayIndex, SlotIndex = slot.SlotIndex };
                ReservationService.AddReservation(reservation);
                confirmation.Reservations.Add(reservation);
            }
            ReservationService.AddConfirmation(confirmation);

            await ReservationService.SendConfirmationCode(request.Email, confirmation.ID, confirmation.Reservations);

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
                List<Reservation> confirmedReservations = ReservationService.Confirm(request.ConfirmationID);
                List<object> slots = new List<object>();
                foreach (Reservation reservation in confirmedReservations)
                {
                    slots.Add(new { year = reservation.Year, monthIndex = reservation.MonthIndex, dayIndex = reservation.DayIndex, slotIndex = reservation.SlotIndex });
                }
                return this.MakeSuccess(new { slots = slots });
            }
            catch (KeyNotFoundException)
            {
                return this.MakeFailure("No such confirmation found.", StatusCodes.Status404NotFound);
            }
            
        }
    }
}
