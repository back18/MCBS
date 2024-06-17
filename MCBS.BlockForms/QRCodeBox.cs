using MCBS.Rendering;
using QRCoder;
using QuanLib.Core.Events;
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
    public class QRCodeBox : PictureBox<Rgb24>
    {
        public QRCodeBox()
        {
            AutoSize = false;
            ECCLevel = QRCodeGenerator.ECCLevel.L;
            _generator = new();
        }

        private readonly QRCodeGenerator _generator;

        public QRCodeGenerator.ECCLevel ECCLevel { get; set; }

        protected override void OnTextChanged(Control sender, ValueChangedEventArgs<string> e)
        {
            base.OnTextChanged(sender, e);

            QRCodeData data = _generator.CreateQrCode(e.NewValue, ECCLevel);
            BitmapByteQRCode code = new(data);
            byte[] bytes = code.GetGraphic(1);
            ResizeOptions options = OptionsUtil.CreateDefaultResizeOption();
            options.Sampler = KnownResamplers.NearestNeighbor;
            Texture = new(Image.Load<Rgb24>(bytes), options);
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            _generator.Dispose();
        }
    }
}
