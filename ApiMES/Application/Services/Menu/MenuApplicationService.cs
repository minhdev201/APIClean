using ApiMES.Application.DTOs.Menu;
using ApiMES.Domain.Entities.Menu;
using ApiMES.Domain.Entities.Users;
using ApiMES.Infrastructure.DAOs.Common;
using ApiMES.Infrastructure.DAOs.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ApiMES.Application.Services.Menu
{
    public class MenuApplicationService(MenuDao menuDao, UserPermissionDao userPermissionDao, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        private readonly MenuDao _menuDao = menuDao;
        private readonly UserPermissionDao _userPermissionDao = userPermissionDao;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IConfiguration _configuration = configuration;

        public async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _userPermissionDao.GetUserPermissionsAsync(user.UserName!);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.NameIdentifier, user.Id)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            claims.AddRange(permissions.Select(p => new Claim("permission", p)));

            var secretKey = _configuration["Jwt:Key"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<List<MenuDto>> GetMenuForUserAsync(string userId)
        {
            var userPermissions = await _userPermissionDao.GetUserPermissionsAsync(userId);
            var allMenuItems = await _menuDao.GetAllMenuItemsAsync();

            return BuildMenuTree(allMenuItems, null, userPermissions);
        }

        private static List<MenuDto> BuildMenuTree(IEnumerable<MenuItem> allMenus, string? parentId, List<string> userPermissions)
        {
            return [.. allMenus
                .Where(m => m.parentId == parentId && HasPermission(m, userPermissions))
                .OrderBy(m => m.sortOrder)
                .Select(m => new MenuDto
                {
                    id = m.id.ToString(),
                    title = m.title,
                    type = m.type,
                    icon = m.icon,
                    img = m.img,
                    url = m.url,
                    classes = m.classes,
                    target = m.target,
                    breadcrumbs = m.breadcrumbs,
                    permissions = JsonSerializer.Deserialize<List<string>>(m.permissions!),
                    children = BuildMenuTree(allMenus, m.id, userPermissions)
                })];
        }

        private static bool HasPermission(MenuItem menu, List<string> userPermissions)
        {
            var requiredPermissions = JsonSerializer.Deserialize<List<string>>(menu.permissions ?? "[]");
            return requiredPermissions!.Count == 0 || requiredPermissions.Any(p => userPermissions.Contains(p));
        }
    }
}
