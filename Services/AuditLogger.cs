using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.MySQL;
using Dapper;
using MySqlConnector;

public class AuditLogger
{
    private readonly ILogger _logger;
    private readonly string _connectionString;

    public AuditLogger(string connectionString)
    {
        _connectionString = connectionString;
        _logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.MySQL("logs.db")
            .CreateLogger();
    }

    public void LogAction(int userId, string action, string ipAddress)
    {
        _logger.Information("User {UserId} performed {Action} from {IP}", userId, action, ipAddress);
        
        using var connection = new MySqlConnection(_connectionString);
        connection.Execute(
            "INSERT INTO AuditLogs (UserId, Action, IPAddress) VALUES (@UserId, @Action, @IP)",
            new { UserId = userId, Action = action, IP = ipAddress });
    }
}