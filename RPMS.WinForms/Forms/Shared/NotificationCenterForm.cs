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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Shared
{
    public class NotificationCenterForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ModernDataGridView dgv = null!;
        private ModernTextBox txtSearch = null!;
        private ComboBox cboFilter = null!;
        private Label lblUnread = null!;

        public NotificationCenterForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            InitializeUI();
            Load += async (s, e) => await LoadDataAsync();
            Activated += async (s, e) => await LoadDataAsync();
        }

        private void InitializeUI()
        {
            Text = "Trung tâm thông báo";
            ClientSize = new Size(1000, 620);

            var btnRefresh = UIHelper.SecondaryButton("Làm mới", 110);
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            var btnMarkAll = UIHelper.PrimaryButton("Đọc tất cả", 120);
            btnMarkAll.BackColor = AppColors.Success;
            btnMarkAll.Click += async (s, e) =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<INotificationService>()
                        .MarkAllAsReadAsync(UserSession.CurrentUser!.UserID);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    AppDialog.ShowError("Không đánh dấu đọc: " + ex.Message);
                }
            };

            var header = UIHelper.CreatePageHeader("Thông báo", btnRefresh, btnMarkAll);
            lblUnread = UIHelper.GetPageHeaderTitle(header);

            txtSearch = new ModernTextBox { PlaceholderText = "Tìm tiêu đề / nội dung" };
            cboFilter = new ComboBox();
            UIHelper.StyleCombo(cboFilter);
            cboFilter.Items.AddRange(new object[] { "Tất cả", "Chưa đọc", "Đã đọc" });
            cboFilter.SelectedIndex = 0;

            var filterBar = UIHelper.CreateFilterBar();
            filterBar.Controls.Add(UIHelper.CreateLabeledField("Tìm kiếm", txtSearch, 260));
            filterBar.Controls.Add(UIHelper.CreateLabeledField("Lọc", cboFilter, 140));

            dgv = new ModernDataGridView();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NotificationID", HeaderText = "ID", FillWeight = 6 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "Tiêu đề", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Content", HeaderText = "Nội dung", FillWeight = 36 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CreatedDate",
                HeaderText = "Thời gian",
                FillWeight = 14,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IsRead", HeaderText = "Đã đọc", FillWeight = 8 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "ViewCol", HeaderText = "", Text = "Xem", UseColumnTextForLinkValue = true, FillWeight = 6 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "ReadCol", HeaderText = "", Text = "Đánh dấu đọc", UseColumnTextForLinkValue = true, FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "DeleteCol", HeaderText = "", Text = "Xóa", UseColumnTextForLinkValue = true, FillWeight = 6, LinkColor = AppColors.Danger });
            dgv.CellContentClick += Dgv_CellContentClick!;

            Controls.Add(dgv);
            Controls.Add(filterBar);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, dgv);
            UIHelper.ApplyGridFill(dgv);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                bool? isRead = cboFilter.SelectedIndex switch
                {
                    1 => false,
                    2 => true,
                    _ => null
                };

                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var list = await svc.GetByUserAsync(
                    UserSession.CurrentUser!.UserID,
                    isRead,
                    txtSearch.Text);
                if (IsDisposed) return;
                dgv.DataSource = list.ToList();

                int unread = await svc.GetUnreadCountAsync(UserSession.CurrentUser.UserID);
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
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();

                if (col == "ViewCol")
                {
                    if (!item.IsRead)
                        await svc.MarkAsReadAsync(item.NotificationID);
                    AppDialog.ShowInfo($"{item.Title}\n\n{item.Content}\n\n{item.CreatedDate:dd/MM/yyyy HH:mm}", "Chi tiết thông báo");
                    await LoadDataAsync();
                }
                else if (col == "ReadCol")
                {
                    await svc.MarkAsReadAsync(item.NotificationID);
                    await LoadDataAsync();
                }
                else if (col == "DeleteCol")
                {
                    if (AppDialog.Confirm("Xóa thông báo này?"))
                    {
                        await svc.DeleteAsync(item.NotificationID);
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
