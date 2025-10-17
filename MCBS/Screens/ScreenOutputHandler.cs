using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using MCBS.UI.Extensions;
using QuanLib.Game;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
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

        public ScreenOutputHandler(ScreenContext owner)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
            _frameCaches = [];
            _outputBuffers = [];
            _blockUpdateList = [];
            ScreenDefaultBlock = "minecraft:smooth_stone";
        }

        private readonly ScreenContext _owner;

        private readonly Dictionary<int, BlockFrame> _frameCaches;

        private readonly Dictionary<int, BlockFrame> _outputBuffers;

        private readonly List<WorldBlock> _blockUpdateList;

        public string ScreenDefaultBlock { get; set; }

        public void HandleOutput()
        {
            if (_blockUpdateList.Count == 0)
                return;

            WorldBlock[] blocks = _blockUpdateList.ToArray();
            _blockUpdateList.Clear();
            MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlock(blocks);
        }

        public async Task HandleOutputAsync()
        {
            if (_blockUpdateList.Count == 0)
                return;

            WorldBlock[] blocks = _blockUpdateList.ToArray();
            _blockUpdateList.Clear();
            await MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlockAsync(blocks);
        }

        public void HandleFrameUpdate(BlockFrame newFrame, int offset = 0)
        {
            if (_frameCaches.TryGetValue(offset, out var frameCache) && frameCache == newFrame)
                return;

            ScreenPixel<string>[] pixels = GetDifferencePixels(newFrame, offset);

            if (!_outputBuffers.TryGetValue(offset, out var outputBuffer))
            {
                outputBuffer = new LosslessBlockFrame(newFrame.Width, newFrame.Height, AIR_BLOCK);
                _outputBuffers.Add(offset, outputBuffer);
            }

            foreach (var pixel in pixels)
                outputBuffer[pixel.Position.X, pixel.Position.Y] = pixel.Pixel;

            WorldBlock[] blocks = ToWorldBlocks(pixels, offset);
            _blockUpdateList.AddRange(blocks);
            _frameCaches[offset] = newFrame;
        }

        public async Task HandleFrameUpdateAsync(BlockFrame newFrame, int offset = 0)
        {
            await Task.Run(() => HandleFrameUpdate(newFrame, offset));
        }

        public void UpdateBuffer()
        {
            Rectangle formRectangle = _owner.RootForm.GetRectangle();
            Size screenSize = formRectangle.Size + new Size(32);
            Point screenOffset = new(formRectangle.Location.X - 16, formRectangle.Location.Y - 16);

            _owner.Screen.SetSize(screenSize);
            _owner.Screen.ApplyTranslate(screenOffset);
            _owner.ScreenView.ClientSize = screenSize;
            _owner.RootForm.ClientLocation = new(16, 16);
            foreach (int offset in _outputBuffers.Keys)
            {
                LosslessBlockFrame outputBuffer = new(screenSize.Width, screenSize.Height, AIR_BLOCK);
                outputBuffer.Overwrite(_outputBuffers[offset], -screenOffset);
                _outputBuffers[offset] = outputBuffer;
                _frameCaches[offset] = outputBuffer;
            }
        }

        public void ResetBuffer()
        {
            Rectangle formRectangle = _owner.RootForm.GetRectangle();
            Size screenSize = formRectangle.Size + new Size(32);
            foreach (int offset in _outputBuffers.Keys)
            {
                LosslessBlockFrame outputBuffer = new(screenSize.Width, screenSize.Height, AIR_BLOCK);
                _outputBuffers[offset] = outputBuffer;
                _frameCaches[offset] = outputBuffer;
            }
        }

        public bool CheckBlcok(string blockId, int offset = 0)
        {
            ArgumentException.ThrowIfNullOrEmpty(blockId, nameof(blockId));

            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;
            Screen screen = _owner.Screen;
            for (int y = 0; y < screen.Height; y++)
                for (int x = 0; x < screen.Width; x++)
                {
                    if (!sender.ConditionalBlock(screen.ScreenPos2WorldPos(new(x, y), offset), blockId))
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

            Screen screen = _owner.Screen;
            Vector3<int> startPos = screen.ScreenPos2WorldPos(Point.Empty, offset);
            Vector3<int> endPos = screen.ScreenPos2WorldPos(new(screen.Width - 1, screen.Height - 1), offset);
            MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.Fill(startPos, endPos, AIR_BLOCK, true);

            return true;
        }

        public void FillBlockForAll(string blockId)
        {
            ArgumentException.ThrowIfNullOrEmpty(blockId, nameof(blockId));

            if (_outputBuffers.Count == 0)
                return;

            Screen screen = _owner.Screen;
            Vector3<int> startPos = screen.ScreenPos2WorldPos(Point.Empty, _outputBuffers.Keys.Min());
            Vector3<int> endPos = screen.ScreenPos2WorldPos(new(screen.Width - 1, screen.Height - 1), _outputBuffers.Keys.Max());
            MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.Fill(startPos, endPos, AIR_BLOCK, true);
        }

        public bool FillDefaultBlock(int offset = 0, bool checkAirBlock = false)
        {
            return FillBlock(ScreenDefaultBlock, offset, checkAirBlock);
        }

        public void FillDefaultBlockForAll()
        {
            FillBlockForAll(ScreenDefaultBlock);
        }

        public bool FillLightBlock(int offset = 0, bool checkAirBlock = false)
        {
            return FillBlock(LIGHT_BLOCK, offset, checkAirBlock);
        }

        public void FillLightBlockForAll()
        {
            FillBlockForAll(LIGHT_BLOCK);
        }

        public bool FillBarrierBlock(int offset = 0, bool checkAirBlock = false)
        {
            return FillBlock(BARRIER_BLOCK, offset, checkAirBlock);
        }

        public void FillBarrierBlockForAll()
        {
            FillBlockForAll(BARRIER_BLOCK);
        }

        public bool FillAirBlock(int offset = 0)
        {
            return FillBlock(AIR_BLOCK, offset, false);
        }

        public void FillAirBlockForAll()
        {
            FillBlockForAll(AIR_BLOCK);
        }

        private ScreenPixel<string>[] GetDifferencePixels(BlockFrame newFrame, int offset = 0)
        {
            ArgumentNullException.ThrowIfNull(newFrame, nameof(newFrame));
            if (newFrame.Width != _owner.Screen.Width || newFrame.Height != _owner.Screen.Height)
                throw new ArgumentException("帧尺寸不一致");

            if (!_outputBuffers.TryGetValue(offset, out var outputBuffer))
                return newFrame.GetAllPixel();

            _frameCaches.TryGetValue(offset, out var frameCache);

            if (frameCache is LayerBlockFrame layerBlockFrame1 &&
                newFrame is LayerBlockFrame layerBlockFrame2 &&
                layerBlockFrame1.Width == layerBlockFrame2.Width &&
                layerBlockFrame1.Height == layerBlockFrame2.Height)
            {
                var (location, differenceFrame1, differenceFrame2) = LayerBlockFrame.GetDifferenceLayer(layerBlockFrame1, layerBlockFrame2);

                BlockFrame outputBufferFrame =
                    differenceFrame2.Width == outputBuffer.Width &&
                    differenceFrame2.Height == outputBuffer.Height &&
                    location == Point.Empty ?
                    outputBuffer :
                    new NestingBlockFrame(differenceFrame2.Width, differenceFrame2.Height, outputBuffer, -location, Point.Empty, 0, string.Empty, AIR_BLOCK);

                ScreenPixel<string>[] screenPixels = BlockFrame.GetDifferencePixels(outputBufferFrame, differenceFrame2);

                if (location != Point.Empty)
                {
                    for (int i = 0; i < screenPixels.Length; i++)
                    {
                        ScreenPixel<string> screenPixel = screenPixels[i];
                        screenPixels[i] = new(new(screenPixel.Position.X + location.X, screenPixel.Position.Y + location.Y), screenPixel.Pixel);
                    }
                }

                return screenPixels;
            }
            else if (frameCache is NestingBlockFrame nestingBlockFrame1 &&
                     newFrame is NestingBlockFrame nestingBlockFrame2 &&
                     nestingBlockFrame1.BackgroundColor == nestingBlockFrame2.BackgroundColor &&
                     nestingBlockFrame1.Width == nestingBlockFrame2.Width &&
                     nestingBlockFrame1.Height == nestingBlockFrame2.Height)
            {
                Point start1 = nestingBlockFrame1.InnerPosition.BorderStartPosition;
                Point start2 = nestingBlockFrame2.InnerPosition.BorderStartPosition;
                Point end1 = nestingBlockFrame1.InnerPosition.BorderEndPosition;
                Point end2 = nestingBlockFrame2.InnerPosition.BorderEndPosition;
                Point start = new(Math.Min(start1.X, start2.X), Math.Min(start1.Y, start2.Y));
                Point end = new(Math.Max(end1.X, end2.X), Math.Max(end1.Y, end2.Y));
                Size size = new(end.X - start.X + 1, end.Y - start.Y + 1);

                NestingBlockFrame reducedNestingBlockFrame1 = new(
                    size.Width, size.Height,
                    nestingBlockFrame1.InnerBlockFrame,
                    nestingBlockFrame1.Location - new Size(start.X, start.Y),
                    Point.Empty,
                    0,
                    nestingBlockFrame1.BackgroundColor,
                    string.Empty);
                NestingBlockFrame reducedNestingBlockFrame2 = new(
                    size.Width, size.Height,
                    nestingBlockFrame2.InnerBlockFrame,
                    nestingBlockFrame2.Location - new Size(start.X, start.Y),
                    Point.Empty,
                    0,
                    nestingBlockFrame2.BackgroundColor,
                    string.Empty);

                ScreenPixel<string>[] screenPixels = BlockFrame.GetDifferencePixels(reducedNestingBlockFrame1, reducedNestingBlockFrame2);

                if (start != Point.Empty)
                {
                    for (int i = 0; i < screenPixels.Length; i++)
                    {
                        ScreenPixel<string> screenPixel = screenPixels[i];
                        screenPixels[i] = new(new(screenPixel.Position.X + start.X, screenPixel.Position.Y + start.Y), screenPixel.Pixel);
                    }
                }

                return screenPixels;
            }
            else
            {
                return BlockFrame.GetDifferencePixels(outputBuffer, newFrame);
            }
        }

        private WorldBlock[] ToWorldBlocks(ScreenPixel<string>[] pixels, int offset = 0)
        {
            ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));

            WorldBlock[] result = new WorldBlock[pixels.Length];
            Screen screen = _owner.Screen;

            for (int i = 0; i < pixels.Length; i++)
            {
                ScreenPixel<string> pixel = pixels[i];
                result[i] = new(screen.ScreenPos2WorldPos(pixel.Position, offset), pixel.Pixel);
            }

            return result;
        }
    }
}
