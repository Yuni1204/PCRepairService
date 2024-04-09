using Microsoft.EntityFrameworkCore;
using PCRepairService.Interfaces;

namespace PCRepairService.DataAccess
{
    public class ServiceDBContextFactory : IDbContextFactory<ServiceDBContext>
    {
        private readonly IConfiguration _options;

        public ServiceDBContextFactory(IConfiguration options)
        {
            _options = options;
        }

        public ServiceDBContext CreateDbContext()
        {
            return new ServiceDBContext(_options);
        }
    }
}
