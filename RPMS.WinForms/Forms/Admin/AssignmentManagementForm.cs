using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
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
        private Label lblHint = null!;
        private ModernButton btnAssign = null!;
        private ModernButton btnRefresh = null!;

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
            ClientSize = new Size(1100, 640);
            MinimumSize = new Size(780, 480);
            AutoScroll = false;

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 128,
                BackColor = AppColors.Card,
                Padding = new Padding(16, 12, 16, 12)
            };
            pnlTop.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(pen, 0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);
            };

            var title = new Label
            {
                Text = "Gán Manager cho nhà",
                Font = AppTypography.Heading,
                ForeColor = AppColors.TextMain,
                AutoSize = true,
                Location = new Point(16, 10)
            };

            lblHint = new Label
            {
                Text = "Chọn nhà và quản lý viên, rồi bấm Gán.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                Location = new Point(16, 42)
            };

            var tbl = new TableLayoutPanel
            {
                Location = new Point(16, 68),
                Height = 44,
                ColumnCount = 5,
                RowCount = 1,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));

            var lblHouse = new Label
            {
                Text = "Nhà",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = AppColors.TextMuted,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            cboHouse = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTypography.Body
            };
            var lblMgr = new Label
            {
                Text = "Manager",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = AppColors.TextMuted,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(8, 0, 0, 0)
            };
            cboManager = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTypography.Body
            };

            var flpBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(8, 0, 0, 0)
            };
            btnAssign = new ModernButton
            {
                Text = "Gán quản lý",
                Size = new Size(120, 36),
                BackColor = AppColors.Primary,
                Margin = new Padding(0, 2, 8, 0)
            };
            btnAssign.Click += async (s, e) => await AssignAsync();
            btnRefresh = new ModernButton
            {
                Text = "Làm mới",
                Size = new Size(90, 36),
                BackColor = AppColors.Border,
                ForeColor = AppColors.TextMain,
                Margin = new Padding(0, 2, 0, 0)
            };
            btnRefresh.Click += async (s, e) => await LoadAllAsync();
            flpBtns.Controls.AddRange(new Control[] { btnAssign, btnRefresh });

            tbl.Controls.Add(lblHouse, 0, 0);
            tbl.Controls.Add(cboHouse, 1, 0);
            tbl.Controls.Add(lblMgr, 2, 0);
            tbl.Controls.Add(cboManager, 3, 0);
            tbl.Controls.Add(flpBtns, 4, 0);

            pnlTop.Controls.Add(title);
            pnlTop.Controls.Add(lblHint);
            pnlTop.Controls.Add(tbl);
            pnlTop.Resize += (s, e) =>
            {
                tbl.Width = Math.Max(400, pnlTop.ClientSize.Width - 32);
            };

            dgv = new ModernDataGridView { Dock = DockStyle.Fill };
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseName", HeaderText = "Nhà", FillWeight = 18 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseAddress", HeaderText = "Địa chỉ", FillWeight = 28 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ManagerName", HeaderText = "Manager", FillWeight = 18 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AssignedDate",
                HeaderText = "Ngày gán",
                FillWeight = 12,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "TT", FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "DeactivateCol",
                HeaderText = "",
                Text = "Ngưng",
                UseColumnTextForLinkValue = true,
                FillWeight = 8,
                LinkColor = AppColors.Danger
            });
            dgv.CellContentClick += async (s, e) => await DgvClickAsync(e);

            Controls.Add(dgv);
            Controls.Add(pnlTop);
            UIHelper.WireListPage(this, pnlTop, dgv);
        }

        private async Task LoadAllAsync()
        {
            try
            {
                btnAssign.Enabled = false;
                var houses = (await _houseService.GetAllHousesAsync())
                    .OrderBy(h => h.HouseName)
                    .Select(h => new HousePickItem
                    {
                        HouseID = h.HouseID,
                        Display = string.IsNullOrWhiteSpace(h.Address)
                            ? h.HouseName
                            : $"{h.HouseName} — {h.Address}"
                    })
                    .ToList();

                cboHouse.BeginUpdate();
                cboHouse.DataSource = null;
                cboHouse.DisplayMember = nameof(HousePickItem.Display);
                cboHouse.ValueMember = nameof(HousePickItem.HouseID);
                cboHouse.DataSource = houses;
                cboHouse.EndUpdate();

                var managers = (await _userService.GetUsersByRoleAsync(4))
                    .Where(u => u.Status == "Active")
                    .OrderBy(u => u.FullName)
                    .Select(u => new ManagerPickItem
                    {
                        UserID = u.UserID,
                        Display = string.IsNullOrWhiteSpace(u.Phone)
                            ? u.FullName
                            : $"{u.FullName} ({u.Phone})"
                    })
                    .ToList();

                cboManager.BeginUpdate();
                cboManager.DataSource = null;
                cboManager.DisplayMember = nameof(ManagerPickItem.Display);
                cboManager.ValueMember = nameof(ManagerPickItem.UserID);
                cboManager.DataSource = managers;
                cboManager.EndUpdate();

                var list = (await _assignmentService.GetAllAsync())
                    .OrderByDescending(a => a.AssignedDate)
                    .ToList();
                dgv.DataSource = list;

                if (houses.Count == 0)
                    lblHint.Text = "Chưa có nhà trong hệ thống — chủ nhà cần tạo nhà trước.";
                else if (managers.Count == 0)
                    lblHint.Text = "Chưa có Manager Active — tạo user Role Manager (VD: manager / 123456).";
                else
                    lblHint.Text = $"Sẵn sàng: {houses.Count} nhà, {managers.Count} manager. Chọn rồi bấm «Gán quản lý».";

                btnAssign.Enabled = houses.Count > 0 && managers.Count > 0;
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Không tải được phân công: " + ex.Message);
            }
        }

        private async Task AssignAsync()
        {
            int houseId = ResolveHouseId();
            int managerId = ResolveManagerId();
            if (houseId <= 0 || managerId <= 0)
            {
                AppDialog.ShowWarning("Vui lòng chọn đầy đủ nhà và manager.");
                return;
            }

            try
            {
                btnAssign.Enabled = false;
                await _assignmentService.CreateAsync(new CreateAssignmentDto
                {
                    HouseID = houseId,
                    ManagerID = managerId
                });
                ToastNotifier.Show(this, "Đã gán quản lý cho nhà", ToastKind.Success);
                AppDialog.ShowInfo("Gán quản lý thành công.");
                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
                btnAssign.Enabled = true;
            }
        }

        private int ResolveHouseId()
        {
            if (cboHouse.SelectedItem is HousePickItem item)
                return item.HouseID;
            if (cboHouse.SelectedValue != null && int.TryParse(cboHouse.SelectedValue.ToString(), out int id))
                return id;
            return 0;
        }

        private int ResolveManagerId()
        {
            if (cboManager.SelectedItem is ManagerPickItem item)
                return item.UserID;
            if (cboManager.SelectedValue != null && int.TryParse(cboManager.SelectedValue.ToString(), out int id))
                return id;
            return 0;
        }

        private async Task DgvClickAsync(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgv.Columns[e.ColumnIndex].Name != "DeactivateCol") return;
            var item = dgv.Rows[e.RowIndex].DataBoundItem as AssignmentDto;
            if (item == null || item.Status != "Active") return;
            if (!AppDialog.Confirm($"Ngưng phân công {item.ManagerName} — {item.HouseName}?")) return;

            try
            {
                await _assignmentService.DeactivateAsync(item.AssignmentID);
                ToastNotifier.Show(this, "Đã ngưng phân công", ToastKind.Info);
                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private sealed class HousePickItem
        {
            public int HouseID { get; set; }
            public string Display { get; set; } = "";
        }

        private sealed class ManagerPickItem
        {
            public int UserID { get; set; }
            public string Display { get; set; } = "";
        }
    }
}
