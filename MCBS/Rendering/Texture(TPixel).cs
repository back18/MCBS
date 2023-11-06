using MCBS.UI;
using QuanLib.Core;
using QuanLib.Minecraft;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class Texture<TPixel> : Texture where TPixel : unmanaged, IPixel<TPixel>
    {
        public Texture(Image<TPixel> imageSource, ResizeOptions resizeOptions)
        {
            if (imageSource is null)
                throw new ArgumentNullException(nameof(imageSource));
            if (resizeOptions is null)
                throw new ArgumentNullException(nameof(resizeOptions));

            ImageSource = imageSource;
            CropRectangle = imageSource.Bounds;
            ResizeOptions = resizeOptions;
            if (ResizeOptions.Size == Size.Empty)
                ResizeOptions.Size = imageSource.Size;
        }

        public Texture(Image<TPixel> imageSource) : this(imageSource, OptionsUtil.CreateDefaultResizeOption()) { }

        private TextureOutput? _output;

        public Image<TPixel> ImageSource { get; private set; }

        public override Rectangle CropRectangle { get; set; }

        public override ResizeOptions ResizeOptions { get; }

        public Image<TPixel> GetImageOutput()
        {
            if (_output is null || CropRectangle != _output.CropRectangle || !OptionsUtil.ResizeOptionsEquals(ResizeOptions, _output.ResizeOptions))
            {
                _output?.Dispose();
                _output = new(this);
            }

            return _output.ImageOutput.Clone();
        }

        public Image<TPixel> GetImageOutput(Size size)
        {
            ResizeOptions.Size = size;
            return GetImageOutput();
        }

        public Size GetOutputSize()
        {
            if (_output is null || CropRectangle != _output.CropRectangle || !OptionsUtil.ResizeOptionsEquals(ResizeOptions, _output.ResizeOptions))
            {
                _output?.Dispose();
                _output = new(this);
            }

            return _output.ImageOutput.Size;
        }

        public override void ImageSourceUpdated()
        {
            _output = null;
        }

        public void ImageSourceUpdated(Image<TPixel> image, bool disposing = true)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));

            if (disposing)
                ImageSource.Dispose();
            ImageSource = image;
            _output = null;
        }

        public override Image GetImageSource()
        {
            return ImageSource;
        }

        public override BlockFrame CreateBlockFrame(Size size, Facing facing)
        {
            return new ColorBlockFrame<TPixel>(GetImageOutput(size), facing);
        }

        public override Texture Clone()
        {
            Texture<TPixel> texture = new(ImageSource, ResizeOptions);
            texture._output = _output;
            return texture;
        }

        protected override void DisposeUnmanaged()
        {
            ImageSource.Dispose();
            _output?.Dispose();
        }

        private class TextureOutput : UnmanagedBase
        {
            public TextureOutput(Texture<TPixel> owner)
            {
                if (owner is null)
                    throw new ArgumentNullException(nameof(owner));

                CropRectangle = owner.CropRectangle;
                ResizeOptions = owner.ResizeOptions.Clone();

                if (CropRectangle != owner.ImageSource.Bounds)
                    ImageOutput = owner.ImageSource.Clone(x => x.Crop(CropRectangle));
                else
                    ImageOutput = owner.ImageSource.Clone();
                ImageOutput.Mutate(x => x.Resize(ResizeOptions));
            }

            public Rectangle CropRectangle { get; set; }

            public ResizeOptions ResizeOptions { get; }

            public Image<TPixel> ImageOutput { get; set; }

            protected override void DisposeUnmanaged()
            {
                ImageOutput.Dispose();
            }
        }
    }
}
