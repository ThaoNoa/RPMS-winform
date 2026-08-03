using RPMS.Common.Constants;
using RPMS.DTO.Calendar;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RPMS.WinForms.UI
{
    public static class ExportHelper
    {
        public static string ExportCsv(string filePath, string[] headers, System.Collections.Generic.IEnumerable<string[]> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));
            foreach (var row in rows)
                sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
            return filePath;
        }

        public static string ExportHtmlReport(string filePath, string title, string bodyHtml)
        {
            var html = $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><title>{title}</title>
<style>
body{{font-family:'Segoe UI',sans-serif;margin:32px;color:#111827;background:#F8FAFC}}
h1{{color:#2563EB}}
.card{{background:#fff;border:1px solid #E5E7EB;border-radius:8px;padding:16px;margin:12px 0}}
table{{border-collapse:collapse;width:100%}}
th,td{{border:1px solid #E5E7EB;padding:8px;text-align:left}}
th{{background:#F8FAFC}}
</style></head><body>
<h1>{title}</h1>
<p>Xuất lúc: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
{bodyHtml}
</body></html>";
            File.WriteAllText(filePath, html, Encoding.UTF8);
            return filePath;
        }

        public static Color MapEventColor(ColorHint hint) => hint switch
        {
            ColorHint.Success => AppColors.Success,
            ColorHint.Warning => AppColors.Warning,
            ColorHint.Danger => AppColors.Danger,
            _ => AppColors.Primary
        };

        private static string EscapeCsv(string? value)
        {
            value ??= "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        public static bool SaveFile(string filter, string defaultName, out string path)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = filter,
                FileName = defaultName
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                path = sfd.FileName;
                return true;
            }
            path = "";
            return false;
        }

        /// <summary>Mở HTML để in / lưu PDF qua trình duyệt (Print Preview).</summary>
        public static void OpenPrintPreview(string html, string filePrefix)
        {
            var folder = Path.Combine(Path.GetTempPath(), "RPMS_Print");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, $"{filePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            File.WriteAllText(path, html, new UTF8Encoding(true));
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }

        public static void ExportExcelCsv(string defaultName, string[] headers, System.Collections.Generic.IEnumerable<string[]> rows)
        {
            if (!SaveFile("Excel CSV (*.csv)|*.csv", defaultName, out var path))
                return;
            ExportCsv(path, headers, rows);
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch { /* ignore */ }
            AppDialog.ShowInfo("Đã xuất Excel (CSV):\n" + path);
        }
    }
}
