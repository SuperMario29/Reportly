using System;
using System.IO;
using Microsoft.Data.SqlClient;

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
        var backupFile = Path.Combine(_backupPath, $"FacebookAds_{DateTime.Now:yyyyMMdd}.bak");
        
        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand(
            $"BACKUP DATABASE FacebookAds TO DISK = '{backupFile}' WITH COMPRESSION",
            connection);

        connection.Open();
        command.ExecuteNonQuery();
    }
}