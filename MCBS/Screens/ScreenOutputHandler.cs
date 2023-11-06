using MCBS.Rendering;
using QuanLib.Minecraft;
using SixLabors.ImageSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕输出处理
    /// </summary>
    public class ScreenOutputHandler
    {
        public ScreenOutputHandler(Screen owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        private readonly Screen _owner;

        public BlockFrame? LastFrame { get; internal set; }

        public bool IsGenerated => LastFrame is not null;

        public void HandleOutput(BlockFrame frame)
        {
            IDictionary<Point, string> pixels = GetDifferencesPixels(frame);
            List<ISetBlockArgument> arguments = ToSetBlockArguments(pixels);
            LastFrame = frame;
            if (arguments.Count > 0)
            {
                MCOS.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlock(arguments);
            }
        }

        public async Task HandleOutputAsync(BlockFrame frame)
        {
            IDictionary<Point, string> pixels = GetDifferencesPixels(frame);
            List<ISetBlockArgument> arguments = ToSetBlockArguments(pixels);
            LastFrame = frame;
            if (arguments.Count > 0)
            {
                await MCOS.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlockAsync(arguments);
            }
        }

        private IDictionary<Point, string> GetDifferencesPixels(BlockFrame frame)
        {
            if (frame is null)
                throw new ArgumentNullException(nameof(frame));
            if (frame.Width != _owner.Width || frame.Height != _owner.Height)
                throw new ArgumentException("帧尺寸不一致");

            if (LastFrame is null)
                return frame.GetAllPixel();
            else
                return BlockFrame.GetDifferencesPixel(LastFrame, frame);
        }

        private List<ISetBlockArgument> ToSetBlockArguments(IDictionary<Point, string> pixels)
        {
            List<ISetBlockArgument> result = new(pixels.Count);
            foreach (var pixel in pixels)
                result.Add(new SetBlockArgument(_owner.ToWorldPosition(pixel.Key), pixel.Value));
            return result;
        }
    }
}
