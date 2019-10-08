using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UPPrayerService.Models;

namespace UPPrayerService
{
    public class DataContext : DbContext
    {
        public DbSet<Endorsement> Endorsements { get; set; }
        public DbSet<Confirmation> Confirmations { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        public DataContext(DbContextOptions<DataContext> dataOptions) : base(dataOptions)
        {

        }
    }
}
