using RPMS.DTO.Invoice;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RPMS.WinForms.UI
{
    public static class InvoicePrintHelper
    {
        public static string BuildHtml(InvoiceDetailDto inv)
        {
            decimal usageE = inv.NewElectric - inv.OldElectric;
            decimal usageW = inv.NewWater - inv.OldWater;
            return $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><title>Hóa đơn {Esc(inv.InvoiceCode)}</title>
<style>
body{{font-family:'Segoe UI',sans-serif;margin:32px;color:#111827;line-height:1.45}}
h1{{text-align:center;color:#2563EB;margin-bottom:4px}}
.sub{{text-align:center;color:#6B7280;margin-bottom:24px}}
.grid{{display:grid;grid-template-columns:1fr 1fr;gap:12px;margin-bottom:20px}}
.card{{border:1px solid #E5E7EB;border-radius:8px;padding:12px 14px;background:#fff}}
.card h3{{margin:0 0 8px;font-size:14px;color:#2563EB}}
.card p{{margin:4px 0;font-size:13px}}
table{{width:100%;border-collapse:collapse;margin:12px 0}}
th,td{{border:1px solid #E5E7EB;padding:8px;text-align:left;font-size:13px}}
th{{background:#F8FAFC}}
.right{{text-align:right}}
.total{{font-size:20px;font-weight:700;color:#2563EB;margin-top:12px}}
.hr{{border:none;border-top:2px solid #111827;margin:16px 0}}
@media print{{.noprint{{display:none}}}}
</style></head><body>
<button class='noprint' onclick='window.print()'>In / Lưu PDF</button>
<h1>CHI TIẾT HÓA ĐƠN {Esc(inv.InvoiceCode)}</h1>
<p class='sub'>Trạng thái: <b>{Esc(inv.Status)}</b> · Hạn TT: {(inv.DueDate?.ToString("dd/MM/yyyy") ?? "-")} · Tháng chỉ số: {(inv.ReadingMonth?.ToString("MM/yyyy") ?? "-")}</p>
<div class='grid'>
  <div class='card'><h3>Thông tin hóa đơn</h3>
    <p>Mã HĐ đơn: <b>{Esc(inv.InvoiceCode)}</b></p>
    <p>Mã hợp đồng: <b>{Esc(inv.ContractCode)}</b></p>
    <p>Trạng thái: <b>{Esc(inv.Status)}</b></p>
    <p>Ngày thanh toán: {(inv.PaidDate?.ToString("dd/MM/yyyy") ?? "-")}</p>
  </div>
  <div class='card'><h3>Thông tin khách thuê</h3>
    <p>Họ tên: <b>{Esc(inv.TenantName)}</b></p>
    <p>Điện thoại: {Esc(inv.TenantPhone)}</p>
    <p>Email: {Esc(inv.TenantEmail)}</p>
  </div>
  <div class='card'><h3>Thông tin phòng</h3>
    <p>Phòng: <b>{Esc(inv.RoomNumber)}</b></p>
    <p>Nhà: {Esc(inv.HouseName)}</p>
    <p>Địa chỉ: {Esc(inv.HouseAddress)}</p>
    <p>Diện tích: {(inv.RoomArea?.ToString("0.##") ?? "-")} m²</p>
  </div>
  <div class='card'><h3>Thông tin hợp đồng</h3>
    <p>Thời hạn: {(inv.ContractStartDate?.ToString("dd/MM/yyyy") ?? "-")} → {(inv.ContractEndDate?.ToString("dd/MM/yyyy") ?? "-")}</p>
    <p>Nhận phòng: {(inv.MoveInDate?.ToString("dd/MM/yyyy") ?? "-")}</p>
    <p>Giá điện: {inv.ElectricPrice:N0} đ/số</p>
    <p>Giá nước: {inv.WaterPrice:N0} đ/m³</p>
    <p>Trạng thái HĐ: {Esc(inv.ContractStatus)}</p>
  </div>
</div>
<table>
<tr><th>Khoản mục</th><th class='right'>Chỉ số cũ</th><th class='right'>Chỉ số mới</th><th class='right'>Tiêu thụ</th><th class='right'>Đơn giá</th><th class='right'>Thành tiền</th></tr>
<tr><td>Điện</td><td class='right'>{inv.OldElectric:N0}</td><td class='right'>{inv.NewElectric:N0}</td><td class='right'>{usageE:N0}</td><td class='right'>{inv.ElectricPrice:N0}</td><td class='right'>{inv.ElectricCost:N0}</td></tr>
<tr><td>Nước</td><td class='right'>{inv.OldWater:N0}</td><td class='right'>{inv.NewWater:N0}</td><td class='right'>{usageW:N0}</td><td class='right'>{inv.WaterPrice:N0}</td><td class='right'>{inv.WaterCost:N0}</td></tr>
<tr><td>Phí khác</td><td class='right'>-</td><td class='right'>-</td><td class='right'>-</td><td class='right'>-</td><td class='right'>{inv.OtherFee:N0}</td></tr>
</table>
<hr class='hr'/>
<table>
<tr><td>Tiền phòng{(inv.IsProrated ? $" ({inv.OccupiedDays}/{inv.DaysInMonth} ngày)" : "")}</td><td class='right'><b>{inv.Rent:N0} đ</b></td></tr>
{(inv.IsProrated ? $"<tr><td colspan='2' style='color:#6B7280;font-size:12px'>{Esc(inv.RentNote)}</td></tr>" : "")}
<tr><td>Điện</td><td class='right'>{inv.ElectricCost:N0} đ</td></tr>
<tr><td>Nước</td><td class='right'>{inv.WaterCost:N0} đ</td></tr>
<tr><td>Phí khác</td><td class='right'>{inv.OtherFee:N0} đ</td></tr>
</table>
<p class='total'>TỔNG TIỀN: {inv.Total:N0} đ</p>
<p style='color:#6B7280;font-size:12px'>Xuất từ RPMS — {DateTime.Now:dd/MM/yyyy HH:mm}</p>
</body></html>";
        }

        public static string ExportHtml(InvoiceDetailDto inv, string outputPath)
        {
            File.WriteAllText(outputPath, BuildHtml(inv), new UTF8Encoding(true));
            return outputPath;
        }

        public static void OpenAndPrint(InvoiceDetailDto inv)
        {
            var folder = Path.Combine(Path.GetTempPath(), "RPMS_Invoices");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, $"{inv.InvoiceCode}.html");
            ExportHtml(inv, path);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            AppDialog.ShowInfo("Đã mở hóa đơn. Dùng In / Lưu PDF trên trình duyệt để in hoặc xuất PDF.");
        }

        private static string Esc(string? s) =>
            (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}
