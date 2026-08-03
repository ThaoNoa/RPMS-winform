using RPMS.DTO.Maintenance;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RPMS.WinForms.UI
{
    public static class MaintenancePrintHelper
    {
        public static string BuildHtml(MaintenanceRequestDto m)
        {
            string statusVi = m.Status switch
            {
                "Pending" => "Chờ xử lý",
                "Processing" => "Đang xử lý",
                "Completed" => "Hoàn thành",
                _ => m.Status
            };
            return $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><title>Phiếu bảo trì #{m.RequestID}</title>
<style>
body{{font-family:'Segoe UI',sans-serif;margin:32px;color:#111827;line-height:1.45}}
h1{{color:#2563EB;text-align:center}}
.sub{{text-align:center;color:#6B7280;margin-bottom:20px}}
.card{{border:1px solid #E5E7EB;border-radius:8px;padding:14px;margin-bottom:14px}}
table{{width:100%;border-collapse:collapse}}
td{{padding:6px 4px;border-bottom:1px solid #F3F4F6;vertical-align:top}}
td:first-child{{width:160px;color:#6B7280}}
.badge{{display:inline-block;padding:4px 10px;border-radius:6px;background:#DBEAFE;color:#1D4ED8;font-weight:600}}
@media print{{.noprint{{display:none}}}}
</style></head><body>
<button class='noprint' onclick='window.print()'>In / Lưu PDF</button>
<h1>PHIẾU YÊU CẦU BẢO TRÌ</h1>
<p class='sub'>Mã phiếu: <b>#{m.RequestID}</b> · <span class='badge'>{Esc(statusVi)}</span></p>
<div class='card'>
<table>
<tr><td>Tiêu đề</td><td><b>{Esc(m.Title)}</b></td></tr>
<tr><td>Phòng / Nhà</td><td>Phòng {Esc(m.RoomNumber)} · {Esc(m.HouseName)}</td></tr>
<tr><td>Địa chỉ</td><td>{Esc(m.HouseAddress)}</td></tr>
<tr><td>Khách thuê</td><td>{Esc(m.TenantName)} {Esc(m.TenantPhone)}</td></tr>
<tr><td>Hợp đồng</td><td>{Esc(m.ContractCode)}</td></tr>
<tr><td>Ngày gửi</td><td>{m.CreatedDate:dd/MM/yyyy HH:mm}</td></tr>
<tr><td>Quản lý phụ trách</td><td>{Esc(string.IsNullOrWhiteSpace(m.AssignedManagerName) ? "Chưa gán" : m.AssignedManagerName)}</td></tr>
<tr><td>Hoàn thành</td><td>{(m.CompletedDate?.ToString("dd/MM/yyyy HH:mm") ?? "—")}</td></tr>
</table>
</div>
<div class='card'>
<h3 style='margin-top:0;color:#2563EB'>Mô tả sự cố</h3>
<p>{Esc(string.IsNullOrWhiteSpace(m.Description) ? "(Không có mô tả)" : m.Description)}</p>
</div>
<p style='color:#6B7280;font-size:12px'>Xuất từ RPMS — {DateTime.Now:dd/MM/yyyy HH:mm}</p>
</body></html>";
        }

        public static string ExportHtml(MaintenanceRequestDto m, string path)
        {
            File.WriteAllText(path, BuildHtml(m), new UTF8Encoding(true));
            return path;
        }

        public static void OpenAndPrint(MaintenanceRequestDto m)
        {
            var folder = Path.Combine(Path.GetTempPath(), "RPMS_Print");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, $"MTN_{m.RequestID}_{DateTime.Now:HHmmss}.html");
            ExportHtml(m, path);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            AppDialog.ShowInfo("Đã mở phiếu bảo trì. Dùng In / Lưu PDF trên trình duyệt (Microsoft Print to PDF).");
        }

        public static void SavePdfHtml(MaintenanceRequestDto m)
        {
            if (!ExportHelper.SaveFile("HTML (*.html)|*.html", $"PhieuBaoTri_{m.RequestID}.html", out var path))
                return;
            ExportHtml(m, path);
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch { /* ignore */ }
            AppDialog.ShowInfo("Đã xuất file. Mở trình duyệt → In → chọn 'Microsoft Print to PDF'.\n" + path);
        }

        private static string Esc(string? s) =>
            (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}
