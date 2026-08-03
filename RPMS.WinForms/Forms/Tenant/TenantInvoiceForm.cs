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
            this.ClientSize = new Size(1000, 600);
            this.BackColor = AppColors.Background;
            this.Text = "Hóa đơn thanh toán";
            this.MinimumSize = new Size(700, 480);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = AppColors.Card };
            pnlTop.Controls.Add(new Label
            {
                Text = "Hóa đơn của tôi",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                Location = new Point(20, 14),
                AutoSize = true
            });
            var btnExcel = new ModernButton
            {
                Text = "Xuất Excel",
                Size = new Size(120, 36),
                BackColor = AppColors.Success,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(860, 10)
            };
            btnExcel.Click += (s, e) => ExportExcel();
            pnlTop.Resize += (s, e) => btnExcel.Left = Math.Max(200, pnlTop.Width - btnExcel.Width - 16);
            pnlTop.Controls.Add(btnExcel);

            dgvInvoices = new ModernDataGridView { Dock = DockStyle.Fill };
            dgvInvoices.AutoGenerateColumns = false;
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InvoiceID", HeaderText = "ID", Width = 50 });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InvoiceCode", HeaderText = "Mã Hóa Đơn", Width = 140 });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", Width = 80 });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Total", HeaderText = "Tổng tiền", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 90 });
            dgvInvoices.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DueDate",
                HeaderText = "Hạn TT",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgvInvoices.Columns.Add(new DataGridViewLinkColumn { Name = "DetailCol", HeaderText = "Chi tiết", Text = "Xem chi tiết", UseColumnTextForLinkValue = true, Width = 110, LinkColor = Color.Blue });
            dgvInvoices.CellContentClick += DgvInvoices_CellContentClick;
            this.Controls.Add(dgvInvoices);
            this.Controls.Add(pnlTop);
            UIHelper.WireListPage(this, pnlTop, dgvInvoices);
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
