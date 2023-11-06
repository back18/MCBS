using MCBS.Rendering;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Events
{
    public class TextureChangedEventArgs<TPixel> : EventArgs where TPixel : unmanaged, IPixel<TPixel>
    {
        public TextureChangedEventArgs(Texture<TPixel> oldTexture, Texture<TPixel> newTexture)
        {
            OldTexture = oldTexture;
            NewTexture = newTexture;
        }

        public Texture<TPixel> OldTexture { get; }

        public Texture<TPixel> NewTexture { get; }
    }
}
