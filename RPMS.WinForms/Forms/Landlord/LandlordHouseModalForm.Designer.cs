using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    partial class LandlordHouseModalForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private ModernTextBox txtName, txtAddress;
        private TextBox txtDescription;
        private ComboBox cboStatus;
        private ModernButton btnSave, btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            txtName = new ModernTextBox();
            txtAddress = new ModernTextBox();
            txtDescription = new TextBox();
            cboStatus = new ComboBox();
            btnSave = UIHelper.PrimaryButton("Lưu thông tin", 130);
            btnCancel = UIHelper.SecondaryButton("Hủy bỏ");

            SuspendLayout();

            lblTitle.Text = "Thông tin Nhà";
            lblTitle.Font = AppTypography.Heading;
            lblTitle.ForeColor = AppColors.TextMain;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Padding = new Padding(AppLayout.PagePadding, 0, 0, 0);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = AppLayout.PageHeaderHeight,
                BackColor = AppColors.Card,
                Padding = new Padding(0, 8, 0, 8)
            };
            header.Controls.Add(lblTitle);

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(AppLayout.PagePadding),
                BackColor = AppColors.Card
            };

            var stack = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Padding = new Padding(4),
                GrowStyle = TableLayoutPanelGrowStyle.AddRows
            };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            int row = 0;
            void AddField(Control field, int height = 62)
            {
                stack.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
                stack.Controls.Add(field, 0, row);
                row++;
            }

            AddField(UIHelper.CreateDialogField("Tên nhà *", txtName));
            AddField(UIHelper.CreateDialogField("Địa chỉ *", txtAddress));

            txtDescription.Multiline = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Font = AppTypography.Body;
            txtDescription.BorderStyle = BorderStyle.FixedSingle;
            txtDescription.AcceptsReturn = true;
            AddField(UIHelper.CreateDialogField("Mô tả", txtDescription, 130), 130);

            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            AddField(UIHelper.CreateDialogField("Trạng thái *", cboStatus));

            // Chiều rộng tối thiểu để CreateDialogField Resize đúng khi hiện form
            stack.MinimumSize = new Size(420, 0);
            body.Controls.Add(stack);

            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;
            var footer = UIHelper.CreateDialogFooter(btnSave, btnCancel);

            ClientSize = new Size(560, 520);
            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);
            UIHelper.ApplyResizableDialog(this, new Size(480, 440));
            Text = "Quản lý Nhà";
            AutoScroll = false;
            StartPosition = FormStartPosition.CenterParent;

            ResumeLayout(false);
        }
    }
}
