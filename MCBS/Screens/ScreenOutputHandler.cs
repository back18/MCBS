using MCBS.Rendering;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
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
        private const string AIR_BLOCK = "minecraft:air";
        private const string LIGHT_BLOCK = "minecraft:light";

        public ScreenOutputHandler(Screen owner)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
            ScreenDefaultBlock = "minecraft:smooth_stone";
        }

        private readonly Screen _owner;

        public BlockFrame? LastFrame { get; internal set; }

        public string ScreenDefaultBlock { get; set; }

        public void HandleOutput(BlockFrame frame)
        {
            IDictionary<Point, string> pixels = GetDifferencesPixels(frame);
            List<WorldBlock> blocks = ToSetBlockArguments(pixels);
            LastFrame = frame;
            if (blocks.Count > 0)
            {
                MCOS.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlock(blocks);
            }
        }

        public async Task HandleOutputAsync(BlockFrame frame)
        {
            IDictionary<Point, string> pixels = GetDifferencesPixels(frame);
            List<WorldBlock> blocks = ToSetBlockArguments(pixels);
            LastFrame = frame;
            if (blocks.Count > 0)
            {
                await MCOS.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlockAsync(blocks);
            }
        }

        public bool CheckBlcok(string blockId)
        {
            ArgumentException.ThrowIfNullOrEmpty(blockId, nameof(blockId));

            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            for (int y = 0; y < _owner.Height; y++)
                for (int x = 0; x < _owner.Width; x++)
                {
                    if (!sender.ConditionalBlock(_owner.ScreenPos2WorldPos(new(x, y)), blockId))
                        return false;
                }

            return true;
        }

        public bool CheckAirBlock()
        {
            return CheckBlcok(AIR_BLOCK);
        }

        public bool FillBlock(string blockId, bool checkAirBlock = false)
        {
            ArgumentException.ThrowIfNullOrEmpty(blockId, nameof(blockId));

            if (checkAirBlock && !CheckAirBlock())
                return false;

            HandleOutput(new HashBlockFrame(_owner.Width, _owner.Height, blockId));
            return true;
        }

        public bool FillDefaultBlock(bool checkAirBlock = false)
        {
            return FillBlock(ScreenDefaultBlock, checkAirBlock);
        }

        public bool FillAirBlock()
        {
            return FillBlock(AIR_BLOCK, false);
        }

        private IDictionary<Point, string> GetDifferencesPixels(BlockFrame frame)
        {
            ArgumentNullException.ThrowIfNull(frame, nameof(frame));
            if (frame.Width != _owner.Width || frame.Height != _owner.Height)
                throw new ArgumentException("帧尺寸不一致");

            if (LastFrame is null)
                return frame.GetAllPixel();
            else
                return BlockFrame.GetDifferencesPixel(LastFrame, frame);
        }

        private List<WorldBlock> ToSetBlockArguments(IDictionary<Point, string> pixels)
        {
            ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));

            List<WorldBlock> result = new(pixels.Count);
            foreach (var pixel in pixels)
                result.Add(new(_owner.ScreenPos2WorldPos(pixel.Key), pixel.Value));
            return result;
        }
    }
}
