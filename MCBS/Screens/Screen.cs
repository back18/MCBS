using MCBS.Rendering;
using QuanLib.Core;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Vector;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕
    /// </summary>
    public class Screen : IPlane, IScreenOptions
    {
        public Screen(IScreenOptions options) : this(options.StartPosition, options.Width, options.Height, options.XFacing, options.YFacing)
        {
        }

        public Screen(BlockPos startPosition, int width, int height, Facing xFacing, Facing yFacing)
        {
            ThrowHelper.ArgumentOutOfRange(-64, 319, startPosition.Y, "startPosition.Y");
            ThrowHelper.ArgumentOutOfMin(1, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(1, height, nameof(height));

            string xyFacing = xFacing.ToString() + yFacing.ToString();
            switch (xyFacing)
            {
                case "XpYm":
                case "YmXm":
                case "XmYp":
                case "YpXp":
                    PlaneAxis = QuanLib.Minecraft.PlaneAxis.XY;
                    NormalFacing = Facing.Zp;
                    PlaneCoordinate = startPosition.Z;
                    break;
                case "XmYm":
                case "YmXp":
                case "XpYp":
                case "YpXm":
                    PlaneAxis = QuanLib.Minecraft.PlaneAxis.XY;
                    NormalFacing = Facing.Zm;
                    PlaneCoordinate = startPosition.Z;
                    break;
                case "ZmYm":
                case "YmZp":
                case "ZpYp":
                case "YpZm":
                    PlaneAxis = QuanLib.Minecraft.PlaneAxis.ZY;
                    NormalFacing = Facing.Xp;
                    PlaneCoordinate = startPosition.X;
                    break;
                case "ZpYm":
                case "YmZm":
                case "ZmYp":
                case "YpZp":
                    PlaneAxis = QuanLib.Minecraft.PlaneAxis.ZY;
                    NormalFacing = Facing.Xm;
                    PlaneCoordinate = startPosition.X;
                    break;
                case "XpZp":
                case "ZpXm":
                case "XmZm":
                case "ZmXp":
                    PlaneAxis = QuanLib.Minecraft.PlaneAxis.XZ;
                    NormalFacing = Facing.Yp;
                    PlaneCoordinate = startPosition.Y;
                    break;
                case "XpZm":
                case "ZmXm":
                case "XmZp":
                case "ZpXp":
                    PlaneAxis = QuanLib.Minecraft.PlaneAxis.XZ;
                    NormalFacing = Facing.Ym;
                    PlaneCoordinate = startPosition.Y;
                    break;
                default:
                    throw new ArgumentException("xFacing 与 yFacing 不应该在同一轴向上");
            }

            int top, bottom, left, right;
            switch (yFacing)
            {
                case Facing.Xp:
                    top = startPosition.X;
                    bottom = startPosition.X + height - 1;
                    break;
                case Facing.Xm:
                    top = startPosition.X;
                    bottom = startPosition.X - height - 1;
                    break;
                case Facing.Yp:
                    top = startPosition.Y;
                    bottom = startPosition.Y + height - 1;
                    break;
                case Facing.Ym:
                    top = startPosition.Y;
                    bottom = startPosition.Y - height - 1;
                    break;
                case Facing.Zp:
                    top = startPosition.Z;
                    bottom = startPosition.Z + height - 1;
                    break;
                case Facing.Zm:
                    top = startPosition.Z;
                    bottom = startPosition.Z - height - 1;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            switch (xFacing)
            {
                case Facing.Xp:
                    left = startPosition.X;
                    right = startPosition.X + height - 1;
                    break;
                case Facing.Xm:
                    left = startPosition.X;
                    right = startPosition.X - height - 1;
                    break;
                case Facing.Yp:
                    left = startPosition.Y;
                    right = startPosition.Y + height - 1;
                    break;
                case Facing.Ym:
                    left = startPosition.Y;
                    right = startPosition.Y - height - 1;
                    break;
                case Facing.Zp:
                    left = startPosition.Z;
                    right = startPosition.Z + height - 1;
                    break;
                case Facing.Zm:
                    left = startPosition.Z;
                    right = startPosition.Z - height - 1;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            StartPosition = startPosition;
            XFacing = xFacing;
            YFacing = yFacing;
            Width = width;
            Height = height;
            DefaultBackgroundBlcokID = "minecraft:smooth_stone";

            _chunks = new();

            ThrowHelper.ArgumentOutOfRange(-64, 319, EndPosition.Y, "EndPosition.Y");
        }

        private const string LIGHT_BLOCK = "minecraft:light";

        private const string AIR_BLOCK = "minecraft:air";

        private readonly List<ChunkPos> _chunks;

        public BlockPos StartPosition { get; }

        public BlockPos EndPosition => ScreenPos2WorldPos(new(Width - 1, Height - 1));

        public BlockPos CenterPosition => ScreenPos2WorldPos(ScreenCenterPosition);

        public Point ScreenCenterPosition => new(Width / 2, Height / 2);

        public PlaneAxis PlaneAxis { get; }

        public Facing NormalFacing { get; }

        public int PlaneCoordinate { get; }

        public Facing XFacing { get; }

        public Facing YFacing { get; }

        public int Width { get; }

        public int Height { get; }

        public Size Size => new(Width, Height);

        public int TotalPixels => Width * Height;

        public string DefaultBackgroundBlcokID { get; set; }

        public ScreenOutputHandler OutputHandler
        {
            get
            {
                _OutputHandler ??= new(this);
                return _OutputHandler;
            }
        }
        private ScreenOutputHandler? _OutputHandler;

        private bool Check(string blockID)
        {
            ArgumentNullException.ThrowIfNull(blockID, nameof(blockID));

            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    if (!sender.ConditionalBlock(ScreenPos2WorldPos(new(x, y)), blockID))
                        return false;
                }

            return true;
        }

        private bool Fill(string blockID, bool check = false)
        {
            if (check && !CheckAir())
            {
                return false;
            }

            OutputHandler.HandleOutput(new HashBlockFrame(Width, Height, blockID));
            return true;
        }

        private bool FillDouble(string blockID, bool check = false)
        {
            BlockPos position1 = StartPosition;
            BlockPos position2 = StartPosition;
            switch (NormalFacing)
            {
                case Facing.Xp:
                case Facing.Xm:
                    position1.X--;
                    position2.X++;
                    break;
                case Facing.Yp:
                case Facing.Ym:
                    position1.Y--;
                    position2.Y++;
                    break;
                case Facing.Zp:
                case Facing.Zm:
                    position1.Z--;
                    position2.Z++;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            Screen screen1 = new(position1, Width, Height, XFacing, YFacing);
            Screen screen2 = new(position2, Width, Height, XFacing, YFacing);

            if (check && (!screen1.CheckAir() || !screen2.CheckAir()))
                return false;

            screen1.Fill(blockID);
            screen2.Fill(blockID);

            return true;
        }

        public bool Fill(bool check = false)
        {
            return Fill(DefaultBackgroundBlcokID, check);
        }

        public bool CheckAir()
        {
            return Check(AIR_BLOCK);
        }

        public void Clear()
        {
            Fill(AIR_BLOCK);
        }

        public void Start()
        {
            LoadScreenChunks();
            Fill();
        }

        public void Stop()
        {
            if (TestLight())
                CloseLight();
            UnloadScreenChunks();
            Clear();
        }

        public void OpenLight()
        {
            FillDouble(LIGHT_BLOCK);
        }

        public void CloseLight()
        {
            FillDouble(AIR_BLOCK);
        }

        public bool TestLight()
        {
            BlockPos position1 = StartPosition;
            BlockPos position2 = StartPosition;
            switch (NormalFacing)
            {
                case Facing.Xp:
                case Facing.Xm:
                    position1.X--;
                    position2.X++;
                    break;
                case Facing.Yp:
                case Facing.Ym:
                    position1.Y--;
                    position2.Y++;
                    break;
                case Facing.Zp:
                case Facing.Zm:
                    position1.Z--;
                    position2.Z++;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            if (sender.ConditionalBlock(position1, LIGHT_BLOCK))
            {
                return true;
            }
            else if (sender.ConditionalBlock(position2, LIGHT_BLOCK))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LoadScreenChunks()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var blockPos = ScreenPos2WorldPos(new(x, y));
                    ChunkPos chunkPos = MinecraftUtil.BlockPos2ChunkPos(blockPos);
                    if (!_chunks.Contains(chunkPos))
                        _chunks.Add(chunkPos);
                }

            foreach (var chunk in _chunks)
                MCOS.Instance.MinecraftInstance.CommandSender.AddForceloadChunk(MinecraftUtil.ChunkPos2BlockPos(chunk));
        }

        public void UnloadScreenChunks()
        {
            foreach (var chunk in _chunks)
                MCOS.Instance.MinecraftInstance.CommandSender.RemoveForceloadChunk(MinecraftUtil.ChunkPos2BlockPos(chunk));

            _chunks.Clear();
        }

        public BlockPos ScreenPos2WorldPos(Point position, int offset = 0)
        {
            int x = 0;
            int y = 0;
            int z = 0;

            switch (XFacing)
            {
                case Facing.Xp:
                    x = StartPosition.X + position.X;
                    break;
                case Facing.Xm:
                    x = StartPosition.X - position.X;
                    break;
                case Facing.Yp:
                    y = StartPosition.Y + position.X;
                    break;
                case Facing.Ym:
                    y = StartPosition.Y - position.X;
                    break;
                case Facing.Zp:
                    z = StartPosition.Z + position.X;
                    break;
                case Facing.Zm:
                    z = StartPosition.Z - position.X;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            switch (YFacing)
            {
                case Facing.Xp:
                    x = StartPosition.X + position.Y;
                    break;
                case Facing.Xm:
                    x = StartPosition.X - position.Y;
                    break;
                case Facing.Yp:
                    y = StartPosition.Y + position.Y;
                    break;
                case Facing.Ym:
                    y = StartPosition.Y - position.Y;
                    break;
                case Facing.Zp:
                    z = StartPosition.Z + position.Y;
                    break;
                case Facing.Zm:
                    z = StartPosition.Z - position.Y;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            switch (NormalFacing)
            {
                case Facing.Xp:
                    x = StartPosition.X + offset;
                    break;
                case Facing.Xm:
                    x = StartPosition.X - offset;
                    break;
                case Facing.Yp:
                    y = StartPosition.Y + offset;
                    break;
                case Facing.Ym:
                    y = StartPosition.Y - offset;
                    break;
                case Facing.Zp:
                    z = StartPosition.Z + offset;
                    break;
                case Facing.Zm:
                    z = StartPosition.Z - offset;
                    break;
                default:
                    break;
            }

            return new(x, y, z);
        }

        public Point WorldPos2ScreenPos(BlockPos position)
        {
            var x = XFacing switch
            {
                Facing.Xp => position.X - StartPosition.X,
                Facing.Xm => StartPosition.X - position.X,
                Facing.Yp => position.Y - StartPosition.Y,
                Facing.Ym => StartPosition.Y - position.Y,
                Facing.Zp => position.Z - StartPosition.Z,
                Facing.Zm => StartPosition.Z - position.Z,
                _ => throw new InvalidOperationException()
            };
            var y = YFacing switch
            {
                Facing.Xp => position.X - StartPosition.X,
                Facing.Xm => StartPosition.X - position.X,
                Facing.Yp => position.Y - StartPosition.Y,
                Facing.Ym => StartPosition.Y - position.Y,
                Facing.Zp => position.Z - StartPosition.Z,
                Facing.Zm => StartPosition.Z - position.Z,
                _ => throw new InvalidOperationException()
            };

            return new(x, y);
        }

        public double GetPlaneDistance<T>(T position) where T : IVector3<double>
        {
            ArgumentNullException.ThrowIfNull(position, nameof(position));

            return NormalFacing switch
            {
                Facing.Xp => position.X - PlaneCoordinate,
                Facing.Xm => PlaneCoordinate - position.X,
                Facing.Yp => position.Y - PlaneCoordinate,
                Facing.Ym => PlaneCoordinate - position.Y,
                Facing.Zp => position.Z - PlaneCoordinate,
                Facing.Zm => PlaneCoordinate - position.Z,
                _ => throw new InvalidOperationException(),
            };
        }

        public bool IncludedOnScreen(Point position)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < Width && position.Y < Height;
        }

        public bool IncludedOnScreen(BlockPos position)
        {
            bool isScreenPlane = NormalFacing switch
            {
                Facing.Xp or Facing.Xm => position.X == PlaneCoordinate,
                Facing.Yp or Facing.Ym => position.Y == PlaneCoordinate,
                Facing.Zp or Facing.Zm => position.Z == PlaneCoordinate,
                _ => throw new InvalidOperationException()
            };
            return isScreenPlane && IncludedOnScreen(WorldPos2ScreenPos(position));
        }

        public override string ToString()
        {
            return $"StartPosition={StartPosition}, Width={Width}, Height={Height}, XFacing={XFacing}, YFacing={YFacing}";
        }

        public static Screen CreateScreen(BlockPos startPosition, BlockPos endPosition, Facing normalFacing)
        {
            ThrowHelper.ArgumentOutOfRange(-64, 319, startPosition.Y, "startPosition.Y");
            ThrowHelper.ArgumentOutOfRange(-64, 319, endPosition.Y, "endPosition.Y");

            Facing xFacing, yFacing;
            int width, height;
            if (startPosition.X == endPosition.X && (normalFacing == Facing.Xp || normalFacing == Facing.Xm))
            {
                bool swap = false;
                switch (normalFacing)
                {
                    case Facing.Xp:
                        if (startPosition.Z > endPosition.Z && startPosition.Y > endPosition.Y)
                        {
                            xFacing = Facing.Zm;
                            yFacing = Facing.Ym;
                        }
                        else if (startPosition.Z <= endPosition.Z && startPosition.Y <= endPosition.Y)
                        {
                            xFacing = Facing.Zp;
                            yFacing = Facing.Yp;
                        }
                        else if (startPosition.Z > endPosition.Z && startPosition.Y <= endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Yp;
                            yFacing = Facing.Zm;
                        }
                        else if (endPosition.Z <= endPosition.Z && startPosition.Y > endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Ym;
                            yFacing = Facing.Zp;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    case Facing.Xm:
                        if (startPosition.Z > endPosition.Z && startPosition.Y > endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Ym;
                            yFacing = Facing.Zm;
                        }
                        else if (startPosition.Z <= endPosition.Z && startPosition.Y <= endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Yp;
                            yFacing = Facing.Zp;
                        }
                        else if (startPosition.Z > endPosition.Z && startPosition.Y <= endPosition.Y)
                        {
                            xFacing = Facing.Zm;
                            yFacing = Facing.Yp;
                        }
                        else if (endPosition.Z <= endPosition.Z && startPosition.Y > endPosition.Y)
                        {
                            xFacing = Facing.Zp;
                            yFacing = Facing.Ym;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (swap)
                {
                    width = Math.Abs(startPosition.Y - endPosition.Y) + 1;
                    height = Math.Abs(startPosition.Z - endPosition.Z) + 1;
                }
                else
                {
                    width = Math.Abs(startPosition.Z - endPosition.Z) + 1;
                    height = Math.Abs(startPosition.Y - endPosition.Y) + 1;
                }
            }
            else if (startPosition.Y == endPosition.Y && (normalFacing == Facing.Yp || normalFacing == Facing.Ym))
            {
                bool swap = false;
                switch (normalFacing)
                {
                    case Facing.Yp:
                        if (startPosition.X > endPosition.X && startPosition.Z > endPosition.Z)
                        {
                            xFacing = Facing.Xm;
                            yFacing = Facing.Zm;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Z <= endPosition.Z)
                        {
                            xFacing = Facing.Xp;
                            yFacing = Facing.Zp;
                        }
                        else if (startPosition.X > endPosition.X && startPosition.Z <= endPosition.Z)
                        {
                            swap = true;
                            xFacing = Facing.Zp;
                            yFacing = Facing.Xm;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Z > endPosition.Z)
                        {
                            swap = true;
                            xFacing = Facing.Zm;
                            yFacing = Facing.Xp;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    case Facing.Ym:
                        if (startPosition.X > endPosition.X && startPosition.Z > endPosition.Z)
                        {
                            swap = true;
                            xFacing = Facing.Zm;
                            yFacing = Facing.Xm;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Z <= endPosition.Z)
                        {
                            swap = true;
                            xFacing = Facing.Zp;
                            yFacing = Facing.Xp;
                        }
                        else if (startPosition.X > endPosition.X && startPosition.Z <= endPosition.Z)
                        {
                            xFacing = Facing.Xm;
                            yFacing = Facing.Zp;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Z > endPosition.Z)
                        {
                            xFacing = Facing.Xp;
                            yFacing = Facing.Zm;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (swap)
                {
                    width = Math.Abs(startPosition.Z - endPosition.Z) + 1;
                    height = Math.Abs(startPosition.X - endPosition.X) + 1;
                }
                else
                {
                    width = Math.Abs(startPosition.X - endPosition.X) + 1;
                    height = Math.Abs(startPosition.Z - endPosition.Z) + 1;
                }
            }
            else if (startPosition.Z == endPosition.Z && (normalFacing == Facing.Zp || normalFacing == Facing.Zm))
            {
                bool swap = false;
                switch (normalFacing)
                {
                    case Facing.Zp:
                        if (startPosition.X > endPosition.X && startPosition.Y > endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Ym;
                            yFacing = Facing.Xm;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Y <= endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Yp;
                            yFacing = Facing.Xp;
                        }
                        else if (startPosition.X > endPosition.X && startPosition.Y <= endPosition.Y)
                        {
                            xFacing = Facing.Xm;
                            yFacing = Facing.Yp;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Y > endPosition.Y)
                        {
                            xFacing = Facing.Xp;
                            yFacing = Facing.Ym;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    case Facing.Zm:
                        if (startPosition.X > endPosition.X && startPosition.Y > endPosition.Y)
                        {
                            xFacing = Facing.Xm;
                            yFacing = Facing.Ym;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Y <= endPosition.Y)
                        {
                            xFacing = Facing.Xp;
                            yFacing = Facing.Yp;
                        }
                        else if (startPosition.X > endPosition.X && startPosition.Y <= endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Yp;
                            yFacing = Facing.Xm;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Y > endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Ym;
                            yFacing = Facing.Xp;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (swap)
                {
                    width = Math.Abs(startPosition.Y - endPosition.Y) + 1;
                    height = Math.Abs(startPosition.X - endPosition.X) + 1;
                }
                else
                {
                    width = Math.Abs(startPosition.X - endPosition.X) + 1;
                    height = Math.Abs(startPosition.Y - endPosition.Y) + 1;
                }
            }
            else
            {
                throw new ArgumentException("屏幕的起始点与截止点不在一个平面");
            }

            return new(startPosition, width, height, xFacing, yFacing);
        }

        public static bool Replace(Screen? oldScreen, Screen newScreen, bool check = false)
        {
            ArgumentNullException.ThrowIfNull(newScreen, nameof(newScreen));

            if (oldScreen is null || oldScreen.OutputHandler.LastFrame is null)
            {
                return newScreen.Fill(check);
            }

            if (oldScreen.PlaneAxis != newScreen.PlaneAxis || oldScreen.PlaneCoordinate != newScreen.PlaneCoordinate)
            {
                if (!newScreen.Fill(check))
                    return false;

                oldScreen.Clear();
                return true;
            }

            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            if (oldScreen.DefaultBackgroundBlcokID == newScreen.DefaultBackgroundBlcokID &&
                oldScreen.StartPosition == oldScreen.StartPosition &&
                oldScreen.XFacing == newScreen.XFacing &&
                oldScreen.YFacing == newScreen.YFacing)
            {
                if (newScreen.Width == oldScreen.Width && newScreen.Height == oldScreen.Height)
                    return true;

                BlockFrame? oldFrame = null;
                if (newScreen.OutputHandler.LastFrame is null)
                    newScreen.OutputHandler.LastFrame = new HashBlockFrame(newScreen.Width, newScreen.Height, newScreen.DefaultBackgroundBlcokID);

                if (newScreen.Width > oldScreen.Width)
                {
                    if (check)
                    {
                        for (int x = oldScreen.Width; x < newScreen.Width; x++)
                            for (int y = 0; y < newScreen.Height; y++)
                            {
                                if (!sender.ConditionalBlock(newScreen.ScreenPos2WorldPos(new(x, y)), AIR_BLOCK))
                                    return false;
                            }
                    }

                    for (int x = oldScreen.Width; x < newScreen.Width; x++)
                        for (int y = 0; y < newScreen.Height; y++)
                            newScreen.OutputHandler.LastFrame[x, y] = AIR_BLOCK;
                }
                else if (newScreen.Width < oldScreen.Width)
                {
                    oldFrame ??= oldScreen.OutputHandler.LastFrame.Clone();
                    for (int x = newScreen.Width; x < oldScreen.Width; x++)
                        for (int y = 0; y < oldScreen.Height; y++)
                            oldFrame[x, y] = AIR_BLOCK;
                }

                if (newScreen.Height > oldScreen.Height)
                {
                    if (check)
                    {
                        for (int y = oldScreen.Height; y < newScreen.Height; y++)
                            for (int x = 0; x < newScreen.Width; x++)
                            {

                                if (!sender.ConditionalBlock(newScreen.ScreenPos2WorldPos(new(x, y)), AIR_BLOCK))
                                    return false;
                            }
                    }

                    for (int y = oldScreen.Height; y < newScreen.Height; y++)
                        for (int x = 0; x < newScreen.Width; x++)
                            newScreen.OutputHandler.LastFrame[x, y] = AIR_BLOCK;
                }
                else if (newScreen.Height < oldScreen.Height)
                {
                    oldFrame ??= oldScreen.OutputHandler.LastFrame.Clone();
                    for (int y = newScreen.Height; y < oldScreen.Height; y++)
                        for (int x = 0; x < oldScreen.Width; x++)
                            oldFrame[x, y] = AIR_BLOCK;
                }

                newScreen.Fill();
                if (oldFrame is not null)
                    oldScreen.OutputHandler.HandleOutput(oldFrame);

                return true;
            }
            else
            {
                oldScreen.Clear();
                return newScreen.Fill(check);
            }
        }
    }
}
