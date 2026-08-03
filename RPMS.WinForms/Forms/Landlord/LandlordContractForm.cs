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
        private readonly IUserService _userService;

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

        public LandlordContractForm(
            IContractService contractService,
            IHouseService houseService,
            IRoomService roomService,
            IUserService userService)
        {
            _contractService = contractService;
            _houseService = houseService;
            _roomService = roomService;
            _userService = userService;
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
            cboHouse.SelectedIndexChanged += async (s, e) => await LoadRoomsAsync();
            pnlCreate.Controls.Add(cboHouse);
            y += 40;

            AddLabel("Phòng trống");
            cboRoom = new ComboBox { Location = new Point(16, y), Size = new Size(320, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRoom.SelectedIndexChanged += CboRoom_SelectedIndexChanged;
            pnlCreate.Controls.Add(cboRoom);
            y += 40;

            AddLabel("Khách thuê");
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
                Text = "Tạo hợp đồng",
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
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TT", Width = 90 });
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

                // RoleID 3 = Tenant (theo MainForm)
                var tenants = (await _userService.GetUsersByRoleAsync(3))
                    .Where(u => u.Status == "Active")
                    .ToList();
                cboTenant.DataSource = tenants;
                cboTenant.DisplayMember = nameof(UserDto.FullName);
                cboTenant.ValueMember = nameof(UserDto.UserID);

                await LoadRoomsAsync();
                await LoadContractsAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
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
            if (cboRoom.SelectedValue == null || cboTenant.SelectedValue == null)
            {
                AppDialog.ShowWarning("Vui lòng chọn phòng và khách thuê.");
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

            try
            {
                await _contractService.CreateContractAsync(new CreateContractDto
                {
                    RoomID = Convert.ToInt32(cboRoom.SelectedValue),
                    TenantID = Convert.ToInt32(cboTenant.SelectedValue),
                    StartDate = dtpStart.Value.Date,
                    EndDate = dtpEnd.Value.Date,
                    Deposit = deposit,
                    MonthlyRent = rent,
                    ElectricPrice = electric,
                    WaterPrice = water
                }, UserSession.CurrentUser!.UserID);

                AppDialog.ShowInfo("Tạo hợp đồng thành công.");
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

                if (contract.Status != "Active")
                {
                    AppDialog.ShowWarning("Chỉ thao tác gia hạn/hủy với hợp đồng Active.");
                    return;
                }

                if (col == "ExtendCol")
                {
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
                }
                else if (col == "TerminateCol")
                {
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
    }
}
