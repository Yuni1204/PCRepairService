using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;


namespace AsyncStopwatchCA
{
    public class CAStopDBContext : DbContext
    {
        public DbSet<Timestamps> Timestamps { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=postgresCont;Port=5432;Database=PCRepairDB;Username=postgres;Password=12345678;");
        }
    }
}
