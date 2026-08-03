using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.Common.Globals;
using RPMS.DTO.Auth;
using RPMS.DTO.Chat;
using RPMS.DTO.User;
using RPMS.WinForms.Controls;
using RPMS.WinForms.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.WinForms.Forms.Shared
{
    public class ChatForm : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ListBox lstConversations = null!;
        private FlowLayoutPanel flpMessages = null!;
        private ModernTextBox txtMessage = null!;
        private Label lblPeerTitle = null!;
        private Label lblEmpty = null!;
        private ComboBox? cboStartUser;
        private ModernButton btnSend = null!;
        private int _currentConversationId;
        private string _currentPeerName = "";
        private System.Windows.Forms.Timer _pollTimer = null!;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private bool _busy;
        private bool _suppressListEvent;
        private int _lastMessageCount = -1;
        private int _lastBubbleAreaWidth;
        private List<ChatMessageDto> _cachedMessages = new();

        public ChatForm(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            InitializeUI();
            Load += async (s, e) =>
            {
                await LoadPeersAsync();
                await LoadConversationsAsync();
            };
            FormClosed += (s, e) =>
            {
                try
                {
                    _pollTimer.Stop();
                    _pollTimer.Dispose();
                    _gate.Dispose();
                }
                catch { /* ignore */ }
            };
        }

        private async Task<T> WithChatAsync<T>(Func<IChatService, Task<T>> action)
        {
            using var scope = _scopeFactory.CreateScope();
            var chat = scope.ServiceProvider.GetRequiredService<IChatService>();
            return await action(chat);
        }

        private async Task WithChatAsync(Func<IChatService, Task> action)
        {
            using var scope = _scopeFactory.CreateScope();
            var chat = scope.ServiceProvider.GetRequiredService<IChatService>();
            await action(chat);
        }

        private async Task WithUserAsync(Func<IUserService, Task> action)
        {
            using var scope = _scopeFactory.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<IUserService>();
            await action(users);
        }

        private void InitializeUI()
        {
            UIHelper.ApplyFormStyle(this);
            MinimumSize = new Size(900, 520);
            Text = "Chat";
            ClientSize = new Size(1100, 680);
            DoubleBuffered = true;
            AutoScroll = false;

            // —— Cột trái ——
            var pnlLeft = new Panel { Dock = DockStyle.Left, Width = 300, BackColor = AppColors.Card };

            var pnlStart = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = AppColors.Card, Padding = new Padding(12) };
            pnlStart.Controls.Add(new Label
            {
                Text = "Bắt đầu chat với",
                Location = new Point(12, 10),
                AutoSize = true,
                ForeColor = AppColors.TextMuted,
                Font = new Font("Segoe UI", 9F)
            });
            cboStartUser = new ComboBox
            {
                Location = new Point(12, 34),
                Size = new Size(200, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTypography.Body
            };
            var btnStart = new ModernButton { Text = "Mở", Location = new Point(220, 32), Size = new Size(64, 32) };
            btnStart.Click += async (s, e) => await StartConversationAsync();
            pnlStart.Controls.Add(cboStartUser);
            pnlStart.Controls.Add(btnStart);

            var lblConvos = new Label
            {
                Text = "Hội thoại",
                Dock = DockStyle.Top,
                Height = 32,
                Padding = new Padding(12, 8, 0, 0),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                BackColor = AppColors.Card
            };

            lstConversations = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = AppTypography.Body,
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                ItemHeight = 44,
                DrawMode = DrawMode.OwnerDrawFixed
            };
            lstConversations.DrawItem += LstConversations_DrawItem;
            lstConversations.SelectedIndexChanged += async (s, e) => await OpenSelectedAsync();

            pnlLeft.Controls.Add(lstConversations);
            pnlLeft.Controls.Add(lblConvos);
            pnlLeft.Controls.Add(pnlStart);

            // —— Cột phải ——
            var pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.Background };

            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = AppColors.Card,
                Padding = new Padding(16, 0, 16, 0)
            };
            lblPeerTitle = new Label
            {
                Text = "Chọn hội thoại để bắt đầu",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = AppColors.TextMain,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblPeerTitle);
            pnlHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            // Input bar — TableLayout để không bị lệch khi resize
            var pnlInput = new Panel { Dock = DockStyle.Bottom, Height = 72, BackColor = AppColors.Card };
            pnlInput.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(pen, 0, 0, pnlInput.Width, 0);
            };
            var tblInput = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(12, 14, 12, 14)
            };
            tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));
            tblInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100f));

            txtMessage = new ModernTextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Nhập tin nhắn… (Enter để gửi)"
            };
            txtMessage.InputKeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await SendAsync();
                }
            };

            var btnImage = new ModernButton
            {
                Text = "Ảnh",
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 0, 0),
                BackColor = AppColors.TextMuted
            };
            btnImage.Click += BtnImage_Click;

            btnSend = new ModernButton
            {
                Text = "Gửi",
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 0, 0),
                BackColor = AppColors.Primary
            };
            btnSend.Click += async (s, e) => await SendAsync();

            tblInput.Controls.Add(txtMessage, 0, 0);
            tblInput.Controls.Add(btnImage, 1, 0);
            tblInput.Controls.Add(btnSend, 2, 0);
            pnlInput.Controls.Add(tblInput);

            flpMessages = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(16, 12, 16, 12),
                BackColor = AppColors.Background
            };
            flpMessages.Resize += (s, e) => RefreshBubbleWidthsIfNeeded();

            lblEmpty = new Label
            {
                Text = "Chưa có tin nhắn.\nHãy chọn người chat bên trái hoặc mở hội thoại có sẵn.",
                Font = new Font("Segoe UI", 11F),
                ForeColor = AppColors.TextMuted,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlRight.Controls.Add(flpMessages);
            pnlRight.Controls.Add(lblEmpty);
            pnlRight.Controls.Add(pnlInput);
            pnlRight.Controls.Add(pnlHeader);

            Controls.Add(pnlRight);
            Controls.Add(pnlLeft);

            _pollTimer = new System.Windows.Forms.Timer { Interval = 3500 };
            _pollTimer.Tick += async (s, e) =>
            {
                if (IsDisposed || _busy || _currentConversationId <= 0) return;
                try
                {
                    await LoadMessagesAsync(_currentConversationId, quiet: true);
                }
                catch { /* ignore poll */ }
            };
            _pollTimer.Start();
        }

        private void LstConversations_DrawItem(object? sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0 || e.Index >= lstConversations.Items.Count) return;
            if (lstConversations.Items[e.Index] is not ConversationListItem item) return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var bg = selected ? Color.FromArgb(219, 234, 254) : AppColors.Card;
            using (var brush = new SolidBrush(bg))
                e.Graphics.FillRectangle(brush, e.Bounds);

            var nameFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            var subFont = new Font("Segoe UI", 8.5F);
            var nameColor = AppColors.TextMain;
            var subColor = AppColors.TextMuted;

            string preview = string.IsNullOrWhiteSpace(item.LastMessage) ? "(Chưa có tin)" : item.LastMessage;
            if (preview.Length > 36) preview = preview[..36] + "…";

            e.Graphics.DrawString(item.PeerName + (item.UnreadCount > 0 ? $" ({item.UnreadCount})" : ""),
                nameFont, new SolidBrush(nameColor), e.Bounds.X + 12, e.Bounds.Y + 6);
            e.Graphics.DrawString(preview, subFont, new SolidBrush(subColor), e.Bounds.X + 12, e.Bounds.Y + 24);
            nameFont.Dispose();
            subFont.Dispose();
        }

        private static bool IsLandlord(LoginResponseDto me) =>
            me.RoleID == 2 || string.Equals(me.RoleName, "Landlord", StringComparison.OrdinalIgnoreCase);

        private static bool IsTenant(LoginResponseDto me) =>
            me.RoleID == 3 || string.Equals(me.RoleName, "Tenant", StringComparison.OrdinalIgnoreCase);

        private async Task LoadPeersAsync()
        {
            try
            {
                var me = UserSession.CurrentUser!;
                await WithUserAsync(async users =>
                {
                    if (IsLandlord(me))
                    {
                        var tenants = (await users.GetUsersByRoleAsync(3)).Where(u => u.Status == "Active").ToList();
                        if (IsDisposed) return;
                        BindPeers(tenants);
                    }
                    else if (IsTenant(me))
                    {
                        var landlords = (await users.GetUsersByRoleAsync(2)).Where(u => u.Status == "Active").ToList();
                        if (IsDisposed) return;
                        BindPeers(landlords);
                    }
                    else if (cboStartUser != null)
                    {
                        cboStartUser.Enabled = false;
                    }
                });
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Không tải danh sách chat: " + ex.Message);
            }
        }

        private void BindPeers(System.Collections.Generic.List<UserDto> peers)
        {
            cboStartUser!.DataSource = null;
            cboStartUser.DisplayMember = nameof(UserDto.FullName);
            cboStartUser.ValueMember = nameof(UserDto.UserID);
            cboStartUser.DataSource = peers;
            cboStartUser.Enabled = peers.Count > 0;
        }

        private async Task LoadConversationsAsync()
        {
            try
            {
                var list = await WithChatAsync(chat =>
                    chat.GetConversationsAsync(UserSession.CurrentUser!.UserID));
                if (IsDisposed) return;

                int keepId = _currentConversationId;
                _suppressListEvent = true;
                lstConversations.BeginUpdate();
                try
                {
                    lstConversations.Items.Clear();
                    foreach (var c in list)
                    {
                        int me = UserSession.CurrentUser!.UserID;
                        string peer = c.LandlordID == me ? c.TenantName : c.LandlordName;
                        lstConversations.Items.Add(new ConversationListItem
                        {
                            ConversationID = c.ConversationID,
                            PeerName = string.IsNullOrWhiteSpace(peer) ? "Người dùng" : peer,
                            LastMessage = c.LastMessage,
                            UnreadCount = c.UnreadCount
                        });
                    }

                    if (keepId > 0)
                    {
                        for (int i = 0; i < lstConversations.Items.Count; i++)
                        {
                            if (lstConversations.Items[i] is ConversationListItem it && it.ConversationID == keepId)
                            {
                                lstConversations.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    lstConversations.EndUpdate();
                    _suppressListEvent = false;
                }
                lstConversations.Invalidate();
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Không tải hội thoại: " + ex.Message);
            }
        }

        private async Task StartConversationAsync()
        {
            if (cboStartUser?.SelectedValue == null)
            {
                AppDialog.ShowWarning("Hãy chọn người để chat.");
                return;
            }

            // ComboBox ValueMember với List<object> đôi khi trả cả UserDto
            int peerId;
            if (cboStartUser.SelectedValue is int id)
                peerId = id;
            else if (cboStartUser.SelectedValue is UserDto u)
                peerId = u.UserID;
            else if (!int.TryParse(cboStartUser.SelectedValue.ToString(), out peerId))
            {
                AppDialog.ShowWarning("Không xác định được người nhận.");
                return;
            }

            var me = UserSession.CurrentUser!;
            int landlordId = IsLandlord(me) ? me.UserID : peerId;
            int tenantId = IsTenant(me) ? me.UserID : peerId;

            if (!IsLandlord(me) && !IsTenant(me))
            {
                AppDialog.ShowWarning("Chỉ Chủ nhà và Người thuê dùng được Chat.");
                return;
            }

            try
            {
                _busy = true;
                var convo = await WithChatAsync(chat =>
                    chat.GetOrCreateConversationAsync(landlordId, tenantId));
                _currentConversationId = convo.ConversationID;
                _currentPeerName = IsLandlord(me) ? convo.TenantName : convo.LandlordName;
                lblPeerTitle.Text = _currentPeerName;
                await LoadConversationsAsync();
                await LoadMessagesAsync(convo.ConversationID);
                txtMessage.FocusInput();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError(ex.Message);
            }
            finally
            {
                _busy = false;
            }
        }

        private async Task OpenSelectedAsync()
        {
            if (_busy || _suppressListEvent) return;
            if (lstConversations.SelectedItem is not ConversationListItem item) return;
            if (item.ConversationID == _currentConversationId && flpMessages.Controls.Count > 0)
                return;

            try
            {
                _busy = true;
                _currentConversationId = item.ConversationID;
                _currentPeerName = item.PeerName;
                lblPeerTitle.Text = item.PeerName;
                await LoadMessagesAsync(_currentConversationId);
                await WithChatAsync(chat =>
                    chat.MarkConversationReadAsync(_currentConversationId, UserSession.CurrentUser!.UserID));
                await LoadConversationsAsync();
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    AppDialog.ShowError("Không mở hội thoại: " + ex.Message);
            }
            finally
            {
                _busy = false;
            }
        }

        private async Task LoadMessagesAsync(int conversationId, bool quiet = false)
        {
            if (IsDisposed || conversationId <= 0) return;

            System.Collections.Generic.IEnumerable<ChatMessageDto> messages;
            try
            {
                messages = await WithChatAsync(chat =>
                    chat.GetMessagesAsync(conversationId, UserSession.CurrentUser!.UserID));
            }
            catch
            {
                if (!quiet) throw;
                return;
            }

            if (IsDisposed || conversationId != _currentConversationId) return;

            var list = messages.ToList();
            // Poll: bỏ qua nếu không đổi
            if (quiet && list.Count == _lastMessageCount) return;
            _lastMessageCount = list.Count;
            _cachedMessages = list;
            RenderMessages(list);
        }

        private void RefreshBubbleWidthsIfNeeded()
        {
            if (_cachedMessages.Count == 0 || _currentConversationId <= 0) return;
            int areaWidth = Math.Max(280, flpMessages.ClientSize.Width - 36);
            if (areaWidth == _lastBubbleAreaWidth) return;
            RenderMessages(_cachedMessages);
        }

        private void RenderMessages(IReadOnlyList<ChatMessageDto> list)
        {
            int areaWidth = Math.Max(280, flpMessages.ClientSize.Width - 36);
            _lastBubbleAreaWidth = areaWidth;

            flpMessages.SuspendLayout();
            flpMessages.Controls.Clear();

            lblEmpty.Visible = list.Count == 0;
            flpMessages.Visible = list.Count > 0;

            foreach (var msg in list)
            {
                flpMessages.Controls.Add(CreateBubble(msg, areaWidth));
            }
            flpMessages.ResumeLayout();

            if (flpMessages.Controls.Count > 0)
            {
                flpMessages.ScrollControlIntoView(flpMessages.Controls[flpMessages.Controls.Count - 1]);
            }
        }

        private Control CreateBubble(ChatMessageDto msg, int areaWidth)
        {
            int bubbleMax = Math.Min(420, Math.Max(180, areaWidth * 2 / 3));
            bool mine = msg.IsMine;

            var row = new Panel
            {
                Width = areaWidth,
                Margin = new Padding(0, 4, 0, 4),
                BackColor = Color.Transparent
            };

            string body = string.IsNullOrWhiteSpace(msg.Content) ? "" : msg.Content.Trim();
            string meta = $"{(mine ? "Bạn" : msg.SenderName)} · {msg.CreatedDate:HH:mm dd/MM}";

            var lblMeta = new Label
            {
                Text = meta,
                AutoSize = true,
                Font = new Font("Segoe UI", 8F),
                ForeColor = AppColors.TextMuted
            };

            var lblBody = new Label
            {
                Text = string.IsNullOrEmpty(body) ? (string.IsNullOrEmpty(msg.ImagePath) ? "" : "[Hình ảnh]") : body,
                AutoSize = true,
                MaximumSize = new Size(bubbleMax - 24, 0),
                Font = new Font("Segoe UI", 10F),
                ForeColor = AppColors.TextMain,
                Padding = new Padding(0)
            };

            int contentH = TextRenderer.MeasureText(
                lblBody.Text,
                lblBody.Font,
                new Size(bubbleMax - 24, int.MaxValue),
                TextFormatFlags.WordBreak).Height;
            contentH = Math.Max(20, contentH);

            bool hasImage = !string.IsNullOrEmpty(msg.ImagePath);
            int imgH = hasImage ? 120 : 0;
            int bubbleH = 28 + contentH + imgH + 8;
            int bubbleW = Math.Min(bubbleMax, Math.Max(120, lblBody.PreferredWidth + 28));

            var bubble = new Panel
            {
                Size = new Size(bubbleW, bubbleH),
                BackColor = mine ? Color.FromArgb(219, 234, 254) : Color.White
            };
            bubble.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = UIHelper.RoundedRect(new Rectangle(0, 0, bubble.Width - 1, bubble.Height - 1), 12);
                using var brush = new SolidBrush(mine ? Color.FromArgb(219, 234, 254) : Color.White);
                e.Graphics.FillPath(brush, path);
                using var pen = new Pen(mine ? Color.FromArgb(147, 197, 253) : AppColors.Border);
                e.Graphics.DrawPath(pen, path);
            };

            lblMeta.Location = new Point(12, 6);
            lblBody.Location = new Point(12, 24);
            bubble.Controls.Add(lblMeta);
            bubble.Controls.Add(lblBody);

            if (hasImage)
            {
                var pic = new PictureBox
                {
                    Location = new Point(12, 28 + contentH),
                    Size = new Size(bubbleW - 24, 110),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.FromArgb(243, 244, 246)
                };
                var path = msg.ImagePath!.StartsWith("/")
                    ? Path.Combine(Application.StartupPath, msg.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))
                    : msg.ImagePath;
                if (File.Exists(path))
                    pic.ImageLocation = path;
                bubble.Controls.Add(pic);
            }

            row.Height = bubbleH + 4;
            bubble.Location = mine
                ? new Point(Math.Max(0, areaWidth - bubbleW - 8), 0)
                : new Point(0, 0);
            row.Controls.Add(bubble);
            return row;
        }

        private async Task SendAsync(string? imagePath = null)
        {
            if (_currentConversationId <= 0)
            {
                AppDialog.ShowWarning("Hãy chọn hoặc mở hội thoại trước.");
                return;
            }

            string content = txtMessage.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(imagePath))
            {
                AppDialog.ShowWarning("Nhập nội dung tin nhắn.");
                txtMessage.FocusInput();
                return;
            }

            if (!await _gate.WaitAsync(0)) return;
            _busy = true;
            btnSend.Enabled = false;
            try
            {
                await WithChatAsync(chat => chat.SendMessageAsync(new SendMessageDto
                {
                    ConversationID = _currentConversationId,
                    SenderID = UserSession.CurrentUser!.UserID,
                    Content = content,
                    ImagePath = imagePath
                }));
                txtMessage.Text = "";
                _lastMessageCount = -1;
                await LoadMessagesAsync(_currentConversationId);
                await LoadConversationsAsync();
                txtMessage.FocusInput();
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Không gửi được tin: " + ex.Message);
            }
            finally
            {
                btnSend.Enabled = true;
                _busy = false;
                _gate.Release();
            }
        }

        private async void BtnImage_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.webp" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            try
            {
                string folder = Path.Combine(Application.StartupPath, "uploads", "chat");
                Directory.CreateDirectory(folder);
                string fileName = $"chat_{Guid.NewGuid():N}{Path.GetExtension(ofd.FileName)}";
                string dest = Path.Combine(folder, fileName);
                File.Copy(ofd.FileName, dest, true);
                await SendAsync($"/uploads/chat/{fileName}");
            }
            catch (Exception ex)
            {
                AppDialog.ShowError("Không gửi ảnh: " + ex.Message);
            }
        }

        private class ConversationListItem
        {
            public int ConversationID { get; set; }
            public string PeerName { get; set; } = "";
            public string LastMessage { get; set; } = "";
            public int UnreadCount { get; set; }
            public override string ToString() => PeerName;
        }
    }
}
