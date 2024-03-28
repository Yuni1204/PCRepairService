﻿using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using PCRepairService.Models;

namespace PCRepairService.Models
{
    public class ServiceDBContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        public DbSet<Kunde> Kunde { get; set; }
        public DbSet<ServiceOrder> ServiceOrder { get; set; }
        public DbSet<AISO> AISO { get; set; }

        public ServiceDBContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        //{
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            //    var config = new ConfigurationBuilder()
            //        .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), ".."))
            //        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //        .Build();

            //    var connectionString = config.GetConnectionString("PCRepairDB");
            //    optionsBuilder.UseNpgsql(connectionString);
            //}
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PCRepairDB"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kunde>().HasKey(e => e.Id);
            modelBuilder.Entity<ServiceOrder>().HasKey(e => e.Id);
            modelBuilder.Entity<ServiceOrder>()
                .HasOne(e => e.Kunde)
                .WithMany()
                .HasForeignKey(e => e.KundeId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<AISO>().HasKey(e => e.Id);
        }


    }
}