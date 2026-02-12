using MCBS.UI.Extensions;
using QuanLib.Minecraft;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    public static class ControlExtension
    {
        public static TPixel GetBlockColor<TPixel>(this Control source, string blockId) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (string.IsNullOrEmpty(blockId))
                return default;

            return new Color(MinecraftResourceManager.Rgba32BlockMappings[source.GetNormalFacing()][blockId]).ToPixel<TPixel>();
        }

        public static bool TryGetBlockColor<TPixel>(this Control source, string? blockId, out TPixel color) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (string.IsNullOrEmpty(blockId))
            {
                color = default;
                return true;
            }

            if (MinecraftResourceManager.Rgba32BlockMappings[source.GetNormalFacing()].TryGetColor(blockId, out var rgba32))
            {
                color = new Color(rgba32).ToPixel<TPixel>();
                return true;
            }
            else
            {
                color = default;
                return false;
            }
        }

        public static TPixel GetBlockColorOrDefault<TPixel>(this Control source, string? blockId, TPixel defaultColor) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (source.TryGetBlockColor<TPixel>(blockId, out var color))
                return color;
            else
                return defaultColor;
        }

        public static TPixel GetBlockColorOrDefault<TPixel>(this Control source, string? blockId, string defaultBlockId) where TPixel : unmanaged, IPixel<TPixel>
        {
            return source.GetBlockColorOrDefault(blockId, source.GetBlockColor<TPixel>(defaultBlockId));
        }
    }
}
