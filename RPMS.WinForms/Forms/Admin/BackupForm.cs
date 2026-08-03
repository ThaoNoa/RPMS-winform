using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Admin
{
    public class BackupForm : Form
    {
        private readonly IBackupService _backupService;
        private Label lblStatus = null!;

        public BackupForm(IBackupService backupService)
        {
            _backupService = backupService;
            InitializeUI();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Backup & Restore Database";
            ClientSize = new Size(720, 360);

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = AppColors.Card };
            pnl.Controls.Add(new Label
            {
                Text = "Sao lưu / Khôi phục cơ sở dữ liệu RPMS",
                Font = AppTypography.Heading,
                Location = new Point(24, 24),
                AutoSize = true,
                ForeColor = AppColors.TextMain
            });
            pnl.Controls.Add(new Label
            {
                Text = "Backup tạo file .bak bằng SQL Server BACKUP DATABASE.\nRestore yêu cầu quyền admin LocalDB và sẽ reconnect lại DB.",
                Location = new Point(24, 70),
                Size = new Size(640, 50),
                ForeColor = AppColors.TextMuted
            });

            var btnBackup = new ModernButton
            {
                Text = "Backup Database",
                Location = new Point(24, 140),
                Size = new Size(180, 42)
            };
            btnBackup.Click += async (s, e) => await BackupAsync();

            var btnRestore = new ModernButton
            {
                Text = "Restore Database",
                Location = new Point(220, 140),
                Size = new Size(180, 42),
                BackColor = AppColors.Warning
            };
            btnRestore.Click += async (s, e) => await RestoreAsync();

            lblStatus = new Label
            {
                Location = new Point(24, 210),
                Size = new Size(640, 80),
                ForeColor = AppColors.TextMain
            };

            pnl.Controls.Add(btnBackup);
            pnl.Controls.Add(btnRestore);
            pnl.Controls.Add(lblStatus);
            Controls.Add(pnl);
        }

        private async System.Threading.Tasks.Task BackupAsync()
        {
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "RPMS_Backups",
                $"RPMS_{DateTime.Now:yyyyMMdd_HHmmss}.bak");

            if (!ExportHelper.SaveFile("Backup (*.bak)|*.bak", Path.GetFileName(defaultPath), out var path))
                return;

            try
            {
                lblStatus.Text = "Đang backup...";
                await _backupService.BackupDatabaseAsync(path);
                lblStatus.Text = "Backup thành công:\n" + path;
                AppDialog.ShowInfo("Backup thành công.\n" + path);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                AppDialog.ShowError(ex.Message);
            }
        }

        private async System.Threading.Tasks.Task RestoreAsync()
        {
            using var ofd = new OpenFileDialog { Filter = "Backup (*.bak)|*.bak" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            if (!AppDialog.Confirm("Restore sẽ ghi đè database hiện tại. Tiếp tục?"))
                return;

            try
            {
                lblStatus.Text = "Đang restore...";
                await _backupService.RestoreDatabaseAsync(ofd.FileName);
                lblStatus.Text = "Restore thành công. Khuyến nghị khởi động lại ứng dụng.";
                AppDialog.ShowInfo("Restore thành công. Hãy khởi động lại ứng dụng.");
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                AppDialog.ShowError(ex.Message);
            }
        }
    }
}
