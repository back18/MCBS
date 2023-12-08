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
        private const string LIGHT_BLOCK = "minecraft:light";
        private const string BARRIER_BLOCK = "minecraft:barrier";
        private const string AIR_BLOCK = "minecraft:air";

        public ScreenOutputHandler(Screen owner)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
            _frames = [];
            ScreenDefaultBlock = "minecraft:smooth_stone";
        }

        private readonly Screen _owner;

        private readonly Dictionary<int, BlockFrame> _frames;

        public BlockFrame? LastFrame { get; internal set; }

        public string ScreenDefaultBlock { get; set; }

        public void HandleOutput(BlockFrame newFrame, int offset = 0)
        {
            ArgumentNullException.ThrowIfNull(newFrame, nameof(newFrame));

            IDictionary<Point, string> pixels = GetDifferencesPixels(newFrame);
            List<WorldBlock> blocks = ToSetBlockArguments(pixels, offset);
            _frames[offset] = newFrame;
            if (blocks.Count > 0)
            {
                MCOS.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlock(blocks);
            }
        }

        public async Task HandleOutputAsync(BlockFrame newFrame, int offset = 0)
        {
            ArgumentNullException.ThrowIfNull(newFrame, nameof(newFrame));

            IDictionary<Point, string> pixels = GetDifferencesPixels(newFrame);
            List<WorldBlock> blocks = ToSetBlockArguments(pixels, offset);
            _frames[offset] = newFrame;
            if (blocks.Count > 0)
            {
                await MCOS.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlockAsync(blocks);
            }
        }

        public bool CheckBlcok(string blockId, int offset = 0)
        {
            ArgumentException.ThrowIfNullOrEmpty(blockId, nameof(blockId));

            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            for (int y = 0; y < _owner.Height; y++)
                for (int x = 0; x < _owner.Width; x++)
                {
                    if (!sender.ConditionalBlock(_owner.ScreenPos2WorldPos(new(x, y), offset), blockId))
                        return false;
                }

            return true;
        }

        public bool CheckAirBlock(int offset = 0)
        {
            return CheckBlcok(AIR_BLOCK, offset);
        }

        public bool FillBlock(string blockId, int offset = 0, bool checkAirBlock = false)
        {
            ArgumentException.ThrowIfNullOrEmpty(blockId, nameof(blockId));

            if (checkAirBlock && !CheckAirBlock(offset))
                return false;

            HandleOutput(new HashBlockFrame(_owner.Width, _owner.Height, blockId), offset);
            return true;
        }

        public bool FillDefaultBlock(int offset = 0, bool checkAirBlock = false)
        {
            return FillBlock(ScreenDefaultBlock, offset, checkAirBlock);
        }

        public bool FillLightBlock(int offset = 0, bool checkAirBlock = false)
        {
            return FillBlock(LIGHT_BLOCK, offset, checkAirBlock);
        }

        public bool FillBarrierBlock(int offset = 0, bool checkAirBlock = false)
        {
            return FillBlock(BARRIER_BLOCK, offset, checkAirBlock);
        }

        public bool FillAirBlock(int offset = 0)
        {
            return FillBlock(AIR_BLOCK, offset, false);
        }

        private IDictionary<Point, string> GetDifferencesPixels(BlockFrame newFrame, int offset = 0)
        {
            ArgumentNullException.ThrowIfNull(newFrame, nameof(newFrame));
            if (newFrame.Width != _owner.Width || newFrame.Height != _owner.Height)
                throw new ArgumentException("帧尺寸不一致");

            if (_frames.TryGetValue(offset, out var blockFrame))
                return BlockFrame.GetDifferencesPixel(blockFrame, newFrame);

            return newFrame.GetAllPixel();
        }

        private List<WorldBlock> ToSetBlockArguments(IDictionary<Point, string> pixels, int offset = 0)
        {
            ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));

            List<WorldBlock> result = new(pixels.Count);
            foreach (var pixel in pixels)
                result.Add(new(_owner.ScreenPos2WorldPos(pixel.Key, offset), pixel.Value));
            return result;
        }
    }
}
