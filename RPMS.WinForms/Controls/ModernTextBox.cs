using RPMS.Common.Constants;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPMS.WinForms.Controls
{
    public class ModernTextBox : UserControl
    {
        private readonly TextBox _textBox;
        private Color _borderColor = AppColors.Border;
        private bool _isFocused;
        private string _placeholder = "";
        private bool _isPassword;

        public ModernTextBox()
        {
            DoubleBuffered = true;
            BackColor = AppColors.Card;
            Padding = new Padding(12, 10, 12, 10);
            Size = new Size(280, 40);
            Font = AppTypography.Body;

            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Font = AppTypography.Body,
                BackColor = AppColors.Card,
                ForeColor = AppColors.TextMain
            };
            _textBox.Enter += TextBox_Enter!;
            _textBox.Leave += TextBox_Leave!;
            _textBox.TextChanged += (s, e) => OnTextChanged(e);
            _textBox.KeyDown += (s, e) => InputKeyDown?.Invoke(this, e);
            Controls.Add(_textBox);
        }

        public event KeyEventHandler? InputKeyDown;

        public void FocusInput() => _textBox.Focus();

        public string PlaceholderText
        {
            get => _placeholder;
            set
            {
                _placeholder = value ?? "";
                ApplyPlaceholderIfNeeded();
            }
        }

        public bool UseSystemPasswordChar
        {
            get => _isPassword;
            set
            {
                _isPassword = value;
                if (!IsPlaceholderActive)
                    _textBox.UseSystemPasswordChar = value;
            }
        }

        public override string Text
        {
            get => IsPlaceholderActive ? "" : _textBox.Text;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    ApplyPlaceholderIfNeeded(force: true);
                }
                else
                {
                    ClearPlaceholderStyle();
                    _textBox.Text = value;
                }
            }
        }

        private bool IsPlaceholderActive =>
            !string.IsNullOrEmpty(_placeholder) &&
            _textBox.ForeColor == AppColors.TextMuted &&
            _textBox.Text == _placeholder;

        private void ApplyPlaceholderIfNeeded(bool force = false)
        {
            if (string.IsNullOrEmpty(_placeholder)) return;
            if (!force && _isFocused) return;
            if (!force && !string.IsNullOrEmpty(_textBox.Text) && !IsPlaceholderActive) return;

            _textBox.UseSystemPasswordChar = false;
            _textBox.ForeColor = AppColors.TextMuted;
            _textBox.Text = _placeholder;
        }

        private void ClearPlaceholderStyle()
        {
            _textBox.ForeColor = AppColors.TextMain;
            _textBox.UseSystemPasswordChar = _isPassword;
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            _isFocused = true;
            if (IsPlaceholderActive)
            {
                _textBox.Text = "";
                ClearPlaceholderStyle();
            }
            Invalidate();
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            _isFocused = false;
            if (string.IsNullOrWhiteSpace(_textBox.Text))
                ApplyPlaceholderIfNeeded(force: true);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var border = _isFocused ? AppColors.Primary : _borderColor;
            using var pen = new Pen(border, _isFocused ? 2f : 1f);
            using var path = UI.UIHelper.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 8);
            g.DrawPath(pen, path);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ApplyPlaceholderIfNeeded(force: true);
        }
    }
}
