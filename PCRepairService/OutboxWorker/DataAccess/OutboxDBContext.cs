using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessengerLibrary;

namespace OutboxWorker.DataAccess
{
    public class OutboxDBContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        public DbSet<Message> OutboxMessages { get; set; }

        public OutboxDBContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PCRepairDB"));
        }
    }
}
