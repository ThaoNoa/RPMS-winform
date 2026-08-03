using RPMS.BLL.Interfaces;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IBackupService
    {
        Task<string> BackupDatabaseAsync(string destinationPath);
        Task<string> RestoreDatabaseAsync(string bakFilePath);
        string ConnectionString { get; }
    }
}
