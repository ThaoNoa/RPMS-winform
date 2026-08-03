using RPMS.DTO.Contract;
using RPMS.WinForms.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RPMS.WinForms.UI
{
    public static class ContractPrintHelper
    {
        public static string ExportHtml(ContractDetailDto c, string outputPath)
        {
            var html = $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><title>Hợp đồng {c.ContractCode}</title>
<style>
body{{font-family:'Times New Roman',serif;margin:40px;color:#111;line-height:1.5}}
h1,h2{{text-align:center}}
.meta{{margin:24px 0}}
.sign{{display:flex;justify-content:space-between;margin-top:60px}}
.box{{width:45%;text-align:center}}
table{{width:100%;border-collapse:collapse;margin:16px 0}}
td{{padding:6px;border-bottom:1px solid #ddd;vertical-align:top}}
@media print{{button{{display:none}}}}
</style></head><body>
<button onclick='window.print()'>In / Lưu PDF</button>
<h1>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</h1>
<p style='text-align:center'><b>Độc lập - Tự do - Hạnh phúc</b></p>
<h2>HỢP ĐỒNG THUÊ PHÒNG TRỌ</h2>
<p style='text-align:center'>Mã hợp đồng: <b>{c.ContractCode}</b></p>
<div class='meta'>
<table>
<tr><td>Phòng</td><td><b>{c.RoomNumber}</b></td></tr>
<tr><td>Bên thuê</td><td><b>{c.TenantName}</b></td></tr>
<tr><td>Ngày bắt đầu</td><td>{c.StartDate:dd/MM/yyyy}</td></tr>
<tr><td>Ngày kết thúc</td><td>{c.EndDate:dd/MM/yyyy}</td></tr>
<tr><td>Ngày nhận phòng</td><td>{(c.MoveInDate?.ToString("dd/MM/yyyy") ?? "-")}</td></tr>
<tr><td>Tiền thuê tháng</td><td>{c.MonthlyRent:N0} đ</td></tr>
<tr><td>Tiền cọc</td><td>{c.Deposit:N0} đ</td></tr>
<tr><td>Giá điện</td><td>{c.ElectricPrice:N0} đ/số</td></tr>
<tr><td>Giá nước</td><td>{c.WaterPrice:N0} đ/số</td></tr>
<tr><td>Trạng thái</td><td>{c.Status}</td></tr>
<tr><td>Người tạo</td><td>{c.CreatedByName}</td></tr>
</table>
</div>
<p><b>Điều khoản chung:</b> Bên thuê cam kết thanh toán đúng hạn, giữ gìn tài sản, tuân thủ nội quy nhà trọ.
Bên cho thuê đảm bảo cung cấp điện nước và xử lý sự cố trong phạm vi trách nhiệm.</p>
<div class='sign'>
<div class='box'><b>BÊN CHO THUÊ</b><br/><br/><br/>(Ký, ghi rõ họ tên)</div>
<div class='box'><b>BÊN THUÊ</b><br/><br/><br/>(Ký, ghi rõ họ tên)</div>
</div>
<p style='margin-top:40px;color:#666;font-size:12px'>Xuất từ RPMS — {DateTime.Now:dd/MM/yyyy HH:mm}</p>
</body></html>";
            File.WriteAllText(outputPath, html, Encoding.UTF8);
            return outputPath;
        }

        public static void OpenAndPrint(ContractDetailDto contract)
        {
            var folder = Path.Combine(Path.GetTempPath(), "RPMS_Contracts");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, $"{contract.ContractCode}.html");
            ExportHtml(contract, path);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            AppDialog.ShowInfo("Đã mở hợp đồng. Dùng In / Lưu PDF trên trình duyệt để xuất PDF.");
        }
    }
}
