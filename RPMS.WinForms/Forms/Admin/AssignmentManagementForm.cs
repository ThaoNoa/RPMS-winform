using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Assignment;
using RPMS.DTO.House;
using RPMS.DTO.User;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Admin
{
    public class AssignmentManagementForm : Form
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IHouseService _houseService;
        private readonly IUserService _userService;
        private ModernDataGridView dgv = null!;
        private ComboBox cboHouse = null!;
        private ComboBox cboManager = null!;

        public AssignmentManagementForm(
            IAssignmentService assignmentService,
            IHouseService houseService,
            IUserService userService)
        {
            _assignmentService = assignmentService;
            _houseService = houseService;
            _userService = userService;
            InitializeUI();
            Load += async (s, e) => await LoadAllAsync();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Phân công quản lý";
            ClientSize = new Size(1050, 620);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = AppColors.Card };
            pnlTop.Controls.Add(new Label
            {
                Text = "Gán Manager cho nhà",
                Font = AppTypography.Heading,
                Location = new Point(20, 12),
                AutoSize = true,
                ForeColor = AppColors.TextMain
            });

            cboHouse = new ComboBox { Location = new Point(20, 48), Size = new Size(280, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cboManager = new ComboBox { Location = new Point(320, 48), Size = new Size(240, 28), DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            var btnAssign = new ModernButton { Text = "Gán", Location = new Point(580, 44), Size = new Size(100, 36), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnAssign.Click += async (s, e) => await AssignAsync();
            pnlTop.Controls.AddRange(new Control[] { cboHouse, cboManager, btnAssign });

            dgv = new ModernDataGridView { Dock = DockStyle.Fill };
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseName", HeaderText = "Nhà", Width = 180 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseAddress", HeaderText = "Địa chỉ", Width = 260 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ManagerName", HeaderText = "Manager", Width = 160 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AssignedDate",
                HeaderText = "Ngày gán",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TT", Width = 90 });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "DeactivateCol",
                HeaderText = "",
                Text = "Ngưng",
                UseColumnTextForLinkValue = true,
                Width = 70,
                LinkColor = AppColors.Danger
            });
            dgv.CellContentClick += async (s, e) => await DgvClickAsync(e);

            Controls.Add(dgv);
            Controls.Add(pnlTop);
            UIHelper.WireListPage(this, pnlTop, dgv);
            MinimumSize = new Size(700, 480);
        }

        private async Task LoadAllAsync()
        {
            try
            {
                var houses = (await _houseService.GetAllHousesAsync()).ToList();
                cboHouse.DataSource = houses;
                cboHouse.DisplayMember = nameof(HouseDto.HouseName);
                cboHouse.ValueMember = nameof(HouseDto.HouseID);

                var managers = (await _userService.GetUsersByRoleAsync(4)).Where(u => u.Status == "Active").ToList();
                cboManager.DataSource = managers;
                cboManager.DisplayMember = nameof(UserDto.FullName);
                cboManager.ValueMember = nameof(UserDto.UserID);

                var list = await _assignmentService.GetAllAsync();
                dgv.DataSource = list.ToList();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async Task AssignAsync()
        {
            if (cboHouse.SelectedValue == null || cboManager.SelectedValue == null)
            {
                AppDialog.ShowWarning("Vui lòng chọn nhà và manager.");
                return;
            }

            try
            {
                await _assignmentService.CreateAsync(new CreateAssignmentDto
                {
                    HouseID = Convert.ToInt32(cboHouse.SelectedValue),
                    ManagerID = Convert.ToInt32(cboManager.SelectedValue)
                });
                AppDialog.ShowInfo("Gán quản lý thành công.");
                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async Task DgvClickAsync(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgv.Columns[e.ColumnIndex].Name != "DeactivateCol") return;
            var item = dgv.Rows[e.RowIndex].DataBoundItem as AssignmentDto;
            if (item == null || item.Status != "Active") return;
            if (!AppDialog.Confirm($"Ngưng phân công {item.ManagerName} - {item.HouseName}?")) return;

            try
            {
                await _assignmentService.DeactivateAsync(item.AssignmentID);
                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }
    }
}
