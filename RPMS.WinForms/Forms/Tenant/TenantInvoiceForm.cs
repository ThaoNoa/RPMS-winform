using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Invoice;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Tenant
{
    public class TenantInvoiceForm : Form
    {
        private readonly IContractService _contractService;
        private readonly IInvoiceService _invoiceService;
        private ModernDataGridView dgvInvoices;

        public TenantInvoiceForm(IContractService contractService, IInvoiceService invoiceService)
        {
            _contractService = contractService;
            _invoiceService = invoiceService;
            InitializeUI();
            this.Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeUI()
        {
            this.ClientSize = new Size(1000, 600);
            this.BackColor = AppColors.Background;
            this.Text = "Hóa đơn thanh toán";

            dgvInvoices = new ModernDataGridView { Dock = DockStyle.Fill };
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InvoiceID", HeaderText = "ID", Width = 50 });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InvoiceCode", HeaderText = "Mã Hóa Đơn", Width = 150 });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", Width = 100 });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Total", HeaderText = "Tổng tiền", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 100 });
            dgvInvoices.Columns.Add(new DataGridViewLinkColumn { Name = "DetailCol", HeaderText = "Chi tiết", Text = "Xem & Chuyển khoản", UseColumnTextForLinkValue = true, Width = 150, LinkColor = Color.Blue });
            dgvInvoices.CellContentClick += DgvInvoices_CellContentClick;
            this.Controls.Add(dgvInvoices);
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            var contracts = await _contractService.GetContractsByTenantAsync(UserSession.CurrentUser!.UserID);
            var invoices = new System.Collections.Generic.List<InvoiceDto>();
            foreach (var c in contracts)
            {
                var invs = await _invoiceService.GetInvoicesByContractAsync(c.ContractID);
                invoices.AddRange(invs);
            }
            dgvInvoices.DataSource = invoices.OrderByDescending(i => i.InvoiceID).ToList();
        }

        private async void DgvInvoices_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvInvoices.Columns[e.ColumnIndex].Name != "DetailCol") return;
            var invoice = dgvInvoices.Rows[e.RowIndex].DataBoundItem as InvoiceDto;

            var detail = await _invoiceService.GetInvoiceByIdAsync(invoice.InvoiceID);
            string msg = $"CHI TIẾT HÓA ĐƠN {detail.InvoiceCode}\n" +
                         $"========================\n" +
                         $"Tiền phòng: {detail.Rent:N0} đ\n" +
                         $"Tiền điện: {detail.ElectricCost:N0} đ (Số mới: {detail.NewElectric} - Cũ: {detail.OldElectric})\n" +
                         $"Tiền nước: {detail.WaterCost:N0} đ (Số mới: {detail.NewWater} - Cũ: {detail.OldWater})\n" +
                         $"Phí dịch vụ/khác: {detail.OtherFee:N0} đ\n\n" +
                         $"TỔNG CỘNG: {detail.Total:N0} đ\n" +
                         $"Trạng thái: {detail.Status}\n\n";

            if (detail.Status == "Unpaid")
            {
                msg += "Bạn có muốn XÁC NHẬN ĐÃ CHUYỂN KHOẢN cho hóa đơn này không?";
                if (AppDialog.Confirm(msg, "Thanh toán"))
                {
                    await _invoiceService.ProcessPaymentAsync(detail.InvoiceID, new ProcessPaymentDto { Amount = detail.Total, Method = "Banking" });
                    AppDialog.ShowInfo("Xác nhận thanh toán thành công!");
                    await LoadDataAsync();
                }
            }
            else
            {
                AppDialog.ShowInfo(msg, "Chi tiết hóa đơn");
            }
        }
    }
}