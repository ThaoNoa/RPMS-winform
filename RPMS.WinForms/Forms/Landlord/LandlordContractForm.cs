using Microsoft.Extensions.DependencyInjection;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    public class LandlordContractForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private ModernDataGridView dgv = null!;
        private Panel pnlCreate = null!;
        private FlowLayoutPanel createStack = null!;
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
        private bool _suppressComboEvents;
        private readonly SemaphoreSlim _uiLoadLock = new(1, 1);

        public LandlordContractForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            InitializeUI();
            Load += async (s, e) => await OnLoadAsync();
        }

        private async Task<T> WithServicesAsync<T>(Func<IHouseService, IRoomService, IContractService, ILandlordService, Task<T>> action)
        {
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            return await action(
                sp.GetRequiredService<IHouseService>(),
                sp.GetRequiredService<IRoomService>(),
                sp.GetRequiredService<IContractService>(),
                sp.GetRequiredService<ILandlordService>());
        }

        private async Task WithServicesAsync(Func<IHouseService, IRoomService, IContractService, ILandlordService, Task> action)
        {
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            await action(
                sp.GetRequiredService<IHouseService>(),
                sp.GetRequiredService<IRoomService>(),
                sp.GetRequiredService<IContractService>(),
                sp.GetRequiredService<ILandlordService>());
        }

        private void InitializeUI()
        {
            ClientSize = new Size(1150, 700);
            Text = "Quản lý hợp đồng";

            var header = UIHelper.CreatePageHeader("Danh sách hợp đồng");

            pnlCreate = UIHelper.CreateSideFormPanel();
            int fieldW = Math.Max(220, AppLayout.SidePanelWidth - AppLayout.PagePadding * 2 - 24);

            createStack = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            createStack.Controls.Add(new Label
            {
                Text = "Tạo hợp đồng mới",
                Font = AppTypography.Heading,
                ForeColor = AppColors.TextMain,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, AppLayout.FieldGap)
            });

            cboHouse = new ComboBox();
            UIHelper.StyleCombo(cboHouse);
            cboHouse.SelectedIndexChanged += async (s, e) =>
            {
                if (_suppressComboEvents) return;
                await OnHouseChangedAsync();
            };
            createStack.Controls.Add(UIHelper.CreateLabeledField("Nhà", cboHouse, fieldW));

            cboRoom = new ComboBox();
            UIHelper.StyleCombo(cboRoom);
            cboRoom.SelectedIndexChanged += async (s, e) =>
            {
                if (_suppressComboEvents) return;
                UpdateRentFromSelectedRoom();
                await OnRoomChangedAsync();
            };
            createStack.Controls.Add(UIHelper.CreateLabeledField("Phòng trống", cboRoom, fieldW));

            cboTenant = new ComboBox();
            UIHelper.StyleCombo(cboTenant);
            createStack.Controls.Add(UIHelper.CreateLabeledField("Khách đã đặt lịch xem (để trống nếu chưa có)", cboTenant, fieldW));

            dtpStart = new DateTimePicker { Format = DateTimePickerFormat.Short, Height = AppLayout.ComboHeight };
            createStack.Controls.Add(UIHelper.CreateLabeledField("Ngày bắt đầu", dtpStart, fieldW));

            dtpEnd = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddMonths(6),
                Height = AppLayout.ComboHeight
            };
            createStack.Controls.Add(UIHelper.CreateLabeledField("Ngày kết thúc", dtpEnd, fieldW));

            txtDeposit = new ModernTextBox { Text = "0" };
            createStack.Controls.Add(UIHelper.CreateLabeledField("Tiền cọc", txtDeposit, fieldW));

            txtRent = new ModernTextBox();
            createStack.Controls.Add(UIHelper.CreateLabeledField("Tiền thuê / tháng", txtRent, fieldW));

            txtElectric = new ModernTextBox { Text = "3500" };
            createStack.Controls.Add(UIHelper.CreateLabeledField("Giá điện / số", txtElectric, fieldW));

            txtWater = new ModernTextBox { Text = "20000" };
            createStack.Controls.Add(UIHelper.CreateLabeledField("Giá nước / số", txtWater, fieldW));

            var btnCreate = UIHelper.PrimaryButton("Lưu hợp đồng", fieldW);
            btnCreate.Margin = new Padding(0, AppLayout.FieldGap, 0, 8);
            btnCreate.Click += async (s, e) => await CreateContractAsync();
            createStack.Controls.Add(btnCreate);

            var btnBulk = UIHelper.SecondaryButton("Tạo nháp tất cả phòng trống", fieldW);
            btnBulk.Margin = new Padding(0, 0, 0, 6);
            btnBulk.Click += async (s, e) => await CreateDraftsForAllRoomsAsync();
            createStack.Controls.Add(btnBulk);

            createStack.Controls.Add(new Label
            {
                Text = "Tạo HĐ nháp (chưa khách) cho mọi phòng chưa có hợp đồng Active/Draft của nhà đang chọn.",
                Font = AppTypography.Caption,
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                MaximumSize = new Size(fieldW, 0),
                Margin = new Padding(0, 0, 0, 12)
            });

            pnlCreate.Controls.Add(createStack);
            pnlCreate.Resize += (_, _) => SyncCreateFieldWidths();

            dgv = new ModernDataGridView();
            UIHelper.ApplyGridFill(dgv);
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "Mã HĐ", FillWeight = 12 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomNumber", HeaderText = "Phòng", FillWeight = 7 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TenantName", HeaderText = "Khách thuê", FillWeight = 12 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StartDate",
                HeaderText = "Bắt đầu",
                FillWeight = 8,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EndDate",
                HeaderText = "Kết thúc",
                FillWeight = 8,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MonthlyRent",
                HeaderText = "Tiền thuê",
                FillWeight = 9,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TT", FillWeight = 6 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PendingEditStatus", HeaderText = "Sửa?", FillWeight = 6 });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "EditCol",
                HeaderText = "",
                Text = "Sửa",
                UseColumnTextForLinkValue = true,
                FillWeight = 5,
                LinkColor = AppColors.Primary
            });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "CancelPendingCol",
                HeaderText = "",
                Text = "Hủy đề xuất",
                UseColumnTextForLinkValue = true,
                FillWeight = 8,
                LinkColor = AppColors.Warning
            });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "AssignCol",
                HeaderText = "",
                Text = "Gán khách",
                UseColumnTextForLinkValue = true,
                FillWeight = 7,
                LinkColor = AppColors.Primary
            });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "PrintCol",
                HeaderText = "",
                Text = "In/PDF",
                UseColumnTextForLinkValue = true,
                FillWeight = 6,
                LinkColor = AppColors.Primary
            });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "ExtendCol",
                HeaderText = "",
                Text = "Gia hạn",
                UseColumnTextForLinkValue = true,
                FillWeight = 6,
                LinkColor = AppColors.Success
            });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "TerminateCol",
                HeaderText = "",
                Text = "Hủy HĐ",
                UseColumnTextForLinkValue = true,
                FillWeight = 6,
                LinkColor = AppColors.Danger
            });
            dgv.CellContentClick += async (s, e) => await Dgv_CellContentClick(e);

            UIHelper.WirePage(this, dgv, header, pnlCreate);
            SyncCreateFieldWidths();
        }

        private void SyncCreateFieldWidths()
        {
            if (createStack == null || pnlCreate == null) return;
            int w = Math.Max(180, pnlCreate.ClientSize.Width - pnlCreate.Padding.Horizontal - 8);
            createStack.Width = w;
            foreach (Control c in createStack.Controls)
            {
                if (c is Panel field && field.Controls.Count >= 2)
                {
                    field.Width = w;
                    field.Controls[1].Width = w;
                }
                else if (c is ModernButton btn)
                {
                    btn.Width = w;
                }
                else if (c is Label lbl && lbl.MaximumSize.Width > 0)
                {
                    lbl.MaximumSize = new Size(w, 0);
                }
            }
        }

        private async Task OnLoadAsync()
        {
            await _uiLoadLock.WaitAsync();
            try
            {
                _suppressComboEvents = true;
                var landlordId = UserSession.CurrentUser!.UserID;
                await WithServicesAsync(async (houses, rooms, contracts, landlord) =>
                {
                    var houseList = (await houses.GetHousesByOwnerAsync(landlordId)).ToList();
                    cboHouse.DataSource = null;
                    cboHouse.DisplayMember = nameof(HouseDto.HouseName);
                    cboHouse.ValueMember = nameof(HouseDto.HouseID);
                    cboHouse.DataSource = houseList;

                    await BindRoomsAsync(rooms);
                    await BindTenantsAsync(landlord);
                    await BindContractsAsync(contracts);
                });
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
            finally
            {
                _suppressComboEvents = false;
                _uiLoadLock.Release();
            }
        }

        private async Task OnHouseChangedAsync()
        {
            await _uiLoadLock.WaitAsync();
            try
            {
                _suppressComboEvents = true;
                await WithServicesAsync(async (_, rooms, _, landlord) =>
                {
                    await BindRoomsAsync(rooms);
                    await BindTenantsAsync(landlord);
                });
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
            finally
            {
                _suppressComboEvents = false;
                _uiLoadLock.Release();
            }
        }

        private async Task OnRoomChangedAsync()
        {
            await _uiLoadLock.WaitAsync();
            try
            {
                await WithServicesAsync(async (_, _, _, landlord) =>
                {
                    await BindTenantsAsync(landlord);
                });
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
            finally
            {
                _uiLoadLock.Release();
            }
        }

        private async Task BindRoomsAsync(IRoomService roomService)
        {
            cboRoom.DataSource = null;
            int houseId = 0;
            if (cboHouse.SelectedItem is HouseDto h)
                houseId = h.HouseID;
            else if (cboHouse.SelectedValue != null)
                int.TryParse(cboHouse.SelectedValue.ToString(), out houseId);
            if (houseId <= 0) return;

            var roomList = (await roomService.GetRoomsByHouseAsync(houseId))
                .Where(r => r.Status == "Available")
                .ToList();
            cboRoom.DisplayMember = nameof(RoomDto.RoomNumber);
            cboRoom.ValueMember = nameof(RoomDto.RoomID);
            cboRoom.DataSource = roomList;
            UpdateRentFromSelectedRoom();
        }

        private async Task BindTenantsAsync(ILandlordService landlordService)
        {
            int? roomId = null;
            if (cboRoom.SelectedItem is RoomDto room)
                roomId = room.RoomID;
            else if (cboRoom.SelectedValue != null &&
                     int.TryParse(cboRoom.SelectedValue.ToString(), out int rid) &&
                     rid > 0)
            {
                roomId = rid;
            }

            var landlordId = UserSession.CurrentUser!.UserID;
            _tenants = (await landlordService.GetAppointmentTenantsAsync(landlordId, roomId)).ToList();
            if (roomId.HasValue && _tenants.Count == 0)
                _tenants = (await landlordService.GetAppointmentTenantsAsync(landlordId, null)).ToList();

            BindTenantCombo();
        }

        private async Task BindContractsAsync(IContractService contractService)
        {
            var list = await contractService.GetContractsByLandlordAsync(UserSession.CurrentUser!.UserID);
            dgv.DataSource = list.OrderByDescending(c => c.ContractID).ToList();
        }

        private void BindTenantCombo()
        {
            var items = new List<UserDto>
            {
                new() { UserID = 0, FullName = "(Chưa có khách — lưu nháp)" }
            };
            foreach (var t in _tenants)
            {
                items.Add(new UserDto
                {
                    UserID = t.UserID,
                    FullName = string.IsNullOrWhiteSpace(t.Phone) ? t.FullName : $"{t.FullName} ({t.Phone})",
                    Phone = t.Phone,
                    Email = t.Email,
                    Username = t.Username,
                    Status = t.Status,
                    RoleID = t.RoleID
                });
            }
            cboTenant.DataSource = null;
            cboTenant.DisplayMember = nameof(UserDto.FullName);
            cboTenant.ValueMember = nameof(UserDto.UserID);
            cboTenant.DataSource = items;
            if (cboTenant.Items.Count > 0)
                cboTenant.SelectedIndex = 0;
        }

        private void UpdateRentFromSelectedRoom()
        {
            if (cboRoom.SelectedItem is RoomDto room)
                txtRent.Text = room.Price.ToString("0");
        }

        private async Task CreateContractAsync()
        {
            int roomId = 0;
            if (cboRoom.SelectedItem is RoomDto r)
                roomId = r.RoomID;
            else if (cboRoom.SelectedValue != null)
                int.TryParse(cboRoom.SelectedValue.ToString(), out roomId);

            if (roomId <= 0)
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
            if (cboTenant.SelectedItem is UserDto u && u.UserID > 0)
                tenantId = u.UserID;
            else if (cboTenant.SelectedValue != null &&
                     int.TryParse(cboTenant.SelectedValue.ToString(), out int tid) &&
                     tid > 0)
            {
                tenantId = tid;
            }

            try
            {
                var created = await WithServicesAsync(async (_, __, contracts, ___) =>
                    await contracts.CreateContractAsync(new CreateContractDto
                    {
                        RoomID = roomId,
                        TenantID = tenantId,
                        StartDate = dtpStart.Value.Date,
                        EndDate = dtpEnd.Value.Date,
                        Deposit = deposit,
                        MonthlyRent = rent,
                        ElectricPrice = electric,
                        WaterPrice = water
                    }, UserSession.CurrentUser!.UserID));

                AppDialog.ShowInfo(tenantId.HasValue
                    ? "Đã gửi đề nghị thuê. Hợp đồng PendingConfirm — chờ khách Đồng ý mới Active / Đã thuê."
                    : "Đã lưu hợp đồng nháp. Khi có khách, bấm \"Gán khách\" trên danh sách.");
                ToastNotifier.Show(this,
                    created.Status == "Draft" ? "Đã lưu hợp đồng nháp"
                    : created.Status == "PendingConfirm" ? "Chờ khách xác nhận thuê"
                    : "Đã tạo hợp đồng",
                    ToastKind.Success);

                await _uiLoadLock.WaitAsync();
                try
                {
                    _suppressComboEvents = true;
                    await WithServicesAsync(async (_, rooms, contracts, _) =>
                    {
                        await BindRoomsAsync(rooms);
                        await BindContractsAsync(contracts);
                    });
                }
                finally
                {
                    _suppressComboEvents = false;
                    _uiLoadLock.Release();
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async Task CreateDraftsForAllRoomsAsync()
        {
            int houseId = 0;
            if (cboHouse.SelectedItem is HouseDto h)
                houseId = h.HouseID;
            else if (cboHouse.SelectedValue != null)
                int.TryParse(cboHouse.SelectedValue.ToString(), out houseId);

            if (houseId <= 0)
            {
                AppDialog.ShowWarning("Vui lòng chọn nhà.");
                return;
            }

            if (!decimal.TryParse(txtDeposit.Text, out decimal deposit) ||
                !decimal.TryParse(txtElectric.Text, out decimal electric) ||
                !decimal.TryParse(txtWater.Text, out decimal water))
            {
                AppDialog.ShowWarning("Vui lòng nhập cọc / giá điện / giá nước hợp lệ.");
                return;
            }

            // Tiền thuê: nếu nhập > 0 dùng chung; nếu trống/0 lấy giá từng phòng
            decimal.TryParse(txtRent.Text, out decimal rentShared);

            if (!AppDialog.Confirm(
                    "Tạo hợp đồng NHÁP cho tất cả phòng chưa có HĐ của nhà này?\n" +
                    "• Chưa gán khách\n" +
                    "• Ngày / cọc / điện / nước lấy từ form\n" +
                    (rentShared > 0
                        ? "• Tiền thuê dùng chung giá trên form"
                        : "• Tiền thuê lấy theo giá từng phòng")))
                return;

            try
            {
                var result = await WithServicesAsync(async (_, __, contracts, ___) =>
                    await contracts.CreateDraftContractsForHouseAsync(new BulkCreateDraftContractsDto
                    {
                        HouseID = houseId,
                        StartDate = dtpStart.Value.Date,
                        EndDate = dtpEnd.Value.Date,
                        Deposit = deposit,
                        MonthlyRent = rentShared,
                        ElectricPrice = electric,
                        WaterPrice = water
                    }, UserSession.CurrentUser!.UserID));

                AppDialog.ShowInfo(result.Message);
                ToastNotifier.Show(this,
                    result.CreatedCount > 0 ? $"Đã tạo {result.CreatedCount} HĐ nháp" : "Không tạo thêm HĐ",
                    result.CreatedCount > 0 ? ToastKind.Success : ToastKind.Info);

                await _uiLoadLock.WaitAsync();
                try
                {
                    _suppressComboEvents = true;
                    await WithServicesAsync(async (_, rooms, contracts, _) =>
                    {
                        await BindRoomsAsync(rooms);
                        await BindContractsAsync(contracts);
                    });
                }
                finally
                {
                    _suppressComboEvents = false;
                    _uiLoadLock.Release();
                }
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
                    var detail = await WithServicesAsync(async (_, __, contracts, ___) =>
                        await contracts.GetContractByIdAsync(contract.ContractID));
                    ContractPrintHelper.OpenAndPrint(detail);
                    return;
                }

                if (col == "EditCol")
                {
                    await EditContractAsync(contract);
                    return;
                }

                if (col == "CancelPendingCol")
                {
                    if (!string.Equals(contract.PendingEditStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                    {
                        AppDialog.ShowInfo("Hợp đồng này không có đề xuất sửa đang chờ.");
                        return;
                    }
                    if (!AppDialog.Confirm($"Hủy đề xuất sửa {contract.ContractCode}?"))
                        return;
                    await WithServicesAsync(async (_, __, contracts, ___) =>
                        await contracts.CancelPendingContractEditAsync(contract.ContractID, UserSession.CurrentUser!.UserID));
                    ToastNotifier.Show(this, "Đã hủy đề xuất sửa", ToastKind.Info);
                    await WithServicesAsync(async (_, __, contracts, ___) => await BindContractsAsync(contracts));
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
                    await WithServicesAsync(async (_, __, contracts, ___) =>
                        await contracts.ExtendContractAsync(contract.ContractID, newEnd, UserSession.CurrentUser!.UserID));
                    AppDialog.ShowInfo($"Đã gia hạn đến {newEnd:dd/MM/yyyy}.");
                    await WithServicesAsync(async (_, __, contracts, ___) => await BindContractsAsync(contracts));
                    return;
                }

                if (col == "TerminateCol")
                {
                    if (contract.Status != "Active" && contract.Status != "Draft" && contract.Status != "PendingConfirm")
                    {
                        AppDialog.ShowWarning("Chỉ hủy hợp đồng nháp, chờ xác nhận hoặc Active.");
                        return;
                    }
                    if (!AppDialog.Confirm($"Hủy hợp đồng {contract.ContractCode}?"))
                        return;
                    await WithServicesAsync(async (_, __, contracts, ___) =>
                        await contracts.TerminateContractAsync(contract.ContractID));
                    AppDialog.ShowInfo("Đã hủy hợp đồng.");
                    await _uiLoadLock.WaitAsync();
                    try
                    {
                        _suppressComboEvents = true;
                        await WithServicesAsync(async (_, rooms, contracts, _) =>
                        {
                            await BindRoomsAsync(rooms);
                            await BindContractsAsync(contracts);
                        });
                    }
                    finally
                    {
                        _suppressComboEvents = false;
                        _uiLoadLock.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async Task EditContractAsync(ContractDto contract)
        {
            if (contract.Status != "Draft" && contract.Status != "Active")
            {
                AppDialog.ShowWarning("Chỉ sửa hợp đồng nháp hoặc Active.");
                return;
            }
            if (string.Equals(contract.PendingEditStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                AppDialog.ShowWarning("Đang chờ khách xác nhận đề xuất trước. Hãy «Hủy đề xuất» nếu muốn sửa lại.");
                return;
            }

            ContractDetailDto detail;
            try
            {
                detail = await WithServicesAsync(async (_, __, contracts, ___) =>
                    await contracts.GetContractByIdAsync(contract.ContractID));
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
                return;
            }

            using var dlg = new Form
            {
                Text = $"Sửa HĐ {contract.ContractCode}",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(420, 420),
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = AppColors.Background
            };

            int y = 16;
            Label L(string t) { var l = new Label { Text = t, Location = new Point(16, y), AutoSize = true, ForeColor = AppColors.TextMuted }; dlg.Controls.Add(l); y += 20; return l; }
            ModernTextBox T(string v) { var tb = new ModernTextBox { Location = new Point(16, y), Size = new Size(380, 32), Text = v }; dlg.Controls.Add(tb); y += 40; return tb; }

            var info = new Label
            {
                Text = contract.TenantID.HasValue
                    ? "Có khách thuê: sau khi lưu sẽ chờ khách xác nhận mới chính thức."
                    : "Chưa có khách: thay đổi áp dụng ngay.",
                Location = new Point(16, y),
                MaximumSize = new Size(380, 0),
                AutoSize = true,
                ForeColor = AppColors.Primary
            };
            dlg.Controls.Add(info);
            y += 40;

            L("Ngày kết thúc");
            var dtpEnd = new DateTimePicker { Location = new Point(16, y), Size = new Size(380, 28), Format = DateTimePickerFormat.Short, Value = detail.EndDate };
            dlg.Controls.Add(dtpEnd);
            y += 40;
            L("Tiền cọc");
            var txtDep = T(detail.Deposit.ToString("0"));
            L("Tiền thuê / tháng");
            var txtRent = T(detail.MonthlyRent.ToString("0"));
            L("Giá điện / số");
            var txtEl = T(detail.ElectricPrice.ToString("0"));
            L("Giá nước / số");
            var txtWa = T(detail.WaterPrice.ToString("0"));
            L("Ghi chú (gửi khách)");
            var txtNote = T("");

            var btnOk = new ModernButton
            {
                Text = contract.TenantID.HasValue ? "Gửi đề xuất" : "Lưu ngay",
                Size = new Size(140, 36),
                Location = new Point(140, y + 8),
                BackColor = AppColors.Primary,
                DialogResult = DialogResult.OK
            };
            var btnCancel = new ModernButton
            {
                Text = "Đóng",
                Size = new Size(90, 36),
                Location = new Point(290, y + 8),
                BackColor = AppColors.Border,
                ForeColor = AppColors.TextMain,
                DialogResult = DialogResult.Cancel
            };
            dlg.Controls.Add(btnOk);
            dlg.Controls.Add(btnCancel);
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;
            dlg.ClientSize = new Size(420, y + 60);

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            if (!decimal.TryParse(txtDep.Text, out var dep) ||
                !decimal.TryParse(txtRent.Text, out var rent) ||
                !decimal.TryParse(txtEl.Text, out var el) ||
                !decimal.TryParse(txtWa.Text, out var wa) ||
                rent <= 0)
            {
                AppDialog.ShowWarning("Số tiền không hợp lệ.");
                return;
            }

            try
            {
                await WithServicesAsync(async (_, __, contracts, ___) =>
                    await contracts.UpdateContractAsync(new UpdateContractDto
                    {
                        ContractID = contract.ContractID,
                        EndDate = dtpEnd.Value.Date,
                        Deposit = dep,
                        MonthlyRent = rent,
                        ElectricPrice = el,
                        WaterPrice = wa,
                        Note = txtNote.Text
                    }, UserSession.CurrentUser!.UserID));

                AppDialog.ShowInfo(contract.TenantID.HasValue
                    ? "Đã gửi đề xuất sửa. Hợp đồng chỉ đổi sau khi khách xác nhận. Giá mới áp dụng từ ngày xác nhận."
                    : "Đã cập nhật hợp đồng.");
                ToastNotifier.Show(this, "Đã lưu thay đổi HĐ", ToastKind.Success);
                await WithServicesAsync(async (_, __, contracts, ___) => await BindContractsAsync(contracts));
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
            if (contract.Status != "Draft")
            {
                AppDialog.ShowWarning("Chỉ gán khách cho hợp đồng nháp (Draft).");
                return;
            }

            var candidates = await WithServicesAsync(async (_, __, ___, landlord) =>
                (await landlord.GetAppointmentTenantsAsync(UserSession.CurrentUser!.UserID, null)).ToList());

            if (candidates.Count == 0)
            {
                AppDialog.ShowWarning("Chưa có khách đặt lịch xem phòng của bạn. Khách cần đặt lịch trước khi được gán vào hợp đồng.");
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

            int tenantId = 0;
            if (cbo.SelectedItem is UserDto sel)
                tenantId = sel.UserID;
            else if (cbo.SelectedValue != null)
                int.TryParse(cbo.SelectedValue.ToString(), out tenantId);
            if (tenantId <= 0)
            {
                AppDialog.ShowWarning("Vui lòng chọn khách thuê.");
                return;
            }

            await WithServicesAsync(async (_, __, contracts, ___) =>
                await contracts.AssignTenantAsync(new AssignTenantDto
                {
                    ContractID = contract.ContractID,
                    TenantID = tenantId
                }, UserSession.CurrentUser!.UserID));

            ToastNotifier.Show(this, "Đã gán khách thuê", ToastKind.Success);
            AppDialog.ShowInfo("Đã gửi đề nghị thuê cho khách. Hợp đồng ở trạng thái PendingConfirm — Dashboard «Đã thuê» chỉ tăng khi khách bấm Đồng ý thuê.");
            ToastNotifier.Show(this, "Chờ khách xác nhận thuê", ToastKind.Success);

            await _uiLoadLock.WaitAsync();
            try
            {
                _suppressComboEvents = true;
                await WithServicesAsync(async (_, rooms, contracts, _) =>
                {
                    await BindRoomsAsync(rooms);
                    await BindContractsAsync(contracts);
                });
            }
            finally
            {
                _suppressComboEvents = false;
                _uiLoadLock.Release();
            }
        }
    }
}
