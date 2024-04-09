using Microsoft.EntityFrameworkCore;

namespace PCRepairService.Interfaces
{
    public interface IServiceDBContextFactory<TContext> where TContext : DbContext
    {
        TContext CreateDbContext();
    }
}
