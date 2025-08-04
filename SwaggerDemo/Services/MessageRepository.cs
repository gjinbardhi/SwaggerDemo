using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using SwaggerDemo.Models;
using SwaggerDemo.Interfaces;    

namespace SwaggerDemo.Services
{
    public class MessageRepository : IMessageRepository   
    {
        private readonly string _connectionString;

        public MessageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task AddAsync(MessageDto message)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    INSERT INTO Messages (Id, Type, Recipient, Content, Status, RetryCount, MaxRetries)
                    VALUES (@Id, @Type, @Recipient, @Content, @Status, @RetryCount, @MaxRetries)";
                await connection.ExecuteAsync(sql, message);
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to insert message into the database.", ex);
            }
        }

        public async Task UpdateStatusAsync(MessageDto message)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"
                    UPDATE Messages
                    SET Status = @Status,
                        RetryCount = @RetryCount
                    WHERE Id = @Id";
                await connection.ExecuteAsync(sql, message);
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Failed to update status for message {message.Id}.", ex);
            }
        }

        public async Task<List<MessageDto>> GetAllAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"SELECT 
                    CAST(Id AS CHAR(36)) AS Id, 
                    Type, Recipient, Content, Status, RetryCount, MaxRetries 
                    FROM Messages";
                var messages = await connection.QueryAsync<MessageDto>(sql);
                return messages.AsList();
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to retrieve all messages.", ex);
            }
        }

        public async Task<List<MessageDto>> GetDeadMessagesAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                const string sql = @"SELECT 
                    CAST(Id AS CHAR(36)) AS Id, 
                    Type, Recipient, Content, Status, RetryCount, MaxRetries 
                    FROM Messages 
                    WHERE Status = 'Dead'";
                var messages = await connection.QueryAsync<MessageDto>(sql);
                return messages.AsList();
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to retrieve dead messages.", ex);
            }
        }
    }
}
