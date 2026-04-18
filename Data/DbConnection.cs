using MySql.Data.MySqlClient;

namespace WebApplication1.Data
{
    // Handles creation of MySQL database connections using app configuration
    public class DbConnection
    {
        private readonly IConfiguration _config;

        // Constructor injects app configuration to access connection strings
        public DbConnection(IConfiguration config)
        {
            _config = config;
        }

        // Returns a new MySQL connection using the default connection string from appsettings.json
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(
                _config.GetConnectionString("DefaultConnection")
            );
        }
    }
}