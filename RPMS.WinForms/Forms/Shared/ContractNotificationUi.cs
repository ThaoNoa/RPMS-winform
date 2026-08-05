using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Contract;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Shared
{
    /// <summary>Xem chi tiết hợp đồng (click từ lưới HĐ).</summary>
    public class ContractDetailViewForm : Form
    {
        public ContractDetailViewForm(ContractDetailDto detail)
        {
            Text = $"Hợp đồng {detail.ContractCode}";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 520);
            BackColor = AppColors.Background;

            var body = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                BackColor = AppColors.Card,
                Font = AppTypography.Body,
                Dock = DockStyle.Fill,
                Text = BuildText(detail)
            };

            var btnPrint = UIHelper.PrimaryButton("In / PDF", 120);
            btnPrint.Click += (s, e) =>
            {
                ContractPrintHelper.OpenAndPrint(detail);
            };
            var btnClose = UIHelper.SecondaryButton("Đóng", 100);
            btnClose.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            var bottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = AppColors.Card
            };
            bottom.Controls.Add(btnClose);
            bottom.Controls.Add(btnPrint);

            var pad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = AppColors.Card };
            pad.Controls.Add(body);

            Controls.Add(pad);
            Controls.Add(bottom);
        }

        private static string BuildText(ContractDetailDto d)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Mã HĐ: {d.ContractCode}");
            sb.AppendLine($"Trạng thái: {d.Status}");
            sb.AppendLine($"Nhà: {d.HouseName}");
            sb.AppendLine($"Phòng: {d.RoomNumber}");
            sb.AppendLine($"Khách: {d.TenantName}");
            sb.AppendLine($"Từ ngày: {d.StartDate:dd/MM/yyyy}");
            sb.AppendLine($"Đến ngày: {d.EndDate:dd/MM/yyyy}");
            if (d.MoveInDate.HasValue) sb.AppendLine($"Nhận phòng: {d.MoveInDate:dd/MM/yyyy}");
            if (d.MoveOutDate.HasValue) sb.AppendLine($"Trả phòng: {d.MoveOutDate:dd/MM/yyyy}");
            sb.AppendLine($"Tiền thuê: {d.MonthlyRent:N0} đ/tháng");
            sb.AppendLine($"Tiền cọc: {d.Deposit:N0} đ");
            sb.AppendLine($"Giá điện: {d.ElectricPrice:N0}");
            sb.AppendLine($"Giá nước: {d.WaterPrice:N0}");
            if (!string.IsNullOrWhiteSpace(d.PendingEditStatus))
                sb.AppendLine($"\nĐề xuất sửa: {d.PendingEditStatus}");
            if (!string.IsNullOrWhiteSpace(d.CancelRequestLabel) || !string.IsNullOrWhiteSpace(d.CancelRequestStatus))
                sb.AppendLine($"Xin hủy: {d.CancelRequestLabel}{(!string.IsNullOrWhiteSpace(d.CancelRequestNote) ? " — " + d.CancelRequestNote : "")}");
            return sb.ToString();
        }
    }

    /// <summary>Chi tiết thông báo + Duyệt/Từ chối cho sửa hoặc hủy HĐ.</summary>
    public class NotificationActionForm : Form
    {
        private readonly IContractService _contracts;
        private readonly NotificationDtoWrap _item;
        private readonly Label _lblBody = null!;

        public bool Changed { get; private set; }

        public NotificationActionForm(IContractService contracts, NotificationDtoWrap item, string detailExtra)
        {
            _contracts = contracts;
            _item = item;

            Text = "Chi tiết thông báo";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(560, 480);
            BackColor = AppColors.Background;

            _lblBody = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = AppTypography.Body,
                ForeColor = AppColors.TextMain,
                Padding = new Padding(4)
            };
            _lblBody.Text = $"{item.Title}\n\n{item.Content}\n\n{item.CreatedDate:dd/MM/yyyy HH:mm}\n\n{detailExtra}";

            var pad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = AppColors.Card };
            pad.Controls.Add(_lblBody);

            var bottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12, 10, 12, 10),
                BackColor = AppColors.Card
            };

            var btnClose = UIHelper.SecondaryButton("Đóng", 100);
            btnClose.Click += (s, e) => Close();
            bottom.Controls.Add(btnClose);

            if (item.CanAct)
            {
                var btnReject = UIHelper.SecondaryButton("Từ chối", 110);
                btnReject.ForeColor = AppColors.Danger;
                btnReject.Click += async (s, e) => await ActAsync(approve: false);
                bottom.Controls.Add(btnReject);

                var btnApprove = UIHelper.PrimaryButton(
                    item.ActionType == NotificationActions.ContractCancel ? "Duyệt hủy" : "Xác nhận", 120);
                btnApprove.BackColor = AppColors.Success;
                btnApprove.Click += async (s, e) => await ActAsync(approve: true);
                bottom.Controls.Add(btnApprove);
            }
            else if (!string.IsNullOrWhiteSpace(item.ActionStatus)
                     && !string.Equals(item.ActionStatus, NotificationActions.Pending, StringComparison.OrdinalIgnoreCase))
            {
                var lbl = new Label
                {
                    AutoSize = true,
                    Text = $"Trạng thái: {item.ActionStatus}",
                    ForeColor = AppColors.TextMuted,
                    Margin = new Padding(8, 12, 8, 0)
                };
                bottom.Controls.Add(lbl);
            }

            Controls.Add(pad);
            Controls.Add(bottom);
        }

        private async Task ActAsync(bool approve)
        {
            try
            {
                int uid = UserSession.CurrentUser!.UserID;
                int contractId = _item.RelatedID!.Value;

                if (_item.ActionType == NotificationActions.ContractEdit)
                {
                    if (approve)
                    {
                        if (!AppDialog.Confirm("Xác nhận áp dụng thay đổi hợp đồng?"))
                            return;
                        await _contracts.ConfirmContractEditAsync(contractId, uid);
                        AppDialog.ShowInfo("Đã xác nhận sửa hợp đồng. Hai bên sẽ thấy HĐ cập nhật.");
                    }
                    else
                    {
                        if (!AppDialog.Confirm("Từ chối đề xuất sửa hợp đồng?"))
                            return;
                        await _contracts.RejectContractEditAsync(contractId, uid);
                        AppDialog.ShowInfo("Đã từ chối đề xuất sửa.");
                    }
                }
                else if (_item.ActionType == NotificationActions.ContractCancel)
                {
                    if (approve)
                    {
                        if (!AppDialog.Confirm("Duyệt hủy thuê? Hợp đồng sẽ Terminated, phòng trống lại."))
                            return;
                        await _contracts.ApproveCancelRequestAsync(contractId, uid);
                        AppDialog.ShowInfo("Đã duyệt hủy. Hợp đồng kết thúc — đồng bộ trên HĐ của chủ và khách.");
                    }
                    else
                    {
                        var note = AppDialog.Prompt("Ghi chú từ chối (tuỳ chọn):", "Từ chối hủy", "");
                        if (!AppDialog.Confirm("Từ chối yêu cầu hủy? Hợp đồng vẫn Active."))
                            return;
                        await _contracts.RejectCancelRequestAsync(contractId, uid, note);
                        AppDialog.ShowInfo("Đã từ chối hủy. Hợp đồng vẫn hiệu lực.");
                    }
                }
                else
                {
                    AppDialog.ShowInfo("Thông báo này không có hành động.");
                    return;
                }

                Changed = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }
    }

    /// <summary>DTO nhẹ để form không phụ thuộc assembly DTO mapping CanAct.</summary>
    public sealed class NotificationDtoWrap
    {
        public int NotificationID { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public string? ActionType { get; set; }
        public int? RelatedID { get; set; }
        public string? ActionStatus { get; set; }
        public bool CanAct { get; set; }
    }
}
