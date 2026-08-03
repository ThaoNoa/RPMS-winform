using Microsoft.Data.SqlClient;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class BackupService : IBackupService
    {
        public string ConnectionString { get; }

        public BackupService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public async Task<string> BackupDatabaseAsync(string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new BadRequestException("Đường dẫn backup không hợp lệ.");

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            var dbName = GetDatabaseName();
            var sql = $@"
BACKUP DATABASE [{dbName}]
TO DISK = @path
WITH FORMAT, INIT, NAME = N'RPMS-Full-Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10;";

            await using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@path", destinationPath);
            cmd.CommandTimeout = 300;
            await cmd.ExecuteNonQueryAsync();
            return destinationPath;
        }

        public async Task<string> RestoreDatabaseAsync(string bakFilePath)
        {
            if (!File.Exists(bakFilePath))
                throw new BadRequestException("File backup không tồn tại.");

            var dbName = GetDatabaseName();
            var masterCs = new SqlConnectionStringBuilder(ConnectionString)
            {
                InitialCatalog = "master"
            }.ConnectionString;

            var sql = $@"
ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [{dbName}] FROM DISK = @path WITH REPLACE;
ALTER DATABASE [{dbName}] SET MULTI_USER;";

            await using var conn = new SqlConnection(masterCs);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@path", bakFilePath);
            cmd.CommandTimeout = 600;
            await cmd.ExecuteNonQueryAsync();
            return bakFilePath;
        }

        private string GetDatabaseName()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
                throw new BadRequestException("Connection string thiếu Database.");
            return builder.InitialCatalog;
        }
    }
}
