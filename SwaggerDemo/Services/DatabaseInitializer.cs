using System.Data;
using Dapper;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using SwaggerDemo.Services;
using SwaggerDemo.Middleware;

namespace SwaggerDemo.Services
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void EnsureTablesExist()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sql = @"
                CREATE TABLE IF NOT EXISTS Messages (
                    Id CHAR(36) PRIMARY KEY,
                    Type VARCHAR(100) NOT NULL,
                    Recipient VARCHAR(100) NOT NULL,
                    Content TEXT NOT NULL,
                    Status VARCHAR(50) NOT NULL,
                    RetryCount INT NOT NULL,
                    MaxRetries INT NOT NULL
                );";

            connection.Execute(sql);
        }
    }
}
