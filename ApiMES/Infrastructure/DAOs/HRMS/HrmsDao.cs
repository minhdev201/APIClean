using ApiMES.Application.DTOs.Users;
using ApiMES.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ApiMES.Infrastructure.DAOs.HRMS
{
    public class HrmsDao
    {
        private readonly GateDbContext _context;

        public HrmsDao(GateDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeDTO>> LoadFromDatabaseAsync()
        {
            return await _context.Set<EmployeeDTO>()
                .FromSqlInterpolated($"EXEC GetUsers")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EmployeeDTO?> GetByIdAsync(string id)
        {
            var normalized = id.ToUpper();
            var list = await LoadFromDatabaseAsync();
            return list.FirstOrDefault(e => e.EmployeeID == normalized);
        }
    }
}
