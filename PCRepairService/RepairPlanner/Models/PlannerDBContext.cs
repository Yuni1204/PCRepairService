//using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using RepairPlanner.Models;

namespace PCRepairService.Models
{
    public class PlannerDBContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        public DbSet<Worker> Worker { get; set; }

        public PlannerDBContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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
            modelBuilder.Entity<Worker>().HasKey(e => e.Id);
        }


    }
}
