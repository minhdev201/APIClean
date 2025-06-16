using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiMES.Infrastructure.Database
{
    public class DbConnectionFactory(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration;

        public IDbConnection CreateConnection(string databaseKey)
        {
            var connectionString = _configuration.GetConnectionString(databaseKey);
            return new SqlConnection(connectionString);
        }
    }
}
