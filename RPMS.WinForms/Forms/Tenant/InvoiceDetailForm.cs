using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.DTO.Invoice;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Tenant
{
    public class InvoiceDetailForm : Form
    {
        private readonly IInvoiceService _invoiceService;
        private InvoiceDetailDto _invoice = null!;
        private ModernButton btnPay = null!;

        public bool PaymentCompleted { get; private set; }
        public int InvoiceId { get; set; }

        public InvoiceDetailForm(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public async System.Threading.Tasks.Task LoadAndShowAsync(IWin32Window? owner = null)
        {
            _invoice = await _invoiceService.GetInvoiceByIdAsync(InvoiceId);
            BuildUi();
            ShowDialog(owner);
        }

        private void BuildUi()
        {
            SuspendLayout();
            Controls.Clear();
            UIHelper.ApplyResizableDialog(this, new Size(720, 560));
            Text = $"Chi tiết hóa đơn {_invoice.InvoiceCode}";
            ClientSize = new Size(820, 740);
            StartPosition = FormStartPosition.CenterParent;
            AutoScroll = false;

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = AppColors.Card,
                Padding = new Padding(16, 10, 16, 10)
            };
            var flpButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            btnPay = new ModernButton
            {
                Text = "Thanh toán",
                Size = new Size(130, 40),
                BackColor = AppColors.Success,
                Visible = string.Equals(_invoice.Status, "Unpaid", StringComparison.OrdinalIgnoreCase),
                Margin = new Padding(0, 0, 8, 0)
            };
            btnPay.Click += async (s, e) => await PayAsync();

            var btnPrint = new ModernButton { Text = "In", Size = new Size(100, 40), BackColor = AppColors.Primary, Margin = new Padding(0, 0, 8, 0) };
            btnPrint.Click += (s, e) => InvoicePrintHelper.OpenAndPrint(_invoice);

            var btnPdf = new ModernButton { Text = "Xuất PDF", Size = new Size(110, 40), BackColor = AppColors.TextMuted, Margin = new Padding(0, 0, 8, 0) };
            btnPdf.Click += (s, e) => ExportPdf();

            var btnClose = new ModernButton { Text = "Đóng", Size = new Size(100, 40), BackColor = AppColors.Border, ForeColor = AppColors.TextMain };
            btnClose.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            flpButtons.Controls.AddRange(new Control[] { btnPay, btnPrint, btnPdf, btnClose });
            pnlBottom.Controls.Add(flpButtons);

            var root = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = AppColors.Background
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                BackColor = AppColors.Background
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            int row = 0;
            void AddRow(Control c, float height = 0, bool auto = true)
            {
                tbl.RowStyles.Add(auto
                    ? new RowStyle(SizeType.AutoSize)
                    : new RowStyle(SizeType.Absolute, height));
                c.Dock = DockStyle.Fill;
                tbl.Controls.Add(c, 0, row++);
            }

            var lblTitle = new Label
            {
                Text = $"CHI TIẾT HÓA ĐƠN {_invoice.InvoiceCode}",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.Primary,
                AutoSize = true,
                MaximumSize = new Size(760, 0),
                Padding = new Padding(0, 8, 0, 8)
            };
            AddRow(lblTitle);

            AddRow(CreateInfoCard("Thông tin hóa đơn",
                ("Mã hóa đơn", _invoice.InvoiceCode),
                ("Trạng thái", _invoice.Status),
                ("Hạn thanh toán", _invoice.DueDate?.ToString("dd/MM/yyyy") ?? "-"),
                ("Tháng chỉ số", _invoice.ReadingMonth?.ToString("MM/yyyy") ?? "-"),
                ("Ngày thanh toán", _invoice.PaidDate?.ToString("dd/MM/yyyy") ?? "-")));

            AddRow(CreateInfoCard("Thông tin khách thuê",
                ("Họ tên", _invoice.TenantName),
                ("Điện thoại", string.IsNullOrWhiteSpace(_invoice.TenantPhone) ? "-" : _invoice.TenantPhone),
                ("Email", string.IsNullOrWhiteSpace(_invoice.TenantEmail) ? "-" : _invoice.TenantEmail)));

            AddRow(CreateInfoCard("Thông tin phòng",
                ("Phòng", _invoice.RoomNumber),
                ("Nhà", string.IsNullOrWhiteSpace(_invoice.HouseName) ? "-" : _invoice.HouseName),
                ("Địa chỉ", string.IsNullOrWhiteSpace(_invoice.HouseAddress) ? "-" : _invoice.HouseAddress),
                ("Diện tích", _invoice.RoomArea.HasValue ? $"{_invoice.RoomArea:0.##} m²" : "-")));

            AddRow(CreateInfoCard("Thông tin hợp đồng",
                ("Mã hợp đồng", _invoice.ContractCode),
                ("Thời hạn", $"{_invoice.ContractStartDate:dd/MM/yyyy} → {_invoice.ContractEndDate:dd/MM/yyyy}"),
                ("Nhận phòng", _invoice.MoveInDate?.ToString("dd/MM/yyyy") ?? "-"),
                ("Giá điện", $"{_invoice.ElectricPrice:N0} đ/số"),
                ("Giá nước", $"{_invoice.WaterPrice:N0} đ/m³"),
                ("Trạng thái HĐ", string.IsNullOrWhiteSpace(_invoice.ContractStatus) ? "-" : _invoice.ContractStatus)));

            AddRow(new Label
            {
                Text = "Chi tiết điện / nước / phí khác",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 4)
            });

            var dgv = new ModernDataGridView
            {
                Height = 130,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Khoản mục", DataPropertyName = "Item", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chỉ số cũ", DataPropertyName = "Old", FillWeight = 14 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chỉ số mới", DataPropertyName = "New", FillWeight = 14 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tiêu thụ", DataPropertyName = "Usage", FillWeight = 12 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đơn giá", DataPropertyName = "UnitPrice", FillWeight = 14 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thành tiền", DataPropertyName = "Amount", FillWeight = 16 });
            dgv.DataSource = new[]
            {
                new { Item = "Điện", Old = _invoice.OldElectric.ToString("N0"), New = _invoice.NewElectric.ToString("N0"), Usage = (_invoice.NewElectric - _invoice.OldElectric).ToString("N0"), UnitPrice = _invoice.ElectricPrice.ToString("N0"), Amount = _invoice.ElectricCost.ToString("N0") },
                new { Item = "Nước", Old = _invoice.OldWater.ToString("N0"), New = _invoice.NewWater.ToString("N0"), Usage = (_invoice.NewWater - _invoice.OldWater).ToString("N0"), UnitPrice = _invoice.WaterPrice.ToString("N0"), Amount = _invoice.WaterCost.ToString("N0") },
                new { Item = "Phí khác", Old = "-", New = "-", Usage = "-", UnitPrice = "-", Amount = _invoice.OtherFee.ToString("N0") }
            };
            AddRow(dgv, 130, false);

            AddRow(CreateSummaryPanel());

            root.Controls.Add(tbl);
            Controls.Add(root);
            Controls.Add(pnlBottom);
            ResumeLayout(true);
        }

        private static Panel CreateInfoCard(string title, params (string Label, string Value)[] rows)
        {
            var card = new Panel
            {
                AutoSize = true,
                BackColor = AppColors.Card,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(12, 8, 12, 8)
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            tbl.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppColors.Primary,
                AutoSize = true,
                MaximumSize = new Size(720, 0),
                Margin = new Padding(0, 0, 0, 4)
            }, 0, 0);
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            int r = 1;
            foreach (var (label, value) in rows)
            {
                tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tbl.Controls.Add(new Label
                {
                    Text = $"{label}:  {value}",
                    ForeColor = AppColors.TextMain,
                    AutoSize = true,
                    MaximumSize = new Size(720, 0),
                    Margin = new Padding(0, 2, 0, 2)
                }, 0, r++);
            }

            card.Controls.Add(tbl);
            return card;
        }

        private Panel CreateSummaryPanel()
        {
            var summary = new Panel
            {
                AutoSize = true,
                BackColor = AppColors.Card,
                Margin = new Padding(0, 8, 0, 16),
                Padding = new Padding(12, 8, 12, 8)
            };
            summary.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, summary.Width - 1, summary.Height - 1);
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));

            int r = 0;
            void AddSummaryLine(string label, string value, bool emphasize)
            {
                tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tbl.Controls.Add(new Label
                {
                    Text = label,
                    Font = emphasize ? new Font("Segoe UI", 12F, FontStyle.Bold) : new Font("Segoe UI", 10F),
                    ForeColor = emphasize ? AppColors.Primary : AppColors.TextMain,
                    AutoSize = true,
                    MaximumSize = new Size(520, 0),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    Margin = new Padding(4, emphasize ? 6 : 3, 0, emphasize ? 6 : 3)
                }, 0, r);
                tbl.Controls.Add(new Label
                {
                    Text = value,
                    Font = emphasize ? new Font("Segoe UI", 12F, FontStyle.Bold) : new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = emphasize ? AppColors.Primary : AppColors.TextMain,
                    AutoSize = true,
                    MaximumSize = new Size(220, 0),
                    TextAlign = ContentAlignment.TopRight,
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    Margin = new Padding(0, emphasize ? 6 : 3, 4, emphasize ? 6 : 3)
                }, 1, r++);
            }

            string rentLabel = _invoice.IsProrated
                ? $"Tiền phòng ({_invoice.OccupiedDays}/{_invoice.DaysInMonth} ngày)"
                : "Tiền phòng";
            AddSummaryLine(rentLabel, $"{_invoice.Rent:N0} đ", false);

            if (_invoice.IsProrated && !string.IsNullOrWhiteSpace(_invoice.RentNote))
            {
                tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var noteLbl = new Label
                {
                    Text = _invoice.RentNote,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                    ForeColor = AppColors.TextMuted,
                    AutoSize = true,
                    MaximumSize = new Size(720, 0),
                    Margin = new Padding(4, 0, 4, 4)
                };
                tbl.Controls.Add(noteLbl, 0, r);
                tbl.SetColumnSpan(noteLbl, 2);
                r++;
            }

            AddSummaryLine("Điện", $"{_invoice.ElectricCost:N0} đ", false);
            AddSummaryLine("Nước", $"{_invoice.WaterCost:N0} đ", false);
            AddSummaryLine("Phí khác", $"{_invoice.OtherFee:N0} đ", false);
            AddSummaryLine("TỔNG TIỀN", $"{_invoice.Total:N0} đ", true);

            summary.Controls.Add(tbl);
            return summary;
        }

        private async System.Threading.Tasks.Task PayAsync()
        {
            if (!AppDialog.Confirm(
                    $"Xác nhận đã chuyển khoản {_invoice.Total:N0} đ cho hóa đơn {_invoice.InvoiceCode}?",
                    "Thanh toán"))
                return;

            try
            {
                btnPay.Enabled = false;
                await _invoiceService.ProcessPaymentAsync(_invoice.InvoiceID, new ProcessPaymentDto
                {
                    Amount = _invoice.Total,
                    Method = "Banking"
                });
                PaymentCompleted = true;
                AppDialog.ShowInfo("Xác nhận thanh toán thành công!");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
                btnPay.Enabled = true;
            }
        }

        private void ExportPdf()
        {
            if (!ExportHelper.SaveFile("HTML (*.html)|*.html", $"{_invoice.InvoiceCode}.html", out var path))
                return;
            InvoicePrintHelper.ExportHtml(_invoice, path);
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch { /* ignore */ }
            AppDialog.ShowInfo("Đã xuất file HTML.\nMở trình duyệt → In → chọn 'Microsoft Print to PDF' để lưu PDF.\n\n" + path);
        }
    }
}
