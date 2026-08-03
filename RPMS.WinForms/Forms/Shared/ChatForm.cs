using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Chat;
using RPMS.DTO.User;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Shared
{
    public class ChatForm : Form
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private ListBox lstConversations = null!;
        private FlowLayoutPanel flpMessages = null!;
        private ModernTextBox txtMessage = null!;
        private ComboBox? cboStartUser;
        private int _currentConversationId;
        private System.Windows.Forms.Timer _pollTimer = null!;

        public ChatForm(IChatService chatService, IUserService userService)
        {
            _chatService = chatService;
            _userService = userService;
            InitializeUI();
            Load += async (s, e) => await LoadConversationsAsync();
            FormClosed += (s, e) => _pollTimer.Stop();
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            Text = "Chat";
            ClientSize = new Size(1100, 650);

            var pnlLeft = new Panel { Dock = DockStyle.Left, Width = 280, BackColor = AppColors.Card };
            lstConversations = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = AppTypography.Body,
                BorderStyle = BorderStyle.None,
                DisplayMember = "Display"
            };
            lstConversations.SelectedIndexChanged += async (s, e) => await OpenSelectedAsync();

            var pnlStart = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = AppColors.Card, Padding = new Padding(10) };
            pnlStart.Controls.Add(new Label { Text = "Bắt đầu chat với", Location = new Point(10, 8), AutoSize = true, ForeColor = AppColors.TextMuted });
            cboStartUser = new ComboBox { Location = new Point(10, 30), Size = new Size(180, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            var btnStart = new ModernButton { Text = "Mở", Location = new Point(200, 28), Size = new Size(60, 32) };
            btnStart.Click += async (s, e) => await StartConversationAsync();
            pnlStart.Controls.Add(cboStartUser);
            pnlStart.Controls.Add(btnStart);
            pnlLeft.Controls.Add(lstConversations);
            pnlLeft.Controls.Add(pnlStart);

            var pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.Background };
            flpMessages = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(12)
            };

            var pnlInput = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = AppColors.Card };
            txtMessage = new ModernTextBox { Location = new Point(20, 18), Size = new Size(620, 35) };
            var btnImage = new ModernButton { Text = "Ảnh", Location = new Point(660, 18), Size = new Size(70, 35), BackColor = AppColors.TextMuted };
            btnImage.Click += BtnImage_Click;
            var btnSend = new ModernButton { Text = "Gửi", Location = new Point(740, 18), Size = new Size(90, 35) };
            btnSend.Click += async (s, e) => await SendAsync();
            pnlInput.Controls.AddRange(new Control[] { txtMessage, btnImage, btnSend });

            pnlRight.Controls.Add(flpMessages);
            pnlRight.Controls.Add(pnlInput);

            Controls.Add(pnlRight);
            Controls.Add(pnlLeft);

            _pollTimer = new System.Windows.Forms.Timer { Interval = 4000 };
            _pollTimer.Tick += async (s, e) =>
            {
                if (_currentConversationId > 0)
                    await LoadMessagesAsync(_currentConversationId);
            };
            _pollTimer.Start();

            Load += async (s, e) => await LoadPeersAsync();
        }

        private async System.Threading.Tasks.Task LoadPeersAsync()
        {
            var me = UserSession.CurrentUser!;
            if (me.RoleID == 2)
            {
                var tenants = (await _userService.GetUsersByRoleAsync(3)).Where(u => u.Status == "Active").ToList();
                cboStartUser!.DataSource = tenants;
            }
            else if (me.RoleID == 3)
            {
                var landlords = (await _userService.GetUsersByRoleAsync(2)).Where(u => u.Status == "Active").ToList();
                cboStartUser!.DataSource = landlords;
            }
            else
            {
                cboStartUser!.Enabled = false;
            }
            cboStartUser.DisplayMember = nameof(UserDto.FullName);
            cboStartUser.ValueMember = nameof(UserDto.UserID);
        }

        private async System.Threading.Tasks.Task LoadConversationsAsync()
        {
            var list = (await _chatService.GetConversationsAsync(UserSession.CurrentUser!.UserID)).ToList();
            var me = UserSession.CurrentUser.UserID;
            lstConversations.Items.Clear();
            foreach (var c in list)
            {
                string peer = c.LandlordID == me ? c.TenantName : c.LandlordName;
                string badge = c.UnreadCount > 0 ? $" ({c.UnreadCount})" : "";
                lstConversations.Items.Add(new ConversationListItem
                {
                    ConversationID = c.ConversationID,
                    Display = $"{peer}{badge} — {c.LastMessage}"
                });
            }
        }

        private async System.Threading.Tasks.Task StartConversationAsync()
        {
            if (cboStartUser?.SelectedValue == null) return;
            var me = UserSession.CurrentUser!;
            int peerId = Convert.ToInt32(cboStartUser.SelectedValue);
            int landlordId = me.RoleID == 2 ? me.UserID : peerId;
            int tenantId = me.RoleID == 3 ? me.UserID : peerId;
            try
            {
                var convo = await _chatService.GetOrCreateConversationAsync(landlordId, tenantId);
                await LoadConversationsAsync();
                _currentConversationId = convo.ConversationID;
                await LoadMessagesAsync(convo.ConversationID);
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async System.Threading.Tasks.Task OpenSelectedAsync()
        {
            if (lstConversations.SelectedItem is not ConversationListItem item) return;
            _currentConversationId = item.ConversationID;
            await LoadMessagesAsync(_currentConversationId);
            await _chatService.MarkConversationReadAsync(_currentConversationId, UserSession.CurrentUser!.UserID);
            await LoadConversationsAsync();
        }

        private async System.Threading.Tasks.Task LoadMessagesAsync(int conversationId)
        {
            var messages = await _chatService.GetMessagesAsync(conversationId, UserSession.CurrentUser!.UserID);
            flpMessages.SuspendLayout();
            flpMessages.Controls.Clear();
            foreach (var msg in messages)
            {
                var bubble = new Panel
                {
                    Width = flpMessages.ClientSize.Width - 40,
                    Height = string.IsNullOrEmpty(msg.ImagePath) ? 70 : 180,
                    Margin = new Padding(4),
                    BackColor = msg.IsMine ? Color.FromArgb(219, 234, 254) : AppColors.Card
                };
                var lbl = new Label
                {
                    Text = $"{msg.SenderName} • {msg.CreatedDate:HH:mm dd/MM}\n{msg.Content}",
                    Dock = DockStyle.Fill,
                    Padding = new Padding(8),
                    ForeColor = AppColors.TextMain
                };
                bubble.Controls.Add(lbl);
                if (!string.IsNullOrEmpty(msg.ImagePath))
                {
                    var pic = new PictureBox
                    {
                        Dock = DockStyle.Bottom,
                        Height = 110,
                        SizeMode = PictureBoxSizeMode.Zoom
                    };
                    var path = msg.ImagePath.StartsWith("/")
                        ? Path.Combine(Application.StartupPath, msg.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))
                        : msg.ImagePath;
                    if (File.Exists(path)) pic.ImageLocation = path;
                    bubble.Controls.Add(pic);
                }
                flpMessages.Controls.Add(bubble);
            }
            flpMessages.ResumeLayout();
            if (flpMessages.Controls.Count > 0)
                flpMessages.ScrollControlIntoView(flpMessages.Controls[flpMessages.Controls.Count - 1]);
        }

        private async System.Threading.Tasks.Task SendAsync(string? imagePath = null)
        {
            if (_currentConversationId <= 0)
            {
                AppDialog.ShowWarning("Hãy chọn hoặc tạo hội thoại trước.");
                return;
            }
            try
            {
                await _chatService.SendMessageAsync(new SendMessageDto
                {
                    ConversationID = _currentConversationId,
                    SenderID = UserSession.CurrentUser!.UserID,
                    Content = txtMessage.Text,
                    ImagePath = imagePath
                });
                txtMessage.Text = "";
                await LoadMessagesAsync(_currentConversationId);
                await LoadConversationsAsync();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
        }

        private async void BtnImage_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Image|*.jpg;*.jpeg;*.png" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            string folder = Path.Combine(Application.StartupPath, "uploads", "chat");
            Directory.CreateDirectory(folder);
            string fileName = $"chat_{Guid.NewGuid():N}{Path.GetExtension(ofd.FileName)}";
            string dest = Path.Combine(folder, fileName);
            File.Copy(ofd.FileName, dest, true);
            await SendAsync($"/uploads/chat/{fileName}");
        }

        private class ConversationListItem
        {
            public int ConversationID { get; set; }
            public string Display { get; set; } = "";
            public override string ToString() => Display;
        }
    }
}
