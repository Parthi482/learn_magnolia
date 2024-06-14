using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Magnolia_cares.database
{
    public class SqlConnections
    {
        private static readonly object _lock = new object();
        private static SqlConnection _connection;
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static SqlConnection GetConnection()
        {
            lock (_lock)
            {
                if (_connection == null)
                {
                    string connectionString = _configuration["ConnectionStrings:ConnectionString"];  
                    _connection = new SqlConnection(connectionString);
                    _connection.Open();
                }
            }
            return _connection;
        }

        public static void Dispose()
        {
            lock (_lock)
            {
                _connection?.Dispose();
                _connection = null;
            }
        }
    }
}
