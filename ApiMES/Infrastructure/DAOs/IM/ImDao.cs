using ApiMES.Application.DTOs.Auth;
using ApiMES.Infrastructure.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ApiMES.Infrastructure.DAOs.IM
{
    public class ImDao
    {
        private readonly IMDbContext _context;
        private readonly ILogger<ImDao> _logger;

        public ImDao(IMDbContext context, ILogger<ImDao> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CheckTCodeAsync(string username, string tcode, CancellationToken cancellationToken = default)
        {
            try
            {
                var parameters = new[]
                {
                new SqlParameter("@username", SqlDbType.NVarChar) { Value = username },
                new SqlParameter("@tcode", SqlDbType.NVarChar) { Value = tcode }
            };

                var exists = await Task.Run(() =>
                    _context.Set<CheckTCodeDTO>()
                        .FromSqlRaw("EXEC GetTcode2UserName @username, @tcode", parameters)
                        .AsNoTracking()
                        .AsEnumerable()
                        .Any(), cancellationToken);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking TCode for user: {Username}", username);
                return false;
            }
        }
    }
}
