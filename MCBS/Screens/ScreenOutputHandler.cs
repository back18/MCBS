﻿using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using MCBS.UI.Extensions;
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

        public ScreenOutputHandler(ScreenContext owner)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
            _buffers = [];
            _blocks = [];
            ScreenDefaultBlock = "minecraft:smooth_stone";
        }

        private readonly ScreenContext _owner;

        private readonly Dictionary<int, BlockFrame> _buffers;

        private readonly List<WorldBlock> _blocks;

        public string ScreenDefaultBlock { get; set; }

        public void HandleOutput()
        {
            if (_blocks.Count == 0)
                return;

            WorldBlock[] blocks = _blocks.ToArray();
            _blocks.Clear();
            MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlock(blocks);
        }

        public async Task HandleOutputAsync()
        {
            if (_blocks.Count == 0)
                return;

            WorldBlock[] blocks = _blocks.ToArray();
            _blocks.Clear();
            await MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlockAsync(blocks);
        }

        public void HandleFrameUpdate(BlockFrame newFrame, int offset = 0)
        {
            if (_buffers.TryGetValue(offset, out var oldFrame) && oldFrame == newFrame)
                return;

            ScreenPixel<string>[] pixels = GetDifferencesPixels(newFrame);
            WorldBlock[] blocks = ToSetBlockArguments(pixels, offset);
            _blocks.AddRange(blocks);
            _buffers[offset] = newFrame;
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

            _owner.Screen.Width = screenSize.Width;
            _owner.Screen.Height = screenSize.Height;
            _owner.Screen.Translate(screenOffset);
            _owner.ScreenView.ClientSize = screenSize;
            _owner.RootForm.ClientLocation = new(16, 16);
            foreach (int offset in _buffers.Keys)
            {
                HashBlockFrame hashBlockFrame = new(screenSize.Width, screenSize.Height, AIR_BLOCK);
                hashBlockFrame.Overwrite(_buffers[offset], -screenOffset);
                _buffers[offset] = hashBlockFrame;
            }
        }

        public void ResetBuffer()
        {
            Rectangle formRectangle = _owner.RootForm.GetRectangle();
            Size screenSize = formRectangle.Size + new Size(32);
            foreach (int offset in _buffers.Keys)
            {
                HashBlockFrame hashBlockFrame = new(screenSize.Width, screenSize.Height, AIR_BLOCK);
                _buffers[offset] = hashBlockFrame;
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

            Rectangle formRectangle = _owner.RootForm.GetRectangle();
            Size screenSize = formRectangle.Size + new Size(32);
            HashBlockFrame baseFrame = new(screenSize, AIR_BLOCK);
            baseFrame.Overwrite(new HashBlockFrame(formRectangle.Size, blockId), formRectangle.Location);
            HandleFrameUpdate(baseFrame, offset);
            HandleOutput();
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

        private ScreenPixel<string>[] GetDifferencesPixels(BlockFrame newFrame, int offset = 0)
        {
            ArgumentNullException.ThrowIfNull(newFrame, nameof(newFrame));
            if (newFrame.Width != _owner.Screen.Width || newFrame.Height != _owner.Screen.Height)
                throw new ArgumentException("帧尺寸不一致");

            if (_buffers.TryGetValue(offset, out var blockFrame))
                return BlockFrame.GetDifferencesPixel(blockFrame, newFrame);
            else
                return newFrame.GetAllPixel();
        }

        private WorldBlock[] ToSetBlockArguments(ScreenPixel<string>[] pixels, int offset = 0)
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
