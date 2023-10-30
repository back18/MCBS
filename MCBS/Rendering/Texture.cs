using QuanLib.Core;
using QuanLib.Minecraft;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public abstract class Texture : UnmanagedBase
    {
        public abstract Rectangle CropRectangle { get; set; }

        public abstract ResizeOptions ResizeOptions { get; }

        public abstract Image GetImageSource();

        public abstract BlockFrame CreateBlockFrame(Size size, Facing facing);

        public abstract Texture Clone();

        public static bool Equals(Texture? texture1, Texture? texture2)
        {
            if (texture1 == texture2)
                return true;
            if (texture1 is null || texture2 is null)
                return false;

            return texture1.GetImageSource() == texture2.GetImageSource() &&
                   texture1.CropRectangle == texture2?.CropRectangle &&
                   OptionsUtil.ResizeOptionsEquals(texture1.ResizeOptions, texture2.ResizeOptions);
        }
    }
}
