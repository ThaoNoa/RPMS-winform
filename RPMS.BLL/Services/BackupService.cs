using Microsoft.Data.SqlClient;
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
            ConnectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<string> BackupDatabaseAsync(string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException("Đường dẫn backup không hợp lệ.", nameof(destinationPath));

            var dir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var dbName = GetDatabaseName();
            var sql = $"BACKUP DATABASE [{EscapeIdent(dbName)}] TO DISK = @path WITH FORMAT, INIT, NAME = N'RPMS-Full', SKIP, NOREWIND, NOUNLOAD, STATS = 10;";

            await using var conn = new SqlConnection(BuildMasterConnectionString());
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@path", destinationPath);
            cmd.CommandTimeout = 300;
            await cmd.ExecuteNonQueryAsync();
            return destinationPath;
        }

        public async Task<string> RestoreDatabaseAsync(string bakFilePath)
        {
            if (string.IsNullOrWhiteSpace(bakFilePath) || !File.Exists(bakFilePath))
                throw new FileNotFoundException("Không tìm thấy file backup.", bakFilePath);

            var dbName = GetDatabaseName();
            var masterCs = BuildMasterConnectionString();

            await using var conn = new SqlConnection(masterCs);
            await conn.OpenAsync();

            // Kick connections off target DB, then restore.
            var killSql = $@"
IF DB_ID(N'{EscapeLiteral(dbName)}') IS NOT NULL
BEGIN
    ALTER DATABASE [{EscapeIdent(dbName)}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
END";
            await using (var killCmd = new SqlCommand(killSql, conn))
            {
                killCmd.CommandTimeout = 120;
                await killCmd.ExecuteNonQueryAsync();
            }

            var restoreSql = $@"
RESTORE DATABASE [{EscapeIdent(dbName)}]
FROM DISK = @path
WITH REPLACE, RECOVERY;";
            await using (var restoreCmd = new SqlCommand(restoreSql, conn))
            {
                restoreCmd.Parameters.AddWithValue("@path", bakFilePath);
                restoreCmd.CommandTimeout = 600;
                await restoreCmd.ExecuteNonQueryAsync();
            }

            var multiSql = $"ALTER DATABASE [{EscapeIdent(dbName)}] SET MULTI_USER;";
            await using (var multiCmd = new SqlCommand(multiSql, conn))
            {
                multiCmd.CommandTimeout = 60;
                await multiCmd.ExecuteNonQueryAsync();
            }

            return bakFilePath;
        }

        private string GetDatabaseName()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
                throw new InvalidOperationException("Connection string thiếu Database/Initial Catalog.");
            return builder.InitialCatalog;
        }

        private string BuildMasterConnectionString()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString)
            {
                InitialCatalog = "master"
            };
            return builder.ConnectionString;
        }

        private static string EscapeIdent(string name) => name.Replace("]", "]]");
        private static string EscapeLiteral(string name) => name.Replace("'", "''");
    }
}
