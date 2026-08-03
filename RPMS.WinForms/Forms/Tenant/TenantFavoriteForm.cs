using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Tenant;
using RPMS.WinForms.Controls;
using RPMS.WinForms.Forms.Tenant;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Tenant
{
    public class TenantFavoriteForm : Form
    {
        private readonly ITenantInteractionService _interactionService;
        private ModernDataGridView dgv = null!;

        public TenantFavoriteForm(ITenantInteractionService interactionService)
        {
            _interactionService = interactionService;
            InitializeUI();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Phòng yêu thích";
            ClientSize = new Size(1000, 600);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = AppColors.Card };
            pnlTop.Controls.Add(new Label
            {
                Text = "Danh sách yêu thích",
                Font = AppTypography.Heading,
                ForeColor = AppColors.TextMain,
                Location = new Point(20, 18),
                AutoSize = true
            });

            dgv = new ModernDataGridView { Dock = DockStyle.Fill };
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", Width = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseName", HeaderText = "Nhà", Width = 160 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseAddress", HeaderText = "Địa chỉ", Width = 240 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Price",
                HeaderText = "Giá",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Area", HeaderText = "Diện tích", Width = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 100 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "BookCol", HeaderText = "", Text = "Đặt lịch", UseColumnTextForLinkValue = true, Width = 80 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "RemoveCol", HeaderText = "", Text = "Xóa", UseColumnTextForLinkValue = true, Width = 60, LinkColor = AppColors.Danger });
            dgv.CellContentClick += Dgv_CellContentClick!;

            Controls.Add(dgv);
            Controls.Add(pnlTop);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var list = await _interactionService.GetFavoritesAsync(UserSession.CurrentUser!.UserID);
                dgv.DataSource = list.ToList();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var item = dgv.Rows[e.RowIndex].DataBoundItem as FavoriteDto;
            if (item == null) return;
            string col = dgv.Columns[e.ColumnIndex].Name;

            try
            {
                if (col == "BookCol")
                {
                    var modal = Program.ServiceProvider.GetRequiredService<TenantAppointmentModalForm>();
                    modal.RoomIdToBook = item.RoomID;
                    modal.RoomInfo = $"Phòng {item.RoomNumber} - {item.HouseAddress}";
                    modal.ShowDialog();
                }
                else if (col == "RemoveCol")
                {
                    if (AppDialog.Confirm($"Xóa phòng {item.RoomNumber} khỏi yêu thích?"))
                    {
                        await _interactionService.RemoveFavoriteAsync(UserSession.CurrentUser!.UserID, item.RoomID);
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
