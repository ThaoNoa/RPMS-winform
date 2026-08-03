using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Contract;
using RPMS.DTO.House;
using RPMS.DTO.Room;
using RPMS.DTO.User;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    public class LandlordContractForm : Form
    {
        private readonly IContractService _contractService;
        private readonly IHouseService _houseService;
        private readonly IRoomService _roomService;
        private readonly ILandlordService _landlordService;

        private ModernDataGridView dgv = null!;
        private ComboBox cboHouse = null!;
        private ComboBox cboRoom = null!;
        private ComboBox cboTenant = null!;
        private DateTimePicker dtpStart = null!;
        private DateTimePicker dtpEnd = null!;
        private ModernTextBox txtDeposit = null!;
        private ModernTextBox txtRent = null!;
        private ModernTextBox txtElectric = null!;
        private ModernTextBox txtWater = null!;
        private List<UserDto> _tenants = new();

        public LandlordContractForm(
            IContractService contractService,
            IHouseService houseService,
            IRoomService roomService,
            ILandlordService landlordService)
        {
            _contractService = contractService;
            _houseService = houseService;
            _roomService = roomService;
            _landlordService = landlordService;
            InitializeUI();
            Load += async (s, e) => await OnLoadAsync();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Quản lý hợp đồng";
            ClientSize = new Size(1150, 700);
            AutoScroll = false;

            var pnlCreate = new Panel
            {
                Dock = DockStyle.Right,
                Width = 360,
                MinimumSize = new Size(320, 0),
                BackColor = AppColors.Card,
                Padding = new Padding(16),
                AutoScroll = true
            };

            int y = 16;
            void AddLabel(string text)
            {
                pnlCreate.Controls.Add(new Label
                {
                    Text = text,
                    Location = new Point(16, y),
                    AutoSize = true,
                    ForeColor = AppColors.TextMuted
                });
                y += 22;
            }

            pnlCreate.Controls.Add(new Label
            {
                Text = "Tạo hợp đồng mới",
                Font = AppTypography.Heading,
                ForeColor = AppColors.TextMain,
                Location = new Point(16, y),
                AutoSize = true
            });
            y += 40;

            AddLabel("Nhà");
            cboHouse = new ComboBox { Location = new Point(16, y), Size = new Size(320, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cboHouse.SelectedIndexChanged += async (s, e) =>
            {
                await LoadRoomsAsync();
                await LoadAppointmentTenantsAsync();
            };
            pnlCreate.Controls.Add(cboHouse);
            y += 40;

            AddLabel("Phòng trống");
            cboRoom = new ComboBox { Location = new Point(16, y), Size = new Size(320, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRoom.SelectedIndexChanged += async (s, e) =>
            {
                CboRoom_SelectedIndexChanged(s, e);
                await LoadAppointmentTenantsAsync();
            };
            pnlCreate.Controls.Add(cboRoom);
            y += 40;

            AddLabel("Khách đã đặt lịch xem (để trống nếu chưa có)");
            cboTenant = new ComboBox { Location = new Point(16, y), Size = new Size(320, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlCreate.Controls.Add(cboTenant);
            y += 40;

            AddLabel("Ngày bắt đầu");
            dtpStart = new DateTimePicker { Location = new Point(16, y), Size = new Size(320, 28), Format = DateTimePickerFormat.Short };
            pnlCreate.Controls.Add(dtpStart);
            y += 40;

            AddLabel("Ngày kết thúc");
            dtpEnd = new DateTimePicker { Location = new Point(16, y), Size = new Size(320, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(6) };
            pnlCreate.Controls.Add(dtpEnd);
            y += 40;

            AddLabel("Tiền cọc");
            txtDeposit = new ModernTextBox { Location = new Point(16, y), Size = new Size(320, 32), Text = "0" };
            pnlCreate.Controls.Add(txtDeposit);
            y += 45;

            AddLabel("Tiền thuê / tháng");
            txtRent = new ModernTextBox { Location = new Point(16, y), Size = new Size(320, 32) };
            pnlCreate.Controls.Add(txtRent);
            y += 45;

            AddLabel("Giá điện / số");
            txtElectric = new ModernTextBox { Location = new Point(16, y), Size = new Size(320, 32), Text = "3500" };
            pnlCreate.Controls.Add(txtElectric);
            y += 45;

            AddLabel("Giá nước / số");
            txtWater = new ModernTextBox { Location = new Point(16, y), Size = new Size(320, 32), Text = "20000" };
            pnlCreate.Controls.Add(txtWater);
            y += 50;

            var btnCreate = new ModernButton
            {
                Text = "Lưu hợp đồng",
                Location = new Point(16, y),
                Size = new Size(320, 40),
                BackColor = AppColors.Primary
            };
            btnCreate.Click += async (s, e) => await CreateContractAsync();
            pnlCreate.Controls.Add(btnCreate);
            foreach (Control c in pnlCreate.Controls)
            {
                if (c is ComboBox or ModernTextBox or DateTimePicker or ModernButton)
                    c.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = AppColors.Card };
            pnlTop.Controls.Add(new Label
            {
                Text = "Danh sách hợp đồng",
                Font = AppTypography.Heading,
                Location = new Point(20, 16),
                AutoSize = true,
                ForeColor = AppColors.TextMain
            });

            dgv = new ModernDataGridView { Dock = DockStyle.Fill };
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "Mã HĐ", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", Width = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenantName", HeaderText = "Khách thuê", Width = 160 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StartDate",
                HeaderText = "Bắt đầu",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EndDate",
                HeaderText = "Kết thúc",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MonthlyRent",
                HeaderText = "Tiền thuê",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TT", Width = 80 });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "AssignCol",
                HeaderText = "",
                Text = "Gán khách",
                UseColumnTextForLinkValue = true,
                Width = 80,
                LinkColor = AppColors.Primary
            });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "PrintCol",
                HeaderText = "",
                Text = "In/PDF",
                UseColumnTextForLinkValue = true,
                Width = 70,
                LinkColor = AppColors.Primary
            });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "ExtendCol",
                HeaderText = "",
                Text = "Gia hạn",
                UseColumnTextForLinkValue = true,
                Width = 70,
                LinkColor = AppColors.Success
            });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "TerminateCol",
                HeaderText = "",
                Text = "Hủy HĐ",
                UseColumnTextForLinkValue = true,
                Width = 70,
                LinkColor = AppColors.Danger
            });
            dgv.CellContentClick += async (s, e) => await Dgv_CellContentClick(e);

            Controls.Add(dgv);
            Controls.Add(pnlCreate);
            Controls.Add(pnlTop);
            UIHelper.WireListPage(this, pnlTop, dgv);
        }

        private async Task OnLoadAsync()
        {
            try
            {
                var landlordId = UserSession.CurrentUser!.UserID;
                var houses = (await _houseService.GetHousesByOwnerAsync(landlordId)).ToList();
                cboHouse.DataSource = houses;
                cboHouse.DisplayMember = nameof(HouseDto.HouseName);
                cboHouse.ValueMember = nameof(HouseDto.HouseID);

                await LoadRoomsAsync();
                await LoadAppointmentTenantsAsync();
                await LoadContractsAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async Task LoadAppointmentTenantsAsync()
        {
            int? roomId = null;
            if (cboRoom.SelectedValue != null &&
                int.TryParse(cboRoom.SelectedValue.ToString(), out int rid) &&
                rid > 0)
            {
                roomId = rid;
            }

            _tenants = (await _landlordService.GetAppointmentTenantsAsync(
                UserSession.CurrentUser!.UserID, roomId)).ToList();

            // Nếu phòng chưa có ai đặt lịch, vẫn hiện khách đã đặt lịch các phòng khác của chủ
            if (roomId.HasValue && _tenants.Count == 0)
            {
                _tenants = (await _landlordService.GetAppointmentTenantsAsync(
                    UserSession.CurrentUser!.UserID, null)).ToList();
            }

            BindTenantCombo(cboTenant, includeEmpty: true);
        }

        private void BindTenantCombo(ComboBox cbo, bool includeEmpty)
        {
            var items = new List<UserDto>();
            if (includeEmpty)
                items.Add(new UserDto { UserID = 0, FullName = "(Chưa có khách — lưu nháp)" });
            foreach (var t in _tenants)
            {
                var label = string.IsNullOrWhiteSpace(t.Phone)
                    ? t.FullName
                    : $"{t.FullName} ({t.Phone})";
                items.Add(new UserDto
                {
                    UserID = t.UserID,
                    FullName = label,
                    Phone = t.Phone,
                    Email = t.Email,
                    Username = t.Username,
                    Status = t.Status,
                    RoleID = t.RoleID
                });
            }
            cbo.DataSource = null;
            cbo.DataSource = items;
            cbo.DisplayMember = nameof(UserDto.FullName);
            cbo.ValueMember = nameof(UserDto.UserID);
            if (cbo.Items.Count > 0)
                cbo.SelectedIndex = 0;
        }

        private async Task LoadRoomsAsync()
        {
            cboRoom.DataSource = null;
            if (cboHouse.SelectedValue == null || !int.TryParse(cboHouse.SelectedValue.ToString(), out int houseId))
                return;

            var rooms = (await _roomService.GetRoomsByHouseAsync(houseId))
                .Where(r => r.Status == "Available")
                .ToList();
            cboRoom.DataSource = rooms;
            cboRoom.DisplayMember = nameof(RoomDto.RoomNumber);
            cboRoom.ValueMember = nameof(RoomDto.RoomID);
        }

        private void CboRoom_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboRoom.SelectedItem is RoomDto room)
                txtRent.Text = room.Price.ToString("0");
        }

        private async Task LoadContractsAsync()
        {
            var list = await _contractService.GetContractsByLandlordAsync(UserSession.CurrentUser!.UserID);
            dgv.DataSource = list.OrderByDescending(c => c.ContractID).ToList();
        }

        private async Task CreateContractAsync()
        {
            if (cboRoom.SelectedValue == null)
            {
                AppDialog.ShowWarning("Vui lòng chọn phòng.");
                return;
            }

            if (!decimal.TryParse(txtDeposit.Text, out decimal deposit) ||
                !decimal.TryParse(txtRent.Text, out decimal rent) ||
                !decimal.TryParse(txtElectric.Text, out decimal electric) ||
                !decimal.TryParse(txtWater.Text, out decimal water) ||
                rent <= 0)
            {
                AppDialog.ShowWarning("Vui lòng nhập số tiền hợp lệ.");
                return;
            }

            int? tenantId = null;
            if (cboTenant.SelectedValue != null &&
                int.TryParse(cboTenant.SelectedValue.ToString(), out int tid) &&
                tid > 0)
            {
                tenantId = tid;
            }

            try
            {
                var created = await _contractService.CreateContractAsync(new CreateContractDto
                {
                    RoomID = Convert.ToInt32(cboRoom.SelectedValue),
                    TenantID = tenantId,
                    StartDate = dtpStart.Value.Date,
                    EndDate = dtpEnd.Value.Date,
                    Deposit = deposit,
                    MonthlyRent = rent,
                    ElectricPrice = electric,
                    WaterPrice = water
                }, UserSession.CurrentUser!.UserID);

                if (tenantId.HasValue)
                    AppDialog.ShowInfo("Tạo hợp đồng thành công (đã gắn khách thuê).");
                else
                    AppDialog.ShowInfo("Đã lưu hợp đồng nháp. Khi có khách, bấm \"Gán khách\" trên danh sách.");

                ToastNotifier.Show(this,
                    created.Status == "Draft" ? "Đã lưu hợp đồng nháp" : "Đã tạo hợp đồng Active",
                    ToastKind.Success);

                await LoadRoomsAsync();
                await LoadContractsAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async Task Dgv_CellContentClick(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var contract = dgv.Rows[e.RowIndex].DataBoundItem as ContractDto;
            if (contract == null) return;
            string col = dgv.Columns[e.ColumnIndex].Name;

            try
            {
                if (col == "PrintCol")
                {
                    var detail = await _contractService.GetContractByIdAsync(contract.ContractID);
                    ContractPrintHelper.OpenAndPrint(detail);
                    return;
                }

                if (col == "AssignCol")
                {
                    await AssignTenantAsync(contract);
                    return;
                }

                if (col == "ExtendCol")
                {
                    if (contract.Status != "Active" || !contract.TenantID.HasValue)
                    {
                        AppDialog.ShowWarning("Chỉ gia hạn hợp đồng Active đã có khách thuê.");
                        return;
                    }
                    var monthsText = AppDialog.Prompt("Gia hạn thêm bao nhiêu tháng?", "Gia hạn hợp đồng", "6");
                    if (string.IsNullOrWhiteSpace(monthsText) || !int.TryParse(monthsText, out int months) || months <= 0)
                    {
                        AppDialog.ShowWarning("Số tháng không hợp lệ.");
                        return;
                    }
                    var newEnd = contract.EndDate.AddMonths(months);
                    await _contractService.ExtendContractAsync(contract.ContractID, newEnd, UserSession.CurrentUser!.UserID);
                    AppDialog.ShowInfo($"Đã gia hạn đến {newEnd:dd/MM/yyyy}.");
                    await LoadContractsAsync();
                    return;
                }

                if (col == "TerminateCol")
                {
                    if (contract.Status != "Active" && contract.Status != "Draft")
                    {
                        AppDialog.ShowWarning("Chỉ hủy hợp đồng nháp hoặc Active.");
                        return;
                    }
                    if (!AppDialog.Confirm($"Hủy hợp đồng {contract.ContractCode}?"))
                        return;
                    await _contractService.TerminateContractAsync(contract.ContractID);
                    AppDialog.ShowInfo("Đã hủy hợp đồng.");
                    await LoadRoomsAsync();
                    await LoadContractsAsync();
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async Task AssignTenantAsync(ContractDto contract)
        {
            if (contract.TenantID.HasValue)
            {
                AppDialog.ShowInfo("Hợp đồng này đã có khách thuê.");
                return;
            }
            if (contract.Status != "Draft" && contract.Status != "Active")
            {
                AppDialog.ShowWarning("Không thể gán khách cho hợp đồng đã kết thúc.");
                return;
            }
            if (_tenants.Count == 0)
            {
                AppDialog.ShowWarning("Chưa có khách đặt lịch xem phòng liên quan. Khách cần đặt lịch trước khi được gán vào hợp đồng.");
                return;
            }

            using var dlg = new Form
            {
                Text = $"Gán khách — {contract.ContractCode}",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(420, 160),
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = AppColors.Background
            };
            var lbl = new Label
            {
                Text = $"Phòng {contract.RoomNumber} — khách đã đặt lịch xem:",
                Location = new Point(16, 16),
                AutoSize = true,
                ForeColor = AppColors.TextMain
            };
            var cbo = new ComboBox
            {
                Location = new Point(16, 48),
                Size = new Size(380, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // Khách đã đặt lịch xem phòng thuộc nhà của landlord
            var candidates = (await _landlordService.GetAppointmentTenantsAsync(
                UserSession.CurrentUser!.UserID, null)).ToList();
            if (candidates.Count == 0)
            {
                AppDialog.ShowWarning("Chưa có khách đặt lịch xem phòng của bạn.");
                return;
            }
            cbo.DataSource = candidates.Select(t => new UserDto
            {
                UserID = t.UserID,
                FullName = string.IsNullOrWhiteSpace(t.Phone) ? t.FullName : $"{t.FullName} ({t.Phone})",
                Phone = t.Phone
            }).ToList();
            cbo.DisplayMember = nameof(UserDto.FullName);
            cbo.ValueMember = nameof(UserDto.UserID);

            var btnOk = new ModernButton
            {
                Text = "Gán khách",
                Size = new Size(120, 36),
                Location = new Point(160, 100),
                BackColor = AppColors.Primary,
                DialogResult = DialogResult.OK
            };
            var btnCancel = new ModernButton
            {
                Text = "Hủy",
                Size = new Size(90, 36),
                Location = new Point(290, 100),
                BackColor = AppColors.Border,
                ForeColor = AppColors.TextMain,
                DialogResult = DialogResult.Cancel
            };
            dlg.Controls.AddRange(new Control[] { lbl, cbo, btnOk, btnCancel });
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            if (cbo.SelectedValue == null || !int.TryParse(cbo.SelectedValue.ToString(), out int tenantId) || tenantId <= 0)
            {
                AppDialog.ShowWarning("Vui lòng chọn khách thuê.");
                return;
            }

            await _contractService.AssignTenantAsync(new AssignTenantDto
            {
                ContractID = contract.ContractID,
                TenantID = tenantId
            }, UserSession.CurrentUser!.UserID);

            ToastNotifier.Show(this, "Đã gán khách thuê", ToastKind.Success);
            AppDialog.ShowInfo("Đã gán khách thuê. Hợp đồng chuyển sang Active, phòng đánh dấu đã thuê.");
            await LoadRoomsAsync();
            await LoadContractsAsync();
        }
    }
}
