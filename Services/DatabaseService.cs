using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Reportly.Models;
using MySqlConnector;

namespace Reportly.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly DatabaseSettings _settings;
        private MySqlConnection _connection;

        public DatabaseService(IConfiguration configuration)
        {
            _settings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
        }

        public async Task<MySqlConnection> GetConnectionAsync()
        {
            if (_connection == null)
            {
                _connection = new MySqlConnection(_settings.MySqlConnectionString);
                await _connection.OpenAsync();
            }
            else if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }

            return _connection;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params MySqlParameter[] parameters)
        {
            int retryCount = 0;
            while (retryCount < _settings.MaxRetryCount)
            {
                try
                {
                    using var connection = await GetConnectionAsync();
                    using var command = new MySqlCommand(sql, connection)
                    {
                        CommandTimeout = _settings.CommandTimeout
                    };

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    return await command.ExecuteNonQueryAsync();
                }
                catch (MySqlException ex) when (IsTransientError(ex) && retryCount < _settings.MaxRetryCount - 1)
                {
                    retryCount++;
                    await Task.Delay(1000 * retryCount);
                }
            }
            throw new Exception("Failed to execute query after retries");
        }

        public async Task<DataTable> ExecuteQueryAsync(string sql, params MySqlParameter[] parameters)
        {
            int retryCount = 0;
            while (retryCount < _settings.MaxRetryCount)
            {
                try
                {
                    using var connection = await GetConnectionAsync();
                    //var command = new SqlParameter(sql, connection)
                    // {
                    //    CommandTimeout = _settings.CommandTimeout
                    //};
                    var command = new MySqlCommand(sql){ CommandTimeout = _settings.CommandTimeout };

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using var adapter = new MySqlDataAdapter(command);
                    var dataTable = new DataTable();
                    await Task.Run(() => adapter.Fill(dataTable));
                    return dataTable;
                }
                catch (MySqlException ex) when (IsTransientError(ex) && retryCount < _settings.MaxRetryCount - 1)
                {
                    retryCount++;
                    await Task.Delay(1000 * retryCount);
                }
            }
            throw new Exception("Failed to execute query after retries");
        }

        private bool IsTransientError(MySqlException ex)
        {
            // List of transient error codes
            return ex.Number switch
            {
                1042 => true, // Unable to connect to any of the specified MySQL hosts
                1205 => true, // Lock wait timeout exceeded
                1213 => true, // Deadlock found
                2006 => true, // MySQL server has gone away
                _ => false
            };
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}