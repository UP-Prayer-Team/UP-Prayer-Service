using System;
using System.Collections.Generic;

namespace UPPrayerService.Models
{
    public class Confirmation
    {
        public string ID { get; set; }
        public string Email { get; set; }
        public List<Reservation> Reservations { get; set; }

        public Confirmation()
        {
        }
    }
}
