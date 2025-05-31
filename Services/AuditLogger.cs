using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.SQLite;
using Microsoft.Data.SqlClient;
using Dapper;

public class AuditLogger
{
    private readonly ILogger _logger;
    private readonly string _connectionString;

    public AuditLogger(string connectionString)
    {
        _connectionString = connectionString;
        _logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.SQLite("logs.db")
            .CreateLogger();
    }

    public void LogAction(int userId, string action, string ipAddress)
    {
        _logger.Information("User {UserId} performed {Action} from {IP}", userId, action, ipAddress);
        
        using var connection = new SqlConnection(_connectionString);
        connection.Execute(
            "INSERT INTO AuditLogs (UserId, Action, IPAddress) VALUES (@UserId, @Action, @IP)",
            new { UserId = userId, Action = action, IP = ipAddress });
    }
}