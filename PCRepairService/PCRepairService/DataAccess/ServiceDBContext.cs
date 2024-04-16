using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using PCRepairService.Interfaces;
using PCRepairService.Models;
using MessengerLibrary;

namespace PCRepairService.DataAccess
{
    public class ServiceDBContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        public DbSet<ServiceOrder> ServiceOrders { get; set; }
        public DbSet<Message> OutboxMessages { get; set; }
        public DbSet<SagaServiceOrder> ServiceOrderSagaLog { get; set; }

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
            modelBuilder.Entity<ServiceOrder>().HasKey(e => e.Id);
            modelBuilder.Entity<Message>().HasKey(e => e.Id);
        }


    }
}
