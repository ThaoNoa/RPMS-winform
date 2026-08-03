using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Tenant;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Tenant
{
    public class TenantAppointmentModalForm : Form
    {
        private readonly ITenantInteractionService _interactionService;
        public int RoomIdToBook { get; set; }
        public string RoomInfo { get; set; } = "";

        private Label lblTitle = null!;
        private Label lblRoomInfo = null!;
        private Label lblDate = null!;
        private Label lblNote = null!;
        private DateTimePicker dtpDate = null!;
        private TextBox txtNote = null!;
        private ModernButton btnSave = null!;
        private ModernButton btnCancel = null!;

        public TenantAppointmentModalForm(ITenantInteractionService interactionService)
        {
            _interactionService = interactionService;
            InitializeUI();
            this.Load += TenantAppointmentModalForm_Load!;
        }

        private void InitializeUI()
        {
            UIHelper.ApplyResizableDialog(this, new Size(480, 400));
            this.ClientSize = new Size(500, 400);
            this.BackColor = AppColors.Card;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Đặt lịch xem phòng";
            this.AutoScroll = false;

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
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            btnCancel = new ModernButton
            {
                Text = "Hủy bỏ",
                Size = new Size(130, 40),
                BackColor = AppColors.Secondary,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnCancel.Click += (s, e) => this.Close();

            btnSave = new ModernButton
            {
                Text = "Xác nhận đặt lịch",
                Size = new Size(130, 40),
                BackColor = AppColors.Primary,
                Margin = new Padding(0, 0, 0, 0)
            };
            btnSave.Click += BtnSave_Click!;

            flpButtons.Controls.Add(btnCancel);
            flpButtons.Controls.Add(btnSave);
            pnlBottom.Controls.Add(flpButtons);

            var root = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = AppColors.Card
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1
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

            lblTitle = new Label
            {
                Text = "Đặt lịch hẹn xem phòng",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                AutoSize = true,
                MaximumSize = new Size(440, 0),
                Margin = new Padding(0, 0, 0, 8)
            };
            AddRow(lblTitle);

            lblRoomInfo = new Label
            {
                Font = new Font("Segoe UI", 11F, FontStyle.Italic),
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                MaximumSize = new Size(440, 0),
                Margin = new Padding(0, 0, 0, 12)
            };
            AddRow(lblRoomInfo);

            lblDate = new Label
            {
                Text = "Ngày giờ xem phòng *",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };
            AddRow(lblDate);

            dtpDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 11F),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm",
                Height = 30
            };
            AddRow(dtpDate, 30, false);

            lblNote = new Label
            {
                Text = "Ghi chú cho chủ nhà",
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 4)
            };
            AddRow(lblNote);

            txtNote = new TextBox
            {
                Multiline = true,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical
            };
            AddRow(txtNote, 80, false);

            root.Controls.Add(tbl);
            this.Controls.Add(root);
            this.Controls.Add(pnlBottom);
        }

        private void TenantAppointmentModalForm_Load(object sender, EventArgs e)
        {
            lblRoomInfo.Text = RoomInfo;
            dtpDate.MinDate = DateTime.Now;
            dtpDate.Value = DateTime.Now.AddHours(1);
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            btnSave.Enabled = false;
            try
            {
                var request = new CreateAppointmentDto
                {
                    RoomID = RoomIdToBook,
                    TenantID = UserSession.CurrentUser!.UserID,
                    AppointmentDate = dtpDate.Value,
                    Note = txtNote.Text.Trim()
                };
                await _interactionService.BookAppointmentAsync(request);
                AppDialog.ShowInfo("Đặt lịch thành công! Vui lòng chờ chủ nhà liên hệ lại.");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (BadRequestException ex)
            {
                AppDialog.ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi: " + ex.Message);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }
    }
}
