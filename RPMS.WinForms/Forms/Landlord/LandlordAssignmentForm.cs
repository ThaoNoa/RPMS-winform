using Microsoft.Extensions.DependencyInjection;
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

namespace RPMS.WinForms.Forms.Landlord
{
    /// <summary>Chủ nhà gán Manager theo UserID — không liệt kê tất cả manager.</summary>
    public class LandlordAssignmentForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ModernDataGridView dgv = null!;
        private ComboBox cboHouse = null!;
        private ModernTextBox txtManagerId = null!;
        private Label lblManagerPreview = null!;
        private Label lblHint = null!;
        private UserDto? _foundManager;

        public LandlordAssignmentForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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
                Height = 150,
                BackColor = AppColors.Card,
                Padding = new Padding(16)
            };
            pnlTop.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(pen, 0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);
            };

            pnlTop.Controls.Add(new Label
            {
                Text = "Gán Manager cho nhà của bạn",
                Font = AppTypography.Heading,
                ForeColor = AppColors.TextMain,
                Location = new Point(16, 10),
                AutoSize = true
            });
            lblHint = new Label
            {
                Text = "Nhập UserID của Manager (không hiện danh sách toàn hệ thống).",
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextMuted,
                Location = new Point(16, 42),
                AutoSize = true
            };
            pnlTop.Controls.Add(lblHint);

            var tbl = new TableLayoutPanel
            {
                Location = new Point(16, 70),
                Height = 64,
                ColumnCount = 6,
                RowCount = 1,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

            cboHouse = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            txtManagerId = new ModernTextBox { Dock = DockStyle.Fill, PlaceholderText = "VD: 4" };
            var btnFind = new ModernButton { Text = "Tìm", Size = new Size(80, 34), BackColor = AppColors.TextMuted, Margin = new Padding(4, 2, 0, 0) };
            btnFind.Click += async (s, e) => await FindManagerAsync();
            var btnAssign = new ModernButton { Text = "Gán", Size = new Size(80, 34), BackColor = AppColors.Primary, Margin = new Padding(4, 2, 0, 0) };
            btnAssign.Click += async (s, e) => await AssignAsync();
            lblManagerPreview = new Label
            {
                Text = "Chưa chọn Manager",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = AppColors.TextMuted,
                Padding = new Padding(8, 0, 0, 0)
            };

            tbl.Controls.Add(new Label { Text = "Nhà", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = AppColors.TextMuted }, 0, 0);
            tbl.Controls.Add(cboHouse, 1, 0);
            tbl.Controls.Add(new Label { Text = "Manager ID", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = AppColors.TextMuted, Padding = new Padding(8, 0, 0, 0) }, 2, 0);
            tbl.Controls.Add(txtManagerId, 3, 0);
            var flp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            flp.Controls.Add(btnFind);
            flp.Controls.Add(btnAssign);
            tbl.Controls.Add(flp, 4, 0);
            tbl.Controls.Add(lblManagerPreview, 5, 0);
            pnlTop.Controls.Add(tbl);
            pnlTop.Resize += (s, e) => tbl.Width = Math.Max(500, pnlTop.ClientSize.Width - 32);

            dgv = new ModernDataGridView { Dock = DockStyle.Fill };
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseName", HeaderText = "Nhà", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseAddress", HeaderText = "Địa chỉ", FillWeight = 28 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ManagerName", HeaderText = "Manager", FillWeight = 16 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ManagerID", HeaderText = "ID", FillWeight = 8 });
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
                using var scope = _scopeFactory.CreateScope();
                var houses = scope.ServiceProvider.GetRequiredService<IHouseService>();
                var assignments = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                int landlordId = UserSession.CurrentUser!.UserID;

                var houseList = (await houses.GetHousesByOwnerAsync(landlordId))
                    .OrderBy(h => h.HouseName)
                    .Select(h => new HousePick
                    {
                        HouseID = h.HouseID,
                        Display = string.IsNullOrWhiteSpace(h.Address) ? h.HouseName : $"{h.HouseName} — {h.Address}"
                    })
                    .ToList();

                cboHouse.DataSource = null;
                cboHouse.DisplayMember = nameof(HousePick.Display);
                cboHouse.ValueMember = nameof(HousePick.HouseID);
                cboHouse.DataSource = houseList;

                dgv.DataSource = (await assignments.GetByLandlordAsync(landlordId)).ToList();
                lblHint.Text = houseList.Count == 0
                    ? "Bạn chưa có nhà — tạo nhà trước rồi gán Manager."
                    : "Nhập UserID Manager → Tìm → Gán. Chỉ nhà của bạn được liệt kê.";
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async Task FindManagerAsync()
        {
            _foundManager = null;
            lblManagerPreview.Text = "Đang tìm…";
            lblManagerPreview.ForeColor = AppColors.TextMuted;

            if (!int.TryParse(txtManagerId.Text.Trim(), out int id) || id <= 0)
            {
                lblManagerPreview.Text = "UserID không hợp lệ";
                lblManagerPreview.ForeColor = AppColors.Danger;
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var users = scope.ServiceProvider.GetRequiredService<IUserService>();
                var user = await users.GetUserByIdAsync(id);
                if (user.RoleID != 4 && !string.Equals(user.RoleName, "Manager", StringComparison.OrdinalIgnoreCase))
                {
                    lblManagerPreview.Text = $"#{id} không phải Manager (Role: {user.RoleName})";
                    lblManagerPreview.ForeColor = AppColors.Danger;
                    return;
                }
                if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    lblManagerPreview.Text = $"Manager #{id} không Active";
                    lblManagerPreview.ForeColor = AppColors.Danger;
                    return;
                }

                _foundManager = user;
                lblManagerPreview.Text = $"✓ {user.FullName} — {user.Phone} (ID {user.UserID})";
                lblManagerPreview.ForeColor = AppColors.Success;
            }
            catch (Exception ex)
            {
                lblManagerPreview.Text = ex.Message;
                lblManagerPreview.ForeColor = AppColors.Danger;
            }
        }

        private async Task AssignAsync()
        {
            if (cboHouse.SelectedItem is not HousePick house)
            {
                AppDialog.ShowWarning("Vui lòng chọn nhà.");
                return;
            }
            if (_foundManager == null)
            {
                AppDialog.ShowWarning("Hãy tìm Manager theo ID trước khi gán.");
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                await svc.CreateAsync(new CreateAssignmentDto
                {
                    HouseID = house.HouseID,
                    ManagerID = _foundManager.UserID
                }, UserSession.CurrentUser!.UserID);
                ToastNotifier.Show(this, "Đã gán Manager", ToastKind.Success);
                AppDialog.ShowInfo($"Đã gán {_foundManager.FullName} cho nhà {house.Display}.");
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
            if (dgv.Rows[e.RowIndex].DataBoundItem is not AssignmentDto item) return;
            if (item.Status != "Active") return;
            if (!AppDialog.Confirm($"Ngưng {item.ManagerName} — {item.HouseName}?")) return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                await svc.DeactivateAsync(item.AssignmentID, UserSession.CurrentUser!.UserID);
                ToastNotifier.Show(this, "Đã ngưng phân công", ToastKind.Info);
                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private sealed class HousePick
        {
            public int HouseID { get; set; }
            public string Display { get; set; } = "";
        }
    }
}
