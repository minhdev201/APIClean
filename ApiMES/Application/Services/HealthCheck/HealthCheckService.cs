using ApiMES.Infrastructure.Database;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiMES.Application.Services.HealthCheck
{
    public class HealthCheckService(GateDbContext context, ILogger<HealthCheckService> logger)
    {
        private readonly GateDbContext _context = context;
        private readonly ILogger<HealthCheckService> _logger = logger;

        public async Task<bool> TestDatabaseConnectionAsync()
        {
            DbConnection? connection = null;
            try
            {
                connection = _context.Database.GetDbConnection();
                _logger.LogInformation("🔍 Attempting to connect to database...");

                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                var builder = new SqlConnectionStringBuilder(connection.ConnectionString);

                _logger.LogInformation("✅ Database connection successful.");
                _logger.LogInformation("📡 Server: {Server}", builder.DataSource);
                _logger.LogInformation("📁 Database: {Database}", builder.InitialCatalog);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to connect to the database.");
                return false;
            }
            finally
            {
                if (connection?.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                    _logger.LogInformation("🔒 Connection closed.");
                }
            }
        }
    }
}
