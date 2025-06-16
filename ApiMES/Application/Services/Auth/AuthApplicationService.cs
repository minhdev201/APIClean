using ApiMES.Domain.Entities;
using ApiMES.Infrastructure.DAOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApiMES.Application.Services.Auth
{
    public class AuthApplicationService(RefreshTokenDao refreshTokenDao, UserDao userDao, IConfiguration configuration, UserManager<ApplicationUser> userManager, UserPermissionDao userPermissionDao)
    {
        private readonly RefreshTokenDao _refreshTokenDao = refreshTokenDao;
        private readonly UserDao _userDao = userDao;
        private readonly IConfiguration _configuration = configuration;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly UserPermissionDao _userPermissionsService = userPermissionDao;

        public async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _userPermissionsService.GetUserPermissionsAsync(user.UserName!);

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

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<(bool, string, string, string[])> ValidateUserAsync(string username, string password)
        {
            var user = await _userDao.FindByUsernameAsync(username);
            if (user == null) return (false, "", "", new[] { "User not found." });

            var valid = await _userDao.CheckPasswordAsync(user, password);
            if (!valid) return (false, "", "", new[] { "Invalid credentials." });

            var accessToken = await GenerateTokenAsync(user);
            var refreshToken = GenerateRefreshToken();

            await _refreshTokenDao.SaveRefreshTokenAsync(user.Id, refreshToken);

            return (true, accessToken, refreshToken, Array.Empty<string>());
        }

        public async Task<(bool IsSuccess, string AccessToken, string NewRefreshToken, string ErrorMessage)> RefreshAccessTokenAsync(string oldRefreshToken)
        {
            var tokenEntry = await _refreshTokenDao.GetByTokenAsync(oldRefreshToken);

            if (tokenEntry == null || tokenEntry.IsRevoked || tokenEntry.ExpiresAt < DateTime.UtcNow)
                return (false, "", "", "Invalid or expired refresh token.");

            var user = await _userDao.FindByIdAsync(tokenEntry.UserId);

            if (user == null)
                return (false, "", "", "User not found.");

            // Sinh token mới
            var newAccessToken = await GenerateTokenAsync(user);
            var newRefreshToken = GenerateRefreshToken();

            // Lưu token mới vào DB
            var newTokenEntry = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7) // hoặc 30 ngày
            };

            // Thu hồi token cũ
            tokenEntry.IsRevoked = true;
            tokenEntry.RevokedAt = DateTime.UtcNow;

            await _refreshTokenDao.RevokeAndAddAsync(tokenEntry, newTokenEntry);

            return (true, newAccessToken, newRefreshToken, "");
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var token = await _refreshTokenDao.GetByTokenAsync(refreshToken);

            if (token == null || token.IsRevoked)
                return false;

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;

            await _refreshTokenDao.UpdateAsync(token);
            return true;
        }
    }
}
