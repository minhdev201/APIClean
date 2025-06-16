using ApiMES.Domain.Entities.Auth;
using ApiMES.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ApiMES.Infrastructure.DAOs.Common
{
    public class RefreshTokenDao(IMDbContext context)
    {
        private readonly IMDbContext _context = context;

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken)
        {
            var token = new RefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task RevokeAndAddAsync(RefreshToken oldToken, RefreshToken newToken)
        {
            _context.RefreshTokens.Update(oldToken);
            await _context.RefreshTokens.AddAsync(newToken);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
