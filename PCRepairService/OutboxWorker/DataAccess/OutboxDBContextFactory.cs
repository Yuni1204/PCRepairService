using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutboxWorker.DataAccess
{
    public class OutboxDBContextFactory : IDbContextFactory<OutboxDBContext>
    {
        private readonly IConfiguration _options;

        public OutboxDBContextFactory(IConfiguration options)
        {
            _options = options;
        }

        public OutboxDBContext CreateDbContext()
        {
            return new OutboxDBContext(_options);
        }
    }
}
