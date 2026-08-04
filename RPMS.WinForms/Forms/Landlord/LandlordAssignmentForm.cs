using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Assignment;
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
    /// <summary>Chủ nhà gán Manager theo UserID hoặc Username — không liệt kê tất cả manager.</summary>
    public class LandlordAssignmentForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ModernDataGridView dgv = null!;
        private ComboBox cboHouse = null!;
        private ModernTextBox txtManagerQuery = null!;
        private Label lblManagerPreview = null!;
        private Label lblHint = null!;
        private ModernButton btnAssign = null!;

        private UserDto? _foundManager;

        public LandlordAssignmentForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            InitializeUI();
            Load += async (s, e) => await LoadAllAsync();
        }

        private void InitializeUI()
        {
            ClientSize = new Size(1100, 680);
            Text = "Phân công quản lý";

            var header = UIHelper.CreatePageHeader("Gán Manager cho nhà của bạn");

            var filter = UIHelper.CreateFilterBar();

            lblHint = new Label
            {
                Text = "Chỉ gán Manager cho nhà đã có khách đồng ý thuê (HĐ Active). Nhập UserID/Username → Tìm → chọn nhà → Gán.",
                Font = AppTypography.Caption,
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                MaximumSize = new Size(1000, 0),
                Margin = new Padding(0, 4, AppLayout.FieldGap, 8)
            };
            filter.Controls.Add(lblHint);

            cboHouse = new ComboBox();
            UIHelper.StyleCombo(cboHouse);
            filter.Controls.Add(UIHelper.CreateLabeledField("Nhà", cboHouse, 320));

            txtManagerQuery = new ModernTextBox
            {
                PlaceholderText = "VD: 4 hoặc manager"
            };
            txtManagerQuery.InputKeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await FindManagerAsync();
                }
            };
            filter.Controls.Add(UIHelper.CreateLabeledField("Manager (UserID hoặc Username)", txtManagerQuery, 220));

            var btnFind = UIHelper.SecondaryButton("Tìm", 90);
            btnFind.Margin = new Padding(0, 18, AppLayout.FieldGap, 6);
            btnFind.Click += async (s, e) => await FindManagerAsync();
            filter.Controls.Add(btnFind);

            lblManagerPreview = new Label
            {
                Text = "Chưa chọn Manager",
                Font = AppTypography.Body,
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                MaximumSize = new Size(280, 40),
                AutoEllipsis = true,
                Margin = new Padding(0, 22, AppLayout.FieldGap, 6)
            };
            filter.Controls.Add(lblManagerPreview);

            btnAssign = UIHelper.PrimaryButton("Gán Manager", 150);
            btnAssign.Margin = new Padding(0, 18, 0, 6);
            btnAssign.Click += async (s, e) => await AssignAsync();
            filter.Controls.Add(btnAssign);

            dgv = new ModernDataGridView();
            UIHelper.ApplyGridFill(dgv);
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "HouseName", HeaderText = "Nhà", FillWeight = 18 });
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

            // Composite top: header + wrapping filter so "Gán Manager" never clips off-screen
            var pageTop = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppColors.Card,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            pageTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            pageTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pageTop.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            header.Dock = DockStyle.Fill;
            filter.Dock = DockStyle.Fill;
            pageTop.Controls.Add(header, 0, 0);
            pageTop.Controls.Add(filter, 0, 1);

            UIHelper.WirePage(this, dgv, pageTop);
        }

        private async Task LoadAllAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var houses = scope.ServiceProvider.GetRequiredService<IHouseService>();
                var contracts = scope.ServiceProvider.GetRequiredService<IContractService>();
                var assignments = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                int landlordId = UserSession.CurrentUser!.UserID;

                var allHouses = (await houses.GetHousesByOwnerAsync(landlordId)).ToList();
                var eligibleHouseIds = (await contracts.GetContractsByLandlordAsync(landlordId))
                    .Where(c => string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase) && c.HouseID > 0)
                    .Select(c => c.HouseID)
                    .ToHashSet();

                var houseList = allHouses
                    .Where(h => eligibleHouseIds.Contains(h.HouseID))
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
                if (houseList.Count > 0)
                    cboHouse.SelectedIndex = 0;

                dgv.DataSource = (await assignments.GetByLandlordAsync(landlordId)).ToList();
                if (allHouses.Count == 0)
                    lblHint.Text = "Bạn chưa có nhà — tạo nhà → gán khách → chờ khách Đồng ý thuê → mới gán Manager.";
                else if (houseList.Count == 0)
                    lblHint.Text = "Chưa có nhà nào có HĐ Active. Gán khách và chờ khách bấm «Đồng ý thuê» rồi mới phân công Manager.";
                else
                    lblHint.Text = "Nhập UserID hoặc Username Manager → Tìm → chọn nhà (đã có khách thuê) → Gán. Demo: ID 4 / manager.";
                btnAssign.Enabled = houseList.Count > 0;
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

            string query = txtManagerQuery.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                lblManagerPreview.Text = "Nhập UserID hoặc Username";
                lblManagerPreview.ForeColor = AppColors.Danger;
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var users = scope.ServiceProvider.GetRequiredService<IUserService>();
                UserDto? user = null;

                if (int.TryParse(query, out int id) && id > 0)
                {
                    try { user = await users.GetUserByIdAsync(id); }
                    catch { /* not found */ }
                }

                if (user == null)
                {
                    user = (await users.GetUsersByRoleAsync(4))
                        .FirstOrDefault(u =>
                            string.Equals(u.Username, query, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(u.FullName, query, StringComparison.OrdinalIgnoreCase));
                }

                if (user == null)
                {
                    lblManagerPreview.Text = $"Không tìm thấy Manager: {query}";
                    lblManagerPreview.ForeColor = AppColors.Danger;
                    return;
                }

                if (user.RoleID != 4 && !string.Equals(user.RoleName, "Manager", StringComparison.OrdinalIgnoreCase))
                {
                    lblManagerPreview.Text = $"#{user.UserID} không phải Manager (Role: {user.RoleName})";
                    lblManagerPreview.ForeColor = AppColors.Danger;
                    return;
                }
                if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    lblManagerPreview.Text = $"Manager #{user.UserID} không Active";
                    lblManagerPreview.ForeColor = AppColors.Danger;
                    return;
                }

                _foundManager = user;
                lblManagerPreview.Text = $"✓ {user.FullName} (@{user.Username}) — ID {user.UserID}";
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
                AppDialog.ShowWarning("Hãy tìm Manager (UserID hoặc Username) trước khi gán.");
                return;
            }

            btnAssign.Enabled = false;
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
                AppDialog.ShowInfo($"Đã gán {_foundManager.FullName} cho nhà:\n{house.Display}");
                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
            finally
            {
                btnAssign.Enabled = cboHouse.Items.Count > 0;
            }
        }

        private async Task DgvClickAsync(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgv.Columns[e.ColumnIndex].Name != "DeactivateCol") return;
            if (dgv.Rows[e.RowIndex].DataBoundItem is not AssignmentDto item) return;
            if (!string.Equals(item.Status, "Active", StringComparison.OrdinalIgnoreCase)) return;
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
