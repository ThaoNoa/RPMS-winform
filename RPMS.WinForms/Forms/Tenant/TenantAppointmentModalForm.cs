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
            this.ClientSize = new Size(500, 400);
            this.BackColor = AppColors.Card;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Đặt lịch xem phòng";

            lblTitle = new Label
            {
                Text = "Đặt lịch hẹn xem phòng",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                Location = new Point(20, 20),
                AutoSize = true
            };
            lblRoomInfo = new Label
            {
                Location = new Point(20, 70),
                Size = new Size(450, 40),
                Font = new Font("Segoe UI", 11F, FontStyle.Italic),
                ForeColor = AppColors.TextMuted
            };
            lblDate = new Label
            {
                Text = "Ngày giờ xem phòng *",
                Location = new Point(20, 120),
                AutoSize = true
            };
            dtpDate = new DateTimePicker
            {
                Location = new Point(20, 145),
                Size = new Size(450, 30),
                Font = new Font("Segoe UI", 11F),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm"
            };
            lblNote = new Label
            {
                Text = "Ghi chú cho chủ nhà",
                Location = new Point(20, 190),
                AutoSize = true
            };
            txtNote = new TextBox
            {
                Multiline = true,
                Location = new Point(20, 215),
                Size = new Size(450, 80),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnSave = new ModernButton
            {
                Text = "Xác nhận đặt lịch",
                Location = new Point(190, 320),
                Size = new Size(130, 40),
                BackColor = AppColors.Primary
            };
            btnSave.Click += BtnSave_Click!;

            btnCancel = new ModernButton
            {
                Text = "Hủy bỏ",
                Location = new Point(340, 320),
                Size = new Size(130, 40),
                BackColor = AppColors.Secondary
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, lblRoomInfo, lblDate, dtpDate, lblNote, txtNote, btnSave, btnCancel });
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