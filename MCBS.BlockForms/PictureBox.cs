using MCBS.BlockForms.Utility;
using MCBS.Rendering;
using MCBS.Rendering.Extensions;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class PictureBox<TPixel> : Control where TPixel : unmanaged, IPixel<TPixel>
    {
        public PictureBox()
        {
            DefaultResizeOptions = OptionsUtil.CreateDefaultResizeOption();
            _Texture = new Texture<TPixel>(new(64, 64, this.GetBlockColor<TPixel>(BlockManager.Concrete.White)), DefaultResizeOptions.Clone());
            ClientSize = new(64, 64);
            AutoSize = true;
            ContentAnchor = AnchorPosition.Centered;

            TextureChanged += OnTextureChanged;
        }

        public ResizeOptions DefaultResizeOptions { get; }

        public Texture<TPixel> Texture
        {
            get => _Texture;
            set
            {
                if (!MCBS.Rendering.Texture.Equals(_Texture, value))
                {
                    Texture<TPixel> temp = _Texture;
                    _Texture = value;
                    TextureChanged.Invoke(this, new(temp, _Texture));
                    RequestRendering();
                }
            }
        }
        private Texture<TPixel> _Texture;

        public event EventHandler<PictureBox<TPixel>, ValueChangedEventArgs<Texture<TPixel>>> TextureChanged;

        protected virtual void OnTextureChanged(PictureBox<TPixel> sender, ValueChangedEventArgs<Texture<TPixel>> e)
        {
            e.OldValue.Dispose();

            if (AutoSize)
                AutoSetSize();
        }

        protected override BlockFrame Rendering()
        {
            BlockFrame textureFrame = Texture.CreateBlockFrame(ClientSize, GetScreenPlane().NormalFacing);
            if (IsRenderingTransparencyTexture)
                return textureFrame;

            BlockFrame baseFrame =  base.Rendering();
            baseFrame.Overwrite(textureFrame, Point.Empty);
            return baseFrame;
        }

        public override void AutoSetSize()
        {
            ClientSize = Texture.GetOutputSize();
        }

        public void SetImage(Image<TPixel> image)
        {
            ResizeOptions resizeOptions = DefaultResizeOptions.Clone();
            resizeOptions.Size = ClientSize;
            Texture = new Texture<TPixel>(image, resizeOptions);
        }

        public bool TryReadImageFile(string path)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                SetImage(Image.Load<TPixel>(path));
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            Texture.Dispose();
        }
    }
}
