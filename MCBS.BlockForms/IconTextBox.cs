using MCBS;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Core.Events;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class IconTextBox<TPixel> : ContainerControl<Control> where TPixel : unmanaged, IPixel<TPixel>
    {
        public IconTextBox()
        {
            Icon_PictureBox = new();
            Icon_PictureBox.BorderWidth = 0;
            Icon_PictureBox.ClientSize = new(16, 16);
            Icon_PictureBox.Skin.SetAllBackgroundColor(string.Empty);

            Text_Label = new();

            _Spacing = 0;

            ClientSize = new(SR.DefaultFont.HalfWidth * 6, SR.DefaultFont.Height);
        }

        public readonly PictureBox<TPixel> Icon_PictureBox;

        public readonly Label Text_Label;

        public int Spacing
        {
            get => _Spacing;
            set
            {
                if (value < 0)
                    value = 0;
                if (_Spacing != value)
                {
                    _Spacing = value;
                    RequestRendering();
                }
            }
        }
        private int _Spacing;

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(Icon_PictureBox);
            Icon_PictureBox.TextureChanged += Icon_PictureBox_TextureChanged;

            ChildControls.Add(Text_Label);
            Text_Label.TextChanged += Text_Label_TextChanged;

            ActiveLayoutAll();
        }

        private void Icon_PictureBox_TextureChanged(PictureBox<TPixel> sender, TextureChangedEventArgs<TPixel> e)
        {
            if (AutoSize)
                AutoSetSize();
        }

        private void Text_Label_TextChanged(Control sender, TextChangedEventArgs e)
        {
            if (AutoSize)
                AutoSetSize();
        }

        public override void ActiveLayoutAll()
        {
            Icon_PictureBox.LayoutVerticalCentered(this, Spacing);
            Text_Label.LayoutVerticalCentered(this, Icon_PictureBox.RightLocation + 1);
        }

        public override void AutoSetSize()
        {
            Size size = SR.DefaultFont.GetTotalSize(Text_Label.Text);
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
