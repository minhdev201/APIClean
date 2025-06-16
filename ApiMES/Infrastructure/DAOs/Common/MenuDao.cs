using ApiMES.Domain.Entities.Menu;
using ApiMES.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ApiMES.Infrastructure.DAOs.Common
{
    public class MenuDao(IMDbContext context)
    {
        private readonly IMDbContext _context = context;

        public async Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            return await _context.MenuItems.ToListAsync();
        }
    }
}
