﻿using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class ButtonWire : PictureBox, IDisposable, IAlignSize
    {
        private static readonly ReadOnlyDictionary<ThemeButtonWire, Image> s_imageButton;
        private static readonly Image s_wire;

        private readonly Label _leftBarrier;
        private readonly Label _rightBarrier;

        private Rectangle _imageRectangle;
        private Rectangle _backgroundRectangle;
        private Color _foreColor;
        private ThemeButtonWire _theme;

        static ButtonWire()
        {
            s_imageButton = Painter.GetThemeImages<ThemeButtonWire>(Resources.ButtonWire);
            s_wire = Resources.Wire;
        }

        public ButtonWire()
        {
            BackColor = Color.Transparent;
            Dock = DockStyle.Fill;

            _foreColor = Color.White;
            _theme = ThemeButtonWire.Blue;

            _leftBarrier = new Label();
            _rightBarrier = new Label();

            _leftBarrier.Dock = DockStyle.Left;
            _rightBarrier.Dock = DockStyle.Right;

            Controls.Add(_leftBarrier);
            Controls.Add(_rightBarrier);
        }

        public void AlignSize()
        {
            var sizeImage = s_imageButton[ThemeButtonWire.Blue].Size;

            var wFactor = (float)sizeImage.Width / ClientRectangle.Width;
            var hFactor = (float)sizeImage.Height / ClientRectangle.Height;

            var resizeFactor = Math.Max(wFactor, hFactor);
            var sizeRect = new Size((int)(sizeImage.Width / resizeFactor), (int)(sizeImage.Height / resizeFactor));

            var x = ClientRectangle.Width - sizeRect.Width >> 1;
            var y = ClientRectangle.Height - sizeRect.Height >> 1;

            _imageRectangle = new Rectangle(x, y, sizeRect.Width, sizeRect.Height);
            _backgroundRectangle = new Rectangle(0, y, ClientRectangle.Width, sizeRect.Height);

            _rightBarrier.Size = _leftBarrier.Size = new Size(ClientRectangle.Width - _imageRectangle.Width >> 1, ClientRectangle.Height);

            Font?.Dispose();
            Font = FontManager.CreateFont(GameFont.Arial, 0.45f * ClientRectangle.Height, FontStyle.Bold);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _leftBarrier.Dispose();
                _rightBarrier.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(s_wire, _backgroundRectangle);
            e.Graphics.DrawImage(s_imageButton[_theme], _imageRectangle);

            TextRenderer.DrawText(e.Graphics, Text, Font, _imageRectangle, _foreColor);
        }

        private void SetStyle(ThemeButtonWire theme, Color foreColor)
        {
            _theme = theme;
            _foreColor = foreColor;

            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) =>
            SetStyle(ThemeButtonWire.Orange, Color.Black);

        protected override void OnMouseLeave(EventArgs e) =>
            SetStyle(ThemeButtonWire.Blue, Color.White);

        protected override void OnMouseDown(MouseEventArgs e) =>
            SetStyle(ThemeButtonWire.Green, Color.Black);

        protected override void OnMouseUp(MouseEventArgs e) =>
            SetStyle(ThemeButtonWire.Orange, Color.Black);

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (Enabled)
                SetStyle(ThemeButtonWire.Blue, Color.White);
            else
                SetStyle(ThemeButtonWire.Gray, Color.Black);
        }
    }
}