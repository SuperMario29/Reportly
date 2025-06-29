using System;
using System.IO;
using MySqlConnector;

public class SqlBackupService
{
    private readonly string _connectionString;
    private readonly string _backupPath;

    public SqlBackupService(string connectionString, string backupPath)
    {
        _connectionString = connectionString;
        _backupPath = backupPath;
    }

    public void PerformBackup()
    {
        var backupFile = Path.Combine(_backupPath, $"Reportly_{DateTime.Now:yyyyMMdd}.bak");
        
        using var connection = new MySqlConnection(_connectionString);
        var command = new MySqlCommand(
            $"BACKUP DATABASE FacebookAds TO DISK = '{backupFile}' WITH COMPRESSION",
            connection);

        connection.Open();
        command.ExecuteNonQuery();
    }
}