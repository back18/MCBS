using MCBS.BlockForms.Utility;
using MCBS.Drawing;
using MCBS.Events;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.Game;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class IconSubtitleBox<TPixel> : ContainerControl<Control> where TPixel : unmanaged, IPixel<TPixel>
    {
        public IconSubtitleBox()
        {
            ClientSize = new(SR.DefaultFont.HalfWidth * 6, SR.DefaultFont.Height);

            _Spacing = 0;

            Icon_PictureBox = new();
            Text_SubtitleBox = new();
        }

        public readonly PictureBox<TPixel> Icon_PictureBox;

        public readonly SubtitleBox Text_SubtitleBox;

        public int Spacing
        {
            get => _Spacing;
            set
            {
                ThrowHelper.ArgumentOutOfMin(0, value, nameof(value));
                if (_Spacing != value)
                {
                    _Spacing = value;
                    RequestRedraw();
                }
            }
        }
        private int _Spacing;

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(Icon_PictureBox);
            Icon_PictureBox.BorderWidth = 0;
            Icon_PictureBox.AutoSize = false;
            Icon_PictureBox.ClientSize = new(16, 16);
            Icon_PictureBox.Skin.SetAllBackgroundColor(string.Empty);
            Icon_PictureBox.Resize += Icon_PictureBox_Resize;

            ChildControls.Add(Text_SubtitleBox);
            Text_SubtitleBox.ClientSize = new(ClientSize.Width - 16 - Spacing * 3, 16);
            Text_SubtitleBox.Stretch = Direction.Left;
            Text_SubtitleBox.Resize += Text_SubtitleBox_Resize;

            ActiveLayoutAll();
        }

        protected override void OnCursorEnter(Control sender, CursorEventArgs e)
        {
            base.OnCursorEnter(sender, e);

            Text_SubtitleBox.Play();
        }

        protected override void OnCursorLeave(Control sender, CursorEventArgs e)
        {
            base.OnCursorLeave(sender, e);

            Text_SubtitleBox.Pause();
            Text_SubtitleBox.Reset();
        }

        public override void ActiveLayoutAll()
        {
            Icon_PictureBox.LayoutVerticalCentered(this, Spacing);
            Text_SubtitleBox.LayoutVerticalCentered(this, Icon_PictureBox.RightLocation + 1);
        }

        private void Icon_PictureBox_Resize(Control sender, ValueChangedEventArgs<Size> e)
        {
            if (AutoSize)
                AutoSetSize();
        }

        private void Text_SubtitleBox_Resize(Control sender, ValueChangedEventArgs<Size> e)
        {
            if (AutoSize)
                AutoSetSize();
        }

        public override void AutoSetSize()
        {
            Size size = SR.DefaultFont.GetTotalSize(Text_SubtitleBox.Text);
            size.Width += Icon_PictureBox.Texture.ImageSource.Width;
            if (Icon_PictureBox.Texture.ImageSource.Height > size.Height)
            {
                size.Height = Math.Max(size.Height, Icon_PictureBox.Texture.ImageSource.Height);
            }
            size.Width += Spacing * 2;
            size.Height += Spacing * 2;
            ClientSize = size;
            ActiveLayoutAll();
        }
    }
}
