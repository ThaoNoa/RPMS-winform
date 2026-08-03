using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.House;
using RPMS.DTO.Post;
using RPMS.DTO.Room;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Landlord
{
    public class LandlordPostForm : Form
    {
        private readonly IPostService _postService;
        private readonly IHouseService _houseService;
        private readonly IRoomService _roomService;

        private ComboBox cboHouse = null!;
        private ComboBox cboRoom = null!;
        private ModernTextBox txtTitle = null!;
        private ModernTextBox txtPrice = null!;
        private TextBox txtDesc = null!;
        private ListBox lstImages = null!;
        private PictureBox picPreview = null!;
        private ModernButton btnPost = null!;
        private List<HouseDto> _houses = new();
        private List<RoomDto> _rooms = new();
        private readonly List<string> _imagePaths = new();

        public int RoomIdToPost { get; set; }

        public LandlordPostForm(IPostService postService, IHouseService houseService, IRoomService roomService)
        {
            _postService = postService;
            _houseService = houseService;
            _roomService = roomService;
            InitializeUI();
            Load += LandlordPostForm_Load!;
        }

        private void InitializeUI()
        {
            UIHelper.ApplyResizableDialog(this, new Size(640, 560));
            ClientSize = new Size(720, 640);
            BackColor = AppColors.Card;
            Text = "Tạo tin đăng cho thuê";
            StartPosition = FormStartPosition.CenterParent;
            AutoScroll = false;

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = AppColors.Card,
                Padding = new Padding(16, 10, 16, 10)
            };
            btnPost = new ModernButton
            {
                Text = "Gửi duyệt",
                Size = new Size(180, 44),
                BackColor = AppColors.Primary,
                Anchor = AnchorStyles.None
            };
            btnPost.Click += BtnPost_Click!;
            var flpBottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            flpBottom.Controls.Add(btnPost);
            pnlBottom.Controls.Add(flpBottom);

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
            void AddField(string labelText, Control input, float inputHeight = 0, bool multiline = false)
            {
                tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var lbl = new Label
                {
                    Text = labelText,
                    AutoSize = true,
                    ForeColor = AppColors.TextMuted,
                    Margin = new Padding(0, row == 0 ? 0 : 8, 0, 4)
                };
                tbl.Controls.Add(lbl, 0, row++);

                tbl.RowStyles.Add(inputHeight > 0
                    ? new RowStyle(SizeType.Absolute, inputHeight)
                    : new RowStyle(SizeType.AutoSize));
                input.Dock = DockStyle.Fill;
                if (input is ComboBox || input is ModernTextBox)
                    input.Margin = new Padding(0, 0, 0, 0);
                tbl.Controls.Add(input, 0, row++);
            }

            cboHouse = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTypography.Body,
                Height = 30
            };
            cboHouse.SelectedIndexChanged += async (s, e) => await LoadRoomsForSelectedHouseAsync();
            AddField("Chọn nhà *", cboHouse, 30);

            cboRoom = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTypography.Body,
                Height = 30
            };
            cboRoom.SelectedIndexChanged += CboRoom_SelectedIndexChanged;
            AddField("Chọn phòng trống *", cboRoom, 30);

            txtTitle = new ModernTextBox { PlaceholderText = "VD: Cho thuê phòng 101 giá tốt", Height = 35 };
            AddField("Tiêu đề tin đăng *", txtTitle, 35);

            txtPrice = new ModernTextBox { PlaceholderText = "3000000", Height = 35, Width = 250, Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right };
            AddField("Giá đăng (VNĐ) *", txtPrice, 35);

            txtDesc = new TextBox
            {
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                Font = AppTypography.Body,
                ScrollBars = ScrollBars.Vertical
            };
            AddField("Nội dung quảng cáo", txtDesc, 70);

            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tbl.Controls.Add(new Label
            {
                Text = "Ảnh tin đăng (có thể chọn nhiều — nếu trống sẽ dùng ảnh phòng)",
                AutoSize = true,
                ForeColor = AppColors.TextMuted,
                MaximumSize = new Size(680, 0),
                Margin = new Padding(0, 8, 0, 4)
            }, 0, row++);

            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            var tblImages = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            tblImages.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tblImages.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            lstImages = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = AppTypography.Body,
                Margin = new Padding(0, 0, 8, 0)
            };
            lstImages.SelectedIndexChanged += (s, e) =>
            {
                if (lstImages.SelectedIndex < 0 || lstImages.SelectedIndex >= _imagePaths.Count) return;
                ImagePathHelper.ApplyToPictureBox(picPreview, _imagePaths[lstImages.SelectedIndex]);
            };

            picPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = AppColors.Background,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(8, 0, 0, 0)
            };

            tblImages.Controls.Add(lstImages, 0, 0);
            tblImages.Controls.Add(picPreview, 1, 0);
            tbl.Controls.Add(tblImages, 0, row++);

            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var flpImgBtns = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 8, 0, 0)
            };
            var btnAddImg = new ModernButton
            {
                Text = "Thêm ảnh…",
                Size = new Size(120, 34),
                BackColor = AppColors.Primary,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnAddImg.Click += BtnAddImages_Click;
            var btnRemoveImg = new ModernButton
            {
                Text = "Xóa ảnh",
                Size = new Size(100, 34),
                BackColor = AppColors.Danger
            };
            btnRemoveImg.Click += (s, e) =>
            {
                if (lstImages.SelectedIndex < 0) return;
                _imagePaths.RemoveAt(lstImages.SelectedIndex);
                RefreshImageList();
            };
            flpImgBtns.Controls.AddRange(new Control[] { btnAddImg, btnRemoveImg });
            tbl.Controls.Add(flpImgBtns, 0, row++);

            root.Controls.Add(tbl);
            Controls.Add(root);
            Controls.Add(pnlBottom);
        }

        private void BtnAddImages_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.webp",
                Title = "Chọn nhiều ảnh cho tin đăng"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            foreach (var f in ofd.FileNames)
                _imagePaths.Add(f);
            RefreshImageList();
            if (lstImages.Items.Count > 0)
                lstImages.SelectedIndex = lstImages.Items.Count - 1;
        }

        private void RefreshImageList()
        {
            lstImages.Items.Clear();
            foreach (var p in _imagePaths)
                lstImages.Items.Add(Path.GetFileName(p));
            if (_imagePaths.Count == 0)
            {
                picPreview.Image?.Dispose();
                picPreview.Image = null;
            }
        }

        private async void LandlordPostForm_Load(object sender, EventArgs e)
        {
            try
            {
                var landlordId = UserSession.CurrentUser!.UserID;
                _houses = (await _houseService.GetHousesByOwnerAsync(landlordId)).ToList();
                cboHouse.DataSource = _houses;
                cboHouse.DisplayMember = "HouseName";
                cboHouse.ValueMember = "HouseID";

                if (_houses.Count == 0)
                {
                    btnPost.Enabled = false;
                    AppDialog.ShowInfo("Bạn chưa có nhà nào. Vui lòng thêm nhà và phòng trước.");
                    return;
                }

                await LoadRoomsForSelectedHouseAsync();
                if (RoomIdToPost > 0)
                    SelectRoomById(RoomIdToPost);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private async Task LoadRoomsForSelectedHouseAsync()
        {
            cboRoom.DataSource = null;
            if (cboHouse.SelectedValue == null || !int.TryParse(cboHouse.SelectedValue.ToString(), out int houseId))
                return;

            var rooms = await _roomService.GetRoomsByHouseAsync(houseId);
            _rooms = rooms.Where(r => r.Status == "Available").ToList();
            cboRoom.DataSource = _rooms;
            cboRoom.DisplayMember = "RoomNumber";
            cboRoom.ValueMember = "RoomID";
            btnPost.Enabled = _rooms.Count > 0;
        }

        private void SelectRoomById(int roomId)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomID == roomId);
            if (room != null)
                cboRoom.SelectedValue = roomId;
        }

        private void CboRoom_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboRoom.SelectedItem is RoomDto room)
            {
                RoomIdToPost = room.RoomID;
                if (string.IsNullOrWhiteSpace(txtPrice.Text) || txtPrice.Text == "0")
                    txtPrice.Text = room.Price.ToString("0");
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                    txtTitle.Text = $"Cho thuê phòng {room.RoomNumber}";
            }
        }

        private async void BtnPost_Click(object sender, EventArgs e)
        {
            if (cboRoom.SelectedValue == null || !int.TryParse(cboRoom.SelectedValue.ToString(), out int roomId))
            {
                AppDialog.ShowWarning("Vui lòng chọn phòng trống để đăng tin.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitle.Text) || !decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                AppDialog.ShowWarning("Vui lòng nhập tiêu đề và giá hợp lệ.");
                return;
            }

            try
            {
                btnPost.Enabled = false;
                var finalPaths = new List<string>();
                string uploadFolder = Path.Combine(Application.StartupPath, "uploads", "posts");
                Directory.CreateDirectory(uploadFolder);
                foreach (var original in _imagePaths)
                {
                    if (original.StartsWith("/"))
                    {
                        finalPaths.Add(original);
                        continue;
                    }
                    if (!File.Exists(original)) continue;
                    string fileName = $"post_{roomId}_{Guid.NewGuid():N}{Path.GetExtension(original)}";
                    string dest = Path.Combine(uploadFolder, fileName);
                    File.Copy(original, dest, true);
                    finalPaths.Add($"/uploads/posts/{fileName}");
                }

                await _postService.CreatePostAsync(new CreatePostDto
                {
                    RoomID = roomId,
                    Title = txtTitle.Text.Trim(),
                    Description = txtDesc.Text.Trim(),
                    PriceSnapshot = price,
                    ExpiryMonths = 1,
                    ImagePaths = finalPaths
                });
                AppDialog.ShowInfo("Đã gửi tin đăng! Vui lòng chờ Admin duyệt.", "Thành công");
                txtTitle.Text = "";
                txtDesc.Text = "";
                txtPrice.Text = "";
                _imagePaths.Clear();
                RefreshImageList();
                await LoadRoomsForSelectedHouseAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Lỗi: " + ex.Message);
            }
            finally
            {
                btnPost.Enabled = _rooms.Count > 0;
            }
        }
    }
}
