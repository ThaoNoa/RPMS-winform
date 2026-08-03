using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Maintenance;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Manager
{
    public class MaintenanceDetailForm : Form
    {
        private readonly IMaintenanceService _maintenanceService;
        private MaintenanceRequestDto _req = null!;
        private PictureBox picImage = null!;
        private Label lblStatus = null!;
        private Label lblHeadline = null!;
        private Label lblInfo = null!;
        private Label lblDesc = null!;
        private ModernButton btnProcess = null!;
        private ModernButton btnComplete = null!;
        private ModernButton btnClose = null!;

        public int RequestId { get; set; }
        public bool Changed { get; private set; }

        public MaintenanceDetailForm(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
            BuildUi();
            Load += async (s, e) => await LoadAsync();
        }

        private void BuildUi()
        {
            UIHelper.ApplyResizableDialog(this, new Size(720, 560));
            Text = "Chi tiết sự cố";
            ClientSize = new Size(860, 680);
            MinimumSize = new Size(720, 560);
            BackColor = AppColors.Background;
            StartPosition = FormStartPosition.CenterParent;
            AutoScroll = false;

            // Footer — luôn bám đáy
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = AppColors.Card,
                Padding = new Padding(16, 12, 16, 12)
            };
            pnlBottom.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(pen, 0, 0, pnlBottom.Width, 0);
            };
            var flpButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            btnProcess = new ModernButton
            {
                Text = "Tiếp nhận & Hẹn",
                Size = new Size(150, 40),
                BackColor = AppColors.Primary,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnProcess.Click += async (s, e) => await ProcessAsync();
            btnComplete = new ModernButton
            {
                Text = "Hoàn thành",
                Size = new Size(130, 40),
                BackColor = AppColors.Success,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnComplete.Click += async (s, e) => await CompleteAsync();
            btnClose = new ModernButton
            {
                Text = "Đóng",
                Size = new Size(100, 40),
                BackColor = AppColors.Border,
                ForeColor = AppColors.TextMain
            };
            btnClose.Click += (s, e) => Close();
            flpButtons.Controls.AddRange(new Control[] { btnProcess, btnComplete, btnClose });
            pnlBottom.Controls.Add(flpButtons);

            // Header
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = AppColors.Card,
                Padding = new Padding(20, 12, 20, 12)
            };
            pnlHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };
            lblHeadline = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.Primary,
                Text = "Chi tiết sự cố",
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            lblStatus = new Label
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Padding(10, 6, 10, 6),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblHeadline);
            pnlHeader.Controls.Add(lblStatus);

            // Body — TableLayout 2 hàng: info full-width, desc|image
            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(16),
                BackColor = AppColors.Background
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 42f));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 58f));

            var cardInfo = MakeCard();
            cardInfo.Controls.Add(MakeSection("Thông tin yêu cầu"));
            lblInfo = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                ForeColor = AppColors.TextMain,
                AutoSize = false,
                Padding = new Padding(4)
            };
            // Scrollable info area
            var infoHost = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12, 40, 12, 12) };
            infoHost.Controls.Add(lblInfo);
            lblInfo.Dock = DockStyle.Top;
            lblInfo.AutoSize = true;
            lblInfo.MaximumSize = new Size(10, 0); // updated on resize
            infoHost.Resize += (s, e) =>
            {
                lblInfo.MaximumSize = new Size(Math.Max(100, infoHost.ClientSize.Width - 8), 0);
            };
            cardInfo.Controls.Add(infoHost);
            body.Controls.Add(cardInfo, 0, 0);
            body.SetColumnSpan(cardInfo, 2);

            var cardDesc = MakeCard();
            cardDesc.Controls.Add(MakeSection("Mô tả sự cố"));
            var descHost = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12, 40, 12, 12) };
            lblDesc = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = AppColors.TextMain,
                MaximumSize = new Size(10, 0)
            };
            descHost.Controls.Add(lblDesc);
            descHost.Resize += (s, e) =>
            {
                lblDesc.MaximumSize = new Size(Math.Max(100, descHost.ClientSize.Width - 8), 0);
            };
            cardDesc.Controls.Add(descHost);
            body.Controls.Add(cardDesc, 0, 1);

            var cardImg = MakeCard();
            cardImg.Controls.Add(MakeSection("Ảnh đính kèm"));
            var imgHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 40, 12, 12) };
            picImage = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(241, 245, 249),
                Cursor = Cursors.Hand
            };
            picImage.Click += (s, e) => OpenFullscreen();
            imgHost.Controls.Add(picImage);
            cardImg.Controls.Add(imgHost);
            body.Controls.Add(cardImg, 1, 1);

            Controls.Add(body);
            Controls.Add(pnlHeader);
            Controls.Add(pnlBottom);
        }

        private static Panel MakeCard()
        {
            var p = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Card,
                Margin = new Padding(6),
                Padding = new Padding(0)
            };
            p.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };
            return p;
        }

        private static Label MakeSection(string text) => new()
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 36,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = AppColors.TextMain,
            Padding = new Padding(12, 10, 12, 0)
        };

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                _req = await _maintenanceService.GetRequestByIdAsync(RequestId);
                Text = $"Sự cố #{_req.RequestID}";
                lblHeadline.Text = string.IsNullOrWhiteSpace(_req.Title) ? $"Sự cố #{_req.RequestID}" : _req.Title;

                lblInfo.Text =
                    $"Mã yêu cầu: #{_req.RequestID}\n" +
                    $"Phòng: {_req.RoomNumber}  ·  Nhà: {(string.IsNullOrWhiteSpace(_req.HouseName) ? "—" : _req.HouseName)}\n" +
                    $"Địa chỉ: {(string.IsNullOrWhiteSpace(_req.HouseAddress) ? "—" : _req.HouseAddress)}\n" +
                    $"Khách thuê: {(string.IsNullOrWhiteSpace(_req.TenantName) ? "—" : _req.TenantName)}" +
                    (string.IsNullOrWhiteSpace(_req.TenantPhone) ? "" : $"  ·  {_req.TenantPhone}") + "\n" +
                    $"Hợp đồng: {(string.IsNullOrWhiteSpace(_req.ContractCode) ? "—" : _req.ContractCode)}\n" +
                    $"Ngày gửi: {_req.CreatedDate:dd/MM/yyyy HH:mm}\n" +
                    $"Quản lý phụ trách: {(string.IsNullOrWhiteSpace(_req.AssignedManagerName) ? "Chưa gán" : _req.AssignedManagerName)}" +
                    (_req.CompletedDate.HasValue ? $"\nHoàn thành: {_req.CompletedDate:dd/MM/yyyy HH:mm}" : "");

                lblDesc.Text = string.IsNullOrWhiteSpace(_req.Description) ? "(Không có mô tả)" : _req.Description;
                ApplyStatusBadge(_req.Status);
                ImagePathHelper.ApplyToPictureBox(picImage, _req.ImagePath, "Không có ảnh");
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Không tải chi tiết sự cố: " + ex.Message);
                Close();
            }
        }

        private void ApplyStatusBadge(string status)
        {
            lblStatus.Text = status switch
            {
                "Pending" => "  Chờ xử lý  ",
                "Processing" => "  Đang xử lý  ",
                "Completed" => "  Hoàn thành  ",
                _ => "  " + status + "  "
            };
            lblStatus.BackColor = status switch
            {
                "Pending" => Color.FromArgb(254, 243, 199),
                "Processing" => Color.FromArgb(219, 234, 254),
                "Completed" => Color.FromArgb(220, 252, 231),
                _ => AppColors.Border
            };
            lblStatus.ForeColor = status switch
            {
                "Pending" => Color.FromArgb(146, 64, 14),
                "Processing" => AppColors.Primary,
                "Completed" => Color.FromArgb(22, 101, 52),
                _ => AppColors.TextMain
            };
        }

        private void OpenFullscreen()
        {
            if (string.IsNullOrWhiteSpace(_req?.ImagePath)) return;
            using var f = new Form
            {
                WindowState = FormWindowState.Maximized,
                BackColor = Color.Black,
                FormBorderStyle = FormBorderStyle.None,
                KeyPreview = true
            };
            var pic = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            ImagePathHelper.ApplyToPictureBox(pic, _req.ImagePath);
            f.Controls.Add(pic);
            f.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) f.Close(); };
            pic.Click += (s, e) => f.Close();
            f.ShowDialog(this);
        }

        private async System.Threading.Tasks.Task ProcessAsync()
        {
            if (_req.Status == "Completed")
            {
                AppDialog.ShowInfo("Sự cố này đã hoàn thành.");
                return;
            }
            var msg = AppDialog.Prompt(
                "Nhập thông báo hẹn ngày sửa chữa gửi khách thuê:",
                "Tiếp nhận sự cố",
                "Quản lý đã tiếp nhận và sẽ đến kiểm tra vào ngày mai.");
            if (string.IsNullOrEmpty(msg)) return;
            try
            {
                await _maintenanceService.UpdateRequestStatusAsync(_req.RequestID, "Processing", UserSession.CurrentUser!.UserID);
                await _maintenanceService.SendMaintenanceNotificationAsync(_req.RequestID, msg);
                Changed = true;
                AppDialog.ShowInfo("Đã tiếp nhận và gửi thông báo.");
                await LoadAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async System.Threading.Tasks.Task CompleteAsync()
        {
            if (_req.Status == "Completed") return;
            if (!AppDialog.Confirm("Xác nhận sự cố đã được khắc phục hoàn tất?", "Hoàn thành")) return;
            try
            {
                await _maintenanceService.UpdateRequestStatusAsync(_req.RequestID, "Completed", UserSession.CurrentUser!.UserID);
                await _maintenanceService.SendMaintenanceNotificationAsync(_req.RequestID, "Sự cố của bạn đã được khắc phục hoàn tất.");
                Changed = true;
                AppDialog.ShowInfo("Đã đánh dấu hoàn thành.");
                await LoadAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }
    }
}
