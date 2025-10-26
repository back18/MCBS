using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.Game;
using QuanLib.Minecraft;
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

            _owner.ScreenController.Move += ScreenController_Move;
            _owner.ScreenController.Translate += ScreenController_Translate;
            _owner.ScreenController.Resize += ScreenController_Resize;
            _owner.ScreenController.PlaneFacingChanged += ScreenController_PlaneFacingChanged;
        }

        private readonly ScreenContext _owner;

        private readonly Dictionary<int, BlockFrame> _frameCaches;

        private readonly Dictionary<int, BlockFrame> _outputBuffers;

        private readonly List<WorldBlock> _blockUpdateList;

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

        public void HandleFrameUpdate(BlockFrame newFrame, int layer = 0)
        {
            ThrowHelper.ArgumentOutOfRange(-_owner.MaxBackLayers, _owner.MaxFrontLayers, layer, nameof(layer));

            if (_frameCaches.TryGetValue(layer, out var frameCache) && frameCache == newFrame)
                return;

            ScreenPixel<string>[] pixels = GetDifferencePixels(newFrame, layer);

            if (!_outputBuffers.TryGetValue(layer, out var outputBuffer))
            {
                outputBuffer = new LosslessBlockFrame(newFrame.Width, newFrame.Height, AIR_BLOCK);
                _outputBuffers.Add(layer, outputBuffer);
            }

            foreach (var pixel in pixels)
                outputBuffer[pixel.Position.X, pixel.Position.Y] = pixel.Pixel;

            WorldBlock[] blocks = ToWorldBlocks(pixels, layer);
            _blockUpdateList.AddRange(blocks);
            _frameCaches[layer] = newFrame;
        }

        public async Task HandleFrameUpdateAsync(BlockFrame newFrame, int layer = 0)
        {
            await Task.Run(() => HandleFrameUpdate(newFrame, layer));
        }

        private ScreenPixel<string>[] GetDifferencePixels(BlockFrame newFrame, int layer = 0)
        {
            ArgumentNullException.ThrowIfNull(newFrame, nameof(newFrame));
            ThrowHelper.ArgumentOutOfRange(-_owner.MaxBackLayers, _owner.MaxFrontLayers, layer, nameof(layer));

            if (newFrame.Width != _owner.Screen.Width || newFrame.Height != _owner.Screen.Height)
                throw new ArgumentException("帧尺寸不一致");

            if (!_outputBuffers.TryGetValue(layer, out var outputBuffer))
                return newFrame.GetAllPixel();

            _frameCaches.TryGetValue(layer, out var frameCache);

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

        private WorldBlock[] ToWorldBlocks(ScreenPixel<string>[] pixels, int layer = 0)
        {
            ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));

            WorldBlock[] result = new WorldBlock[pixels.Length];
            Screen screen = _owner.Screen;

            for (int i = 0; i < pixels.Length; i++)
            {
                ScreenPixel<string> pixel = pixels[i];
                result[i] = new(screen.ScreenPos2WorldPos(pixel.Position, layer), pixel.Pixel);
            }

            return result;
        }

        private void ResetBuffer()
        {
            Screen screen = _owner.Screen;
            _owner.ScreenController.ClearScreenRange();

            foreach (int offset in _outputBuffers.Keys)
            {
                LosslessBlockFrame outputBuffer = new(screen.Width, screen.Height, AIR_BLOCK);
                _outputBuffers[offset] = outputBuffer;
                _frameCaches[offset] = outputBuffer;
            }
        }

        private void ScreenController_Move(ScreenController sender, ValueChangedEventArgs<Vector3<int>> e)
        {
            _owner.RootForm.ClientLocation = new(16, 16);

            Screen screen = _owner.Screen;
            int normalFacingIndex = Math.Abs((int)screen.NormalFacing) - 1;

            if (e.OldValue[normalFacingIndex] != e.NewValue[normalFacingIndex])
                ResetBuffer();
        }

        private void ScreenController_Translate(ScreenController sender, EventArgs<Point> e)
        {
            Screen screen = _owner.Screen;
            Point offset = e.Argument;

            foreach (int layer in _outputBuffers.Keys)
            {
                LosslessBlockFrame outputBuffer = new(screen.Width, screen.Height, AIR_BLOCK);
                outputBuffer.Overwrite(_outputBuffers[layer], -offset);
                _outputBuffers[layer] = outputBuffer;
                _frameCaches[layer] = outputBuffer;
            }
        }

        private void ScreenController_Resize(ScreenController sender, ValueChangedEventArgs<Size> e)
        {
            Size size = e.NewValue;
            _owner.ScreenView.ClientSize = size - new Size(_owner.ScreenView.BorderWidth * 2);

            BlockFrame? defaultOutputBuffer = _outputBuffers.Values.FirstOrDefault();
            if (defaultOutputBuffer is null)
                return;

            foreach (int layer in _outputBuffers.Keys)
            {
                if (_outputBuffers[layer].Width == size.Width && _outputBuffers[layer].Height == size.Height)
                    continue;

                LosslessBlockFrame outputBuffer = new(size.Width, size.Height, AIR_BLOCK);
                outputBuffer.Overwrite(_outputBuffers[layer], Point.Empty);
                _outputBuffers[layer] = outputBuffer;
                _frameCaches[layer] = outputBuffer;
            }
        }

        private void ScreenController_PlaneFacingChanged(ScreenController sender, ValueChangedEventArgs<PlaneFacing> e)
        {
            if (e.OldValue.NormalFacing == e.NewValue.NormalFacing)
                ResetBuffer();
        }
    }
}
