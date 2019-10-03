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
    }
}
