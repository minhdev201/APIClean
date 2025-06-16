using ApiMES.Application.DTOs.Users;
using Microsoft.EntityFrameworkCore;

namespace ApiMES.Infrastructure.Database
{
    public class GateDbContext : DbContext
    {
        public GateDbContext(DbContextOptions<GateDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EmployeeDTO>().HasNoKey();
        }
    }
}
