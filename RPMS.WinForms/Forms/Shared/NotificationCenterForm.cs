using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Notification;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Shared
{
    public class NotificationCenterForm : Form
    {
        private readonly INotificationService _notificationService;
        private ModernDataGridView dgv = null!;
        private ModernTextBox txtSearch = null!;
        private ComboBox cboFilter = null!;
        private Label lblUnread = null!;

        public NotificationCenterForm(INotificationService notificationService)
        {
            _notificationService = notificationService;
            InitializeUI();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Trung tâm thông báo";
            ClientSize = new Size(1000, 620);
            AutoScroll = false;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = AppColors.Card };
            lblUnread = new Label
            {
                Text = "Thông báo",
                Font = AppTypography.Heading,
                ForeColor = AppColors.TextMain,
                Location = new Point(20, 20),
                AutoSize = true
            };

            txtSearch = new ModernTextBox { Location = new Point(280, 18), Size = new Size(220, 35), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            cboFilter = new ComboBox
            {
                Location = new Point(520, 20),
                Size = new Size(140, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cboFilter.Items.AddRange(new object[] { "Tất cả", "Chưa đọc", "Đã đọc" });
            cboFilter.SelectedIndex = 0;

            var btnSearch = new ModernButton { Text = "Lọc", Location = new Point(680, 18), Size = new Size(90, 35), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnSearch.Click += async (s, e) => await LoadDataAsync();

            var btnMarkAll = new ModernButton
            {
                Text = "Đọc tất cả",
                Location = new Point(780, 18),
                Size = new Size(110, 35),
                BackColor = AppColors.Success,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnMarkAll.Click += async (s, e) =>
            {
                try
                {
                    await _notificationService.MarkAllAsReadAsync(UserSession.CurrentUser!.UserID);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    AppDialog.ShowError("Không đánh dấu đọc: " + ex.Message);
                }
            };

            pnlTop.Controls.AddRange(new Control[] { lblUnread, txtSearch, cboFilter, btnSearch, btnMarkAll });

            dgv = new ModernDataGridView { Dock = DockStyle.Fill };
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NotificationID", HeaderText = "ID", Width = 50 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tiêu đề", Width = 220 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Content", HeaderText = "Nội dung", Width = 360 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CreatedDate",
                HeaderText = "Thời gian",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IsRead", HeaderText = "Đã đọc", Width = 70 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "ViewCol", HeaderText = "", Text = "Xem", UseColumnTextForLinkValue = true, Width = 50 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "ReadCol", HeaderText = "", Text = "Đánh dấu đọc", UseColumnTextForLinkValue = true, Width = 100 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "DeleteCol", HeaderText = "", Text = "Xóa", UseColumnTextForLinkValue = true, Width = 50, LinkColor = AppColors.Danger });
            dgv.CellContentClick += Dgv_CellContentClick!;

            Controls.Add(dgv);
            Controls.Add(pnlTop);
            UIHelper.WireListPage(this, pnlTop, dgv);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                bool? isRead = cboFilter.SelectedIndex switch
                {
                    1 => false,
                    2 => true,
                    _ => null
                };

                var list = await _notificationService.GetByUserAsync(
                    UserSession.CurrentUser!.UserID,
                    isRead,
                    txtSearch.Text);
                if (IsDisposed) return;
                dgv.DataSource = list.ToList();

                int unread = await _notificationService.GetUnreadCountAsync(UserSession.CurrentUser.UserID);
                if (IsDisposed) return;
                lblUnread.Text = $"Thông báo ({unread} chưa đọc)";
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Không tải thông báo: " + ex.Message);
            }
        }

        private async void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var item = dgv.Rows[e.RowIndex].DataBoundItem as NotificationDto;
            if (item == null) return;
            string col = dgv.Columns[e.ColumnIndex].Name;

            try
            {
                if (col == "ViewCol")
                {
                    if (!item.IsRead)
                        await _notificationService.MarkAsReadAsync(item.NotificationID);
                    AppDialog.ShowInfo($"{item.Title}\n\n{item.Content}\n\n{item.CreatedDate:dd/MM/yyyy HH:mm}", "Chi tiết thông báo");
                    await LoadDataAsync();
                }
                else if (col == "ReadCol")
                {
                    await _notificationService.MarkAsReadAsync(item.NotificationID);
                    await LoadDataAsync();
                }
                else if (col == "DeleteCol")
                {
                    if (AppDialog.Confirm("Xóa thông báo này?"))
                    {
                        await _notificationService.DeleteAsync(item.NotificationID);
                        await LoadDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }
    }
}
