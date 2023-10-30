using FFMediaToolkit.Decoding;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public static class OptionsUtil
    {
        public static ResizeOptions CreateDefaultResizeOption()
        {
            return new()
            {
                Mode = ResizeMode.Crop,
                Position = AnchorPositionMode.Center,
                CenterCoordinates = null,
                Size = new(64, 64),
                Sampler = KnownResamplers.Bicubic,
                Compand = false,
                TargetRectangle = null,
                PremultiplyAlpha = true,
                PadColor = default
            };
        }

        public static MediaOptions CreateDefaultMediaOptions()
        {
            return new MediaOptions();
        }

        public static bool ResizeOptionsEquals(ResizeOptions? resizeOptions1, ResizeOptions? resizeOptions2)
        {
            if (resizeOptions1 == resizeOptions2)
                return true;
            if (resizeOptions1 is null || resizeOptions2 is null)
                return false;

            return resizeOptions1.Mode == resizeOptions2.Mode &&
                   resizeOptions1.Position == resizeOptions2.Position &&
                   resizeOptions1.CenterCoordinates == resizeOptions2.CenterCoordinates &&
                   resizeOptions1.Size == resizeOptions2.Size &&
                   resizeOptions1.Sampler == resizeOptions2.Sampler &&
                   resizeOptions1.Compand == resizeOptions2.Compand &&
                   resizeOptions1.TargetRectangle == resizeOptions2.TargetRectangle &&
                   resizeOptions1.PremultiplyAlpha == resizeOptions2.PremultiplyAlpha &&
                   resizeOptions1.PadColor == resizeOptions2.PadColor;
        }

        public static bool MediaOptionsEquals(MediaOptions? mediaOptions1, MediaOptions? mediaOptions2)
        {
            if (mediaOptions1 == mediaOptions2)
                return true;
            if (mediaOptions1 is null || mediaOptions2 is null)
                return false;

            return mediaOptions1.PacketBufferSizeLimit == mediaOptions2.PacketBufferSizeLimit &&
                   mediaOptions1.DemuxerOptions == mediaOptions2.DemuxerOptions &&
                   mediaOptions1.VideoPixelFormat == mediaOptions2.VideoPixelFormat &&
                   mediaOptions1.TargetVideoSize == mediaOptions2.TargetVideoSize &&
                   mediaOptions1.VideoSeekThreshold == mediaOptions2.VideoSeekThreshold &&
                   mediaOptions1.AudioSeekThreshold == mediaOptions2.AudioSeekThreshold &&
                   mediaOptions1.DecoderThreads == mediaOptions2.DecoderThreads &&
                   mediaOptions1.DecoderOptions == mediaOptions2.DecoderOptions &&
                   mediaOptions1.StreamsToLoad == mediaOptions2.StreamsToLoad;
        }
    }
}
