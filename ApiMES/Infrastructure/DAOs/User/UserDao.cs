using ApiMES.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace ApiMES.Infrastructure.DAOs.User
{
    public class UserDao
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserDao(UserManager<ApplicationUser> userManager)
            => _userManager = userManager;

        public Task<ApplicationUser?> FindByUsernameAsync(string username)
            => _userManager.FindByNameAsync(username);

        public Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
            => _userManager.CheckPasswordAsync(user, password);

        public Task<ApplicationUser?> FindByIdAsync(string userId)
    => _userManager.FindByIdAsync(userId);
    }
}
