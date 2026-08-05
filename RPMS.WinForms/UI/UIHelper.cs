using RPMS.Common.Constants;
using RPMS.WinForms.Controls;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace RPMS.WinForms.UI
{
    public static class UIHelper
    {
        public static void ApplyFormStyle(Form form)
        {
            form.BackColor = AppColors.Background;
            form.Font = AppTypography.Body;
            form.ForeColor = AppColors.TextMain;
            if (form.FormBorderStyle is FormBorderStyle.FixedDialog
                or FormBorderStyle.FixedSingle
                or FormBorderStyle.FixedToolWindow)
            {
                form.FormBorderStyle = FormBorderStyle.Sizable;
            }
            form.MaximizeBox = true;
            form.MinimizeBox = true;
            if (form.MinimumSize.Width < AppLayout.PageMin.Width)
                form.MinimumSize = AppLayout.PageMin;
        }

        public static void ApplyResizableDialog(Form form, Size? minSize = null)
        {
            ApplyFormStyle(form);
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MaximizeBox = true;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimumSize = minSize ?? AppLayout.DialogMin;
        }

        /// <summary>Header trang: tiêu đề trái + cụm nút phải (không đè khi resize).</summary>
        public static Panel CreatePageHeader(string title, params Control[] trailingActions)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = AppLayout.PageHeaderHeight,
                BackColor = AppColors.Card,
                Padding = new Padding(AppLayout.PagePadding, 8, AppLayout.PagePadding, 8)
            };
            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(pen, 0, panel.Height - 1, panel.Width, panel.Height - 1);
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var lbl = new Label
            {
                Text = title,
                Font = AppTypography.Heading,
                ForeColor = AppColors.TextMain,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            var actions = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            foreach (var c in trailingActions.Where(x => x != null))
            {
                c.Margin = new Padding(6, 2, 0, 2);
                if (c is ModernButton btn && btn.Height < AppLayout.ButtonHeight)
                    btn.Height = AppLayout.ButtonHeight;
                actions.Controls.Add(c);
            }

            layout.Controls.Add(lbl, 0, 0);
            layout.Controls.Add(actions, 1, 0);
            panel.Controls.Add(layout);
            panel.Tag = lbl; // để form lấy Label tiêu đề: GetPageHeaderTitle(panel)
            return panel;
        }

        public static Label GetPageHeaderTitle(Panel header)
        {
            if (header.Tag is Label tagged)
                return tagged;
            var nested = header.Controls.OfType<TableLayoutPanel>()
                .SelectMany(t => t.Controls.OfType<Label>())
                .FirstOrDefault();
            if (nested != null) return nested;
            return header.Controls.OfType<Label>().FirstOrDefault()
                ?? throw new InvalidOperationException("Page header không có Label tiêu đề.");
        }

        /// <summary>Thanh lọc: FlowLayout wrap — không absolute X.</summary>
        public static FlowLayoutPanel CreateFilterBar(int height = 0)
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = height > 0 ? height : AppLayout.ToolbarHeight,
                AutoSize = height <= 0,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = AppColors.Card,
                Padding = new Padding(AppLayout.PagePadding, 10, AppLayout.PagePadding, 10),
                MinimumSize = new Size(0, 56)
            };
        }

        /// <summary>Khối label + control (cho FlowLayout filter).</summary>
        public static Panel CreateLabeledField(string label, Control input, int width)
        {
            var wrap = new Panel
            {
                Width = width,
                Height = 58,
                Margin = new Padding(0, 0, AppLayout.FieldGap, 6)
            };
            var lbl = new Label
            {
                Text = label,
                Font = AppTypography.Caption,
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            input.Location = new Point(0, 18);
            input.Width = width;
            if (input is ModernTextBox)
                input.Height = AppLayout.InputHeight;
            else if (input is ComboBox)
                input.Height = AppLayout.ComboHeight;
            wrap.Controls.Add(lbl);
            wrap.Controls.Add(input);
            return wrap;
        }

        /// <summary>
        /// Khối label + input kéo giãn theo cột TableLayout (dialog Thêm/Sửa).
        /// </summary>
        public static Panel CreateDialogField(string label, Control input, int minHeight = 62)
        {
            var wrap = new Panel
            {
                Dock = DockStyle.Fill,
                MinimumSize = new Size(80, minHeight),
                Height = minHeight,
                Margin = new Padding(0, 0, AppLayout.FieldGap, 10),
                Padding = new Padding(0)
            };
            var lbl = new Label
            {
                Text = label,
                Font = AppTypography.Caption,
                ForeColor = AppColors.TextMuted,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            if (input is ModernTextBox)
                input.Height = AppLayout.InputHeight;
            else if (input is ComboBox)
            {
                input.Height = AppLayout.ComboHeight;
                StyleCombo((ComboBox)input);
            }

            void LayoutInput()
            {
                int top = 20;
                input.Location = new Point(0, top);
                input.Width = Math.Max(40, wrap.ClientSize.Width);
                if (input is TextBox tb && tb.Multiline)
                    input.Height = Math.Max(80, wrap.ClientSize.Height - top);
            }

            wrap.Controls.Add(lbl);
            wrap.Controls.Add(input);
            wrap.Resize += (_, _) => LayoutInput();
            LayoutInput();
            return wrap;
        }

        public static Panel CreateSideFormPanel(int width = 0)
        {
            return new Panel
            {
                Dock = DockStyle.Right,
                Width = width > 0 ? width : AppLayout.SidePanelWidth,
                MinimumSize = new Size(300, 0),
                BackColor = AppColors.Card,
                Padding = new Padding(AppLayout.PagePadding),
                AutoScroll = true
            };
        }

        public static void WireListPage(Form form, Control? header, Control content)
        {
            ApplyFormStyle(form);
            form.AutoScroll = false;
            if (header != null)
            {
                header.Dock = DockStyle.Top;
                if (header.Height < 48) header.Height = AppLayout.PageHeaderHeight;
            }
            content.Dock = DockStyle.Fill;
            if (content is DataGridView dgv)
                ApplyGridFill(dgv);
        }

        /// <summary>
        /// Trang: content Fill + optional side + header. Add order: Fill → Side → Header.
        /// </summary>
        public static void WirePage(Form form, Control content, Control? header = null, Control? sidePanel = null)
        {
            ApplyFormStyle(form);
            form.AutoScroll = false;
            form.Controls.Clear();
            content.Dock = DockStyle.Fill;
            form.Controls.Add(content);
            if (sidePanel != null)
            {
                if (sidePanel.Dock != DockStyle.Left && sidePanel.Dock != DockStyle.Right)
                    sidePanel.Dock = DockStyle.Right;
                form.Controls.Add(sidePanel);
            }
            if (header != null)
            {
                header.Dock = DockStyle.Top;
                form.Controls.Add(header);
            }
        }

        public static void ApplyGridFill(DataGridView dgv)
        {
            dgv.Dock = DockStyle.Fill;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AutoGenerateColumns = false;
        }

        public static void SoftAnchorDialogControls(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                // Không đụng ModernTextBox (inner TextBox đã Dock=Fill) — Anchor sẽ phá layout nhập liệu
                if (c is ModernTextBox)
                    continue;

                SoftAnchorDialogControls(c);
                if (c is TextBox or RichTextBox or ComboBox or ListBox or CheckedListBox)
                    c.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                else if (c is PictureBox pic)
                {
                    pic.SizeMode = PictureBoxSizeMode.Zoom;
                    pic.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                }
                else if (c is DataGridView dgv)
                {
                    dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                else if (c is Button)
                {
                    if (parent is Form f && c.Top > f.ClientSize.Height * 0.7)
                        c.Anchor = AnchorStyles.Bottom | (c.Left > f.ClientSize.Width / 2 ? AnchorStyles.Right : AnchorStyles.Left);
                }
                // Không đổi Anchor Panel đã Dock (footer/header/body)
            }
        }

        /// <summary>Footer dialog: nút căn phải, không absolute Y.</summary>
        public static Panel CreateDialogFooter(params Control[] buttons)
        {
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = AppColors.Card,
                Padding = new Padding(AppLayout.PagePadding, 10, AppLayout.PagePadding, 10)
            };
            footer.Paint += (s, e) =>
            {
                using var pen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
            };
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            foreach (var b in buttons.Reverse())
            {
                b.Margin = new Padding(8, 0, 0, 0);
                flow.Controls.Add(b);
            }
            footer.Controls.Add(flow);
            return footer;
        }

        public static void ApplyCardStyle(Panel panel)
        {
            panel.BackColor = AppColors.Card;
            panel.Padding = new Padding(AppLayout.PagePadding);
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void DrawCardBorder(Graphics g, Rectangle bounds, int radius = 8)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = RoundedRect(bounds, radius);
            using var pen = new Pen(AppColors.Border);
            g.DrawPath(pen, path);
        }

        public static Label CreateTitleLabel(string text) => new()
        {
            Text = text,
            Font = AppTypography.Subtitle,
            ForeColor = AppColors.TextMain,
            AutoSize = true
        };

        public static Label CreateMutedLabel(string text) => new()
        {
            Text = text,
            Font = AppTypography.Body,
            ForeColor = AppColors.TextMuted,
            AutoSize = true
        };

        public static Label CreateFieldLabel(string text) => new()
        {
            Text = text,
            Font = AppTypography.Caption,
            ForeColor = AppColors.TextMuted,
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 2)
        };

        public static void StyleCombo(ComboBox cbo)
        {
            cbo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo.Font = AppTypography.Body;
            cbo.FlatStyle = FlatStyle.Flat;
            cbo.Height = AppLayout.ComboHeight;
        }

        public static ModernButton PrimaryButton(string text, int width = 0) => new()
        {
            Text = text,
            Size = new Size(width > 0 ? width : AppLayout.ButtonMinWidth, AppLayout.ButtonHeight),
            BackColor = AppColors.Primary
        };

        public static ModernButton SecondaryButton(string text, int width = 0) => new()
        {
            Text = text,
            Size = new Size(width > 0 ? width : AppLayout.ButtonMinWidth, AppLayout.ButtonHeight),
            BackColor = AppColors.TextMuted
        };

        public static ModernButton DangerOutlineButton(string text, int width = 0) => new()
        {
            Text = text,
            Size = new Size(width > 0 ? width : AppLayout.ButtonMinWidth, AppLayout.ButtonHeight),
            BackColor = AppColors.Card,
            ForeColor = AppColors.Danger
        };
    }
}
