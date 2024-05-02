using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;


namespace AsyncTestStopwatch.DataAccess
{
    public class StopwatchDBContext : DbContext
    {
        IConfiguration _configuration;
        public DbSet<StopwatchModel> Timestamps { get; set; }

        public StopwatchDBContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PCRepairDB"));
        }
    }
}
