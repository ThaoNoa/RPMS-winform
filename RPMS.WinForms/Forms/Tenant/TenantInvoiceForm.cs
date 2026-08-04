using Microsoft.Extensions.DependencyInjection;
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
        private ModernDataGridView dgvInvoices = null!;
        private System.Collections.Generic.List<InvoiceDto> _invoices = new();

        public TenantInvoiceForm(IContractService contractService, IInvoiceService invoiceService)
        {
            _contractService = contractService;
            _invoiceService = invoiceService;
            InitializeUI();
            this.Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeUI()
        {
            Text = "Hóa đơn thanh toán";
            ClientSize = new Size(1000, 600);

            var btnExcel = UIHelper.PrimaryButton("Xuất Excel", 120);
            btnExcel.BackColor = AppColors.Success;
            btnExcel.Click += (s, e) => ExportExcel();

            var header = UIHelper.CreatePageHeader("Hóa đơn của tôi", btnExcel);

            dgvInvoices = new ModernDataGridView();
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InvoiceID", HeaderText = "ID", FillWeight = 6 });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InvoiceCode", HeaderText = "Mã Hóa Đơn", FillWeight = 16 });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 10 });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Total", HeaderText = "Tổng tiền", FillWeight = 12, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", FillWeight = 10 });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DueDate",
                HeaderText = "Hạn TT",
                FillWeight = 12,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgvInvoices.Columns.Add(new DataGridViewLinkColumn { Name = "DetailCol", HeaderText = "Chi tiết", Text = "Xem chi tiết", UseColumnTextForLinkValue = true, FillWeight = 12, LinkColor = Color.Blue });
            dgvInvoices.CellContentClick += DgvInvoices_CellContentClick;

            Controls.Add(dgvInvoices);
            Controls.Add(header);
            UIHelper.WireListPage(this, header, dgvInvoices);
            UIHelper.ApplyGridFill(dgvInvoices);
        }

        private void ExportExcel()
        {
            if (_invoices.Count == 0)
            {
                ToastNotifier.Show(this, "Chưa có hóa đơn để xuất", ToastKind.Warning);
                return;
            }
            ExportHelper.ExportExcelCsv(
                $"HoaDon_{DateTime.Now:yyyyMMdd}.csv",
                new[] { "Mã HĐ", "Phòng", "Tổng tiền", "Trạng thái", "Hạn TT" },
                _invoices.Select(i => new[]
                {
                    i.InvoiceCode ?? "",
                    i.RoomNumber ?? "",
                    i.Total.ToString("0"),
                    i.Status ?? "",
                    i.DueDate?.ToString("dd/MM/yyyy") ?? ""
                }));
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                if (IsDisposed) return;
                var contracts = await _contractService.GetContractsByTenantAsync(UserSession.CurrentUser!.UserID);
                var invoices = new System.Collections.Generic.List<InvoiceDto>();
                foreach (var c in contracts)
                {
                    var invs = await _invoiceService.GetInvoicesByContractAsync(c.ContractID);
                    invoices.AddRange(invs);
                }
                if (IsDisposed) return;
                _invoices = invoices.OrderByDescending(i => i.InvoiceID).ToList();
                dgvInvoices.DataSource = _invoices;
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Không tải được hóa đơn: " + ex.Message);
            }
        }

        private async void DgvInvoices_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvInvoices.Columns[e.ColumnIndex].Name != "DetailCol") return;
            var invoice = dgvInvoices.Rows[e.RowIndex].DataBoundItem as InvoiceDto;
            if (invoice == null) return;

            try
            {
                var detailForm = Program.ServiceProvider.GetRequiredService<InvoiceDetailForm>();
                detailForm.InvoiceId = invoice.InvoiceID;
                await detailForm.LoadAndShowAsync(this);
                if (detailForm.PaymentCompleted || detailForm.DialogResult == DialogResult.OK)
                    await LoadDataAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi mở chi tiết hóa đơn: " + ex.Message);
            }
        }
    }
}
