using MCBS.Rendering;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    public static class RenderingHelper
    {
        public static BlockFrame RenderingBackground(this Control source, Size size)
        {
            Texture? texture = source.GetBackgroundTexture();
            if (texture is null)
                return new HashBlockFrame(size, source.GetBackgroundColor());

            BlockFrame textureFrame = texture.CreateBlockFrame(size, source.GetScreenPlane().NormalFacing);
            if (source.IsRenderingTransparencyTexture)
                return textureFrame;

            HashBlockFrame baseFrame = new(size, source.GetBackgroundColor());
            baseFrame.Overwrite(textureFrame, Point.Empty);
            return baseFrame;
        }
    }
}
