//using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using RepairPlanner.Models;

namespace PCRepairService.Models
{
    public class PlannerDBContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        public DbSet<PServiceOrder> ServiceOrders { get; set; }

        public PlannerDBContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PCRepairDB"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Worker>().HasKey(e => e.Id);
            modelBuilder.Entity<PServiceOrder>().HasKey(e => e.Id);
        }


    }
}
