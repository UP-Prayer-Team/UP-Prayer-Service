using System;
using System.Collections.Generic;

namespace UPPrayerService.Models
{
    public class Confirmation
    {
        public string ConfirmationID { get; set; }
        public string Email { get; set; }
        public List<string> Reservations { get; set; }

        public Confirmation()
        {
        }
    }
}
