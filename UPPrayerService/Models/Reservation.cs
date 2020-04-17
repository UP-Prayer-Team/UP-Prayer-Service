using System;
namespace UPPrayerService.Models
{
    public class Reservation
    {
        public string ID { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public string District { get; set; }
        public int Year { get; set; }
        public int MonthIndex { get; set; }
        public int DayIndex { get; set; }
        public int SlotIndex { get; set; }
        public bool IsConfirmed { get; set; }

        public Reservation()
        {
        }



        /// <summary>
        /// Takes the reservation info and converts it to a DataTime object.
        /// </summary>
        /// <returns>Returns a new date time object.</returns>
        public DateTime GetStartTime()
        {
            int hour;
            int minute;
            DateTime currentReservationDate;

            // Convert the slot index to military time.
            hour = Convert.ToInt32(Math.Floor((double)this.SlotIndex / 2));
            minute = 30 * (this.SlotIndex % 2);

            // Creates a new date object. Note the DayIndex and MonthIndex are both 0 base so you have to add
            // one to get the correct date and month.
            currentReservationDate = new DateTime(this.Year, this.MonthIndex + 1,
                this.DayIndex + 1, hour, minute, 0);

            return currentReservationDate;
        }
    }
}
