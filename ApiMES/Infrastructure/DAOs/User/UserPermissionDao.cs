using ApiMES.Domain.Entities.Users;
using ApiMES.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiMES.Infrastructure.DAOs.User
{
    public class UserPermissionDao(UserManager<ApplicationUser> userManager,
                                      RoleManager<IdentityRole> roleManager,
                                      IMDbContext context)
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IMDbContext _context = context;


        //public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
        //{
        //    var user = await _userManager.FindByIdAsync(userId.ToString());
        //    var roles = await _userManager.GetRolesAsync(user);

        //    var permissions = await (from role in _roleManager.Roles
        //                             where roles.Contains(role.Name)
        //                             join rc in _context.RoleClaims on role.Id equals rc.RoleId
        //                             where rc.ClaimType == "permission"
        //                             select rc.ClaimValue).Distinct().ToListAsync();

        //    return permissions;
        //}

        public async Task<List<string>> GetUserPermissionsAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return [];

            var roles = await _userManager.GetRolesAsync(user);

            var permissions = await (from role in _roleManager.Roles
                                     where roles.Contains(role.Name!)
                                     join rc in _context.RoleClaims on role.Id equals rc.RoleId
                                     where rc.ClaimType == "permission"
                                     select rc.ClaimValue).Distinct().ToListAsync();

            return permissions;
        }
    }
}
