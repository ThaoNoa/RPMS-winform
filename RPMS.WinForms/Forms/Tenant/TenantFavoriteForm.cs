using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Tenant;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Tenant
{
    public class TenantFavoriteForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ModernDataGridView dgv = null!;

        public TenantFavoriteForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            InitializeUI();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeUI()
        {
            Text = "Phòng yêu thích";
            ClientSize = new Size(1000, 600);

            var header = UIHelper.CreatePageHeader("Danh sách yêu thích");

            dgv = new ModernDataGridView();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseName", HeaderText = "Nhà", FillWeight = 16 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseAddress", HeaderText = "Địa chỉ", FillWeight = 24 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Price",
                HeaderText = "Giá",
                FillWeight = 12,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Area", HeaderText = "Diện tích", FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "BookCol", HeaderText = "", Text = "Đặt lịch", UseColumnTextForLinkValue = true, FillWeight = 9 });
            dgv.Columns.Add(new DataGridViewLinkColumn { Name = "RemoveCol", HeaderText = "", Text = "Xóa", UseColumnTextForLinkValue = true, FillWeight = 7, LinkColor = AppColors.Danger });
            dgv.CellContentClick += Dgv_CellContentClick!;

            Controls.Add(dgv);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, dgv);
            UIHelper.ApplyGridFill(dgv);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var list = await System.Threading.Tasks.Task.Run(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    return (await scope.ServiceProvider.GetRequiredService<ITenantInteractionService>()
                        .GetFavoritesAsync(UserSession.CurrentUser!.UserID)
                        .ConfigureAwait(false)).ToList();
                }).ConfigureAwait(true);
                if (!IsDisposed)
                    dgv.DataSource = list;
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
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
                    using var scope = _scopeFactory.CreateScope();
                    var modal = scope.ServiceProvider.GetRequiredService<TenantAppointmentModalForm>();
                    modal.RoomIdToBook = item.RoomID;
                    modal.RoomInfo = $"Phòng {item.RoomNumber} - {item.HouseAddress}";
                    modal.ShowDialog(this);
                }
                else if (col == "RemoveCol")
                {
                    if (AppDialog.Confirm($"Xóa phòng {item.RoomNumber} khỏi yêu thích?"))
                    {
                        await System.Threading.Tasks.Task.Run(async () =>
                        {
                            using var scope = _scopeFactory.CreateScope();
                            await scope.ServiceProvider.GetRequiredService<ITenantInteractionService>()
                                .RemoveFavoriteAsync(UserSession.CurrentUser!.UserID, item.RoomID)
                                .ConfigureAwait(false);
                        }).ConfigureAwait(true);
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
