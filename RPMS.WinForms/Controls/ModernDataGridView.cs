using RPMS.Common.Constants;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    public class ModernDataGridView : DataGridView
    {
        public ModernDataGridView()
        {
            this.BackgroundColor = AppColors.Card;
            this.BorderStyle = BorderStyle.None;
            this.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            this.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            this.RowHeadersVisible = false;
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.AllowUserToResizeRows = false;
            this.ReadOnly = true;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.MultiSelect = false;
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.GridColor = AppColors.Border;
            this.EnableHeadersVisualStyles = false;
            this.RowTemplate.Height = 45;

            this.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Background;
            this.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextMain;
            this.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            this.ColumnHeadersDefaultCellStyle.SelectionBackColor = AppColors.Background;
            this.ColumnHeadersDefaultCellStyle.SelectionForeColor = AppColors.TextMain;
            this.ColumnHeadersHeight = 50;
            this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            this.DefaultCellStyle.BackColor = AppColors.Card;
            this.DefaultCellStyle.ForeColor = AppColors.TextMain;
            this.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            this.DefaultCellStyle.SelectionBackColor = ControlPaint.Light(AppColors.Primary, 0.8F);
            this.DefaultCellStyle.SelectionForeColor = AppColors.TextMain;
            this.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);
        }
    }
}