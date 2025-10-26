using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.Game;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.TickLoop;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    public class ScreenController : ITickUpdatable
    {
        private const string AIR_BLOCK = "minecraft:air";

        public ScreenController(ScreenContext owner, Screen screen, int maxBackLayers, int maxFrontLayers)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
            _oldSnapshot = _newSnapshot = new(screen, maxBackLayers, maxFrontLayers);

            Move += OnMove;
            Translate += OnTranslate;
            Advance += OnAdvance;
            Resize += OnResize;
            PlaneFacingChanged += OnPlaneFacingChanged;
            NormalFacingChanged += OnNormalFacingChanged;
            MaxBackLayersChanged += OnMaxBackLayersChanged;
            MaxFrontLayersChanged += OnMaxFrontLayersChanged;
            ScreenChanged += OnScreenChanged;
            ScreenSnapshotChanged += OnScreenSnapshotChanged;
            CheckRangeFailed += OnCheckRangeFailed;
        }

        private readonly ScreenContext _owner;

        private ScreenSnapshot _oldSnapshot;

        private ScreenSnapshot _newSnapshot;

        public event EventHandler<ScreenController, ValueChangedEventArgs<Vector3<int>>> Move;

        public event EventHandler<ScreenController, EventArgs<Point>> Translate;

        public event EventHandler<ScreenController, EventArgs<int>> Advance;

        public event EventHandler<ScreenController, ValueChangedEventArgs<Size>> Resize;

        public event EventHandler<ScreenController, ValueChangedEventArgs<PlaneFacing>> PlaneFacingChanged;

        public event EventHandler<ScreenController, ValueChangedEventArgs<Facing>> NormalFacingChanged;

        public event EventHandler<ScreenController, ValueChangedEventArgs<int>> MaxBackLayersChanged;

        public event EventHandler<ScreenController, ValueChangedEventArgs<int>> MaxFrontLayersChanged;

        public event EventHandler<ScreenController, ValueChangedEventArgs<Screen>> ScreenChanged;

        public event EventHandler<ScreenController, ValueChangedEventArgs<ScreenSnapshot>> ScreenSnapshotChanged;

        public event EventHandler<ScreenController, EventArgs<FacingRange>> CheckRangeFailed;

        protected virtual void OnMove(ScreenController sender, ValueChangedEventArgs<Vector3<int>> e) { }

        protected virtual void OnTranslate(ScreenController sender, EventArgs<Point> e) { }

        protected virtual void OnAdvance(ScreenController sender, EventArgs<int> e) { }

        protected virtual void OnResize(ScreenController sender, ValueChangedEventArgs<Size> e) { }

        protected virtual void OnPlaneFacingChanged(ScreenController sender, ValueChangedEventArgs<PlaneFacing> e) { }

        protected virtual void OnNormalFacingChanged(ScreenController sender, ValueChangedEventArgs<Facing> e) { }

        protected virtual void OnMaxBackLayersChanged(ScreenController sender, ValueChangedEventArgs<int> e) { }

        protected virtual void OnMaxFrontLayersChanged(ScreenController sender, ValueChangedEventArgs<int> e) { }

        protected virtual void OnScreenChanged(ScreenController sender, ValueChangedEventArgs<Screen> e) { }

        protected virtual void OnScreenSnapshotChanged(ScreenController sender, ValueChangedEventArgs<ScreenSnapshot> e) { }

        protected virtual void OnCheckRangeFailed(ScreenController sender, EventArgs<FacingRange> e) { }

        protected virtual void HandleScreenEvent(ScreenSnapshot oldSnapshot, ScreenSnapshot newSnapshot)
        {
            if (oldSnapshot == newSnapshot)
                return;

            Screen oldScreen = oldSnapshot.Screen;
            Screen newScreen = newSnapshot.Screen;
            Vector3<int> oldPos = oldScreen.StartPosition;
            Vector3<int> newPos = newScreen.StartPosition;
            Size oldSize = new(oldScreen.Width, oldScreen.Height);
            Size newSize = new(newScreen.Width, newScreen.Height);
            PlaneFacing oldFacing = new(oldScreen.XFacing, oldScreen.YFacing);
            PlaneFacing newFacing = new(newScreen.XFacing, newScreen.YFacing);

            ScreenSnapshotChanged.Invoke(this, new(oldSnapshot, newSnapshot));

            if (oldSnapshot.MaxBackLayers != newSnapshot.MaxBackLayers)
            {
                MaxBackLayersChanged.Invoke(this, new(oldSnapshot.MaxBackLayers, newSnapshot.MaxBackLayers));
            }

            if (oldSnapshot.MaxFrontLayers != newSnapshot.MaxFrontLayers)
            {
                MaxFrontLayersChanged.Invoke(this, new(oldSnapshot.MaxFrontLayers, newSnapshot.MaxFrontLayers));
            }

            if (oldScreen == newScreen)
                return;

            ScreenChanged.Invoke(this, new(oldScreen, newScreen));

            if (oldPos != newPos)
            {
                Move.Invoke(this, new(oldPos, newPos));

                if (oldFacing == newFacing)
                {
                    int xFacingIndex = Math.Abs((int)newFacing.XFacing) - 1;
                    int yFacingIndex = Math.Abs((int)newFacing.YFacing) - 1;
                    int normalFacingIndex = Math.Abs((int)newFacing.NormalFacing) - 1;

                    if (oldPos[normalFacingIndex] == newPos[normalFacingIndex] &&
                        (oldPos[xFacingIndex] != newPos[xFacingIndex] ||
                        oldPos[yFacingIndex] != newPos[yFacingIndex]))
                    {
                        int x = newPos[xFacingIndex] - oldPos[xFacingIndex];
                        int y = newPos[yFacingIndex] - oldPos[yFacingIndex];

                        switch (newFacing.XFacing)
                        {
                            case Facing.Xm:
                            case Facing.Ym:
                            case Facing.Zm:
                                x = -x;
                                break;
                        };

                        switch (newFacing.YFacing)
                        {
                            case Facing.Xm:
                            case Facing.Ym:
                            case Facing.Zm:
                                y = -y;
                                break;
                        };

                        Translate.Invoke(this, new(new(x, y)));
                    }

                    if (oldPos[normalFacingIndex] != newPos[normalFacingIndex] &&
                        (oldPos[xFacingIndex] == newPos[xFacingIndex] &&
                        oldPos[yFacingIndex] == newPos[yFacingIndex]))
                    {
                        int offset = newPos[normalFacingIndex] - oldPos[normalFacingIndex];

                        switch (newFacing.NormalFacing)
                        {
                            case Facing.Xm:
                            case Facing.Ym:
                            case Facing.Zm:
                                offset = -offset;
                                break;
                        };

                        Advance.Invoke(this, new(offset));
                    }
                }
            }

            if (oldSize != newSize)
            {
                Resize.Invoke(this, new(oldSize, newSize));
            }

            if (oldFacing != newFacing)
            {
                PlaneFacingChanged.Invoke(this, new(oldFacing, newFacing));

                if (oldFacing.NormalFacing != newFacing.NormalFacing)
                {
                    NormalFacingChanged.Invoke(this, new(oldFacing.NormalFacing, newFacing.NormalFacing));
                }
            }
        }

        public void OnTickUpdate(int tick)
        {
            if (_oldSnapshot == _newSnapshot)
                return;

            ScreenSnapshot oldSnapshot = _oldSnapshot;
            ScreenSnapshot newSnapshot = _newSnapshot;
            CubeRange oldRange = oldSnapshot.ScreenRange.Normalize();
            CubeRange newRange = newSnapshot.ScreenRange.Normalize();
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;

            List<FacingRange> checkAirFailed = [];
            FacingRange[] additionRanges = GetAdditionRanges(oldRange, newRange);

            foreach (FacingRange facingRange in additionRanges)
            {
                if (!sender.CheckRangeBlock(facingRange.Range.StartPosition, facingRange.Range.EndPosition, AIR_BLOCK))
                    checkAirFailed.Add(facingRange);
            }

            if (checkAirFailed.Count != 0)
            {
                foreach (FacingRange facingRange in checkAirFailed)
                    CheckRangeFailed.Invoke(this, new(facingRange));

                _newSnapshot = oldSnapshot;
                return;
            }

            FacingRange[] reductionRanges = GetReductionRanges(oldRange, newRange);

            foreach(FacingRange facingRange in reductionRanges)
                sender.Fill(facingRange.Range.StartPosition, facingRange.Range.EndPosition, AIR_BLOCK, true);

            _oldSnapshot = newSnapshot;
            HandleScreenEvent(oldSnapshot, newSnapshot);
        }

        public Screen GetScreen()
        {
            return _oldSnapshot.Screen;
        }

        public int GetMaxBackLayers()
        {
            return _oldSnapshot.MaxBackLayers;
        }

        public int GetMaxFrontLayers()
        {
            return _oldSnapshot.MaxFrontLayers;
        }

        public Screen SetPosition(Vector3<int> position) => _newSnapshot.Screen = _newSnapshot.Screen.SetPosition(position);

        public Screen SetPosition(int x, int y, int z) => _newSnapshot.Screen = _newSnapshot.Screen.SetPosition(x, y, z);

        public Screen SetSize(int width, int height) => _newSnapshot.Screen = _newSnapshot.Screen.SetSize(width, height);

        public Screen SetSize(Size size) => _newSnapshot.Screen = _newSnapshot.Screen.SetSize(size);

        public Screen UpRotate() => _newSnapshot.Screen = _newSnapshot.Screen.UpRotate();

        public Screen DownRotate() => _newSnapshot.Screen = _newSnapshot.Screen.DownRotate();

        public Screen LeftRotate() => _newSnapshot.Screen = _newSnapshot.Screen.LeftRotate();

        public Screen RightRotate() => _newSnapshot.Screen = _newSnapshot.Screen.RightRotate();

        public Screen ClockwiseRotate() => _newSnapshot.Screen = _newSnapshot.Screen.ClockwiseRotate();

        public Screen CounterclockwiseRotate() => _newSnapshot.Screen = _newSnapshot.Screen.CounterclockwiseRotate();

        public Screen ApplyTranslate(int dx, int dy) => _newSnapshot.Screen = _newSnapshot.Screen.ApplyTranslate(dx, dy);

        public Screen ApplyTranslate(Point offset) => _newSnapshot.Screen = _newSnapshot.Screen.ApplyTranslate(offset);

        public Screen ApplyAdvance(int offset) => _newSnapshot.Screen = _newSnapshot.Screen.ApplyAdvance(offset);

        public void SetMaxBackLayers(int layers)
        {
            ThrowHelper.ArgumentOutOfRange(0, 64, layers, nameof(layers));

            _newSnapshot.MaxBackLayers = layers;
        }

        public void SetMaxFrontLayers(int layers)
        {
            ThrowHelper.ArgumentOutOfRange(0, 64, layers, nameof(layers));

            _newSnapshot.MaxFrontLayers = layers;
        }

        public void ClearScreenRange()
        {
            CubeRange range = _oldSnapshot.ScreenRange;
            MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.Fill(range.StartPosition, range.EndPosition, AIR_BLOCK, true);
        }

        private static FacingRange[] GetReductionRanges(CubeRange oldRange, CubeRange newRange)
        {
            return GetAdditionRanges(newRange, oldRange);
        }

        private static FacingRange[] GetAdditionRanges(CubeRange oldRange, CubeRange newRange)
        {
            if (oldRange.IsContains(newRange, false))
                return [];

            List<FacingRange> result = [];

            if (newRange.StartPosition.X > oldRange.EndPosition.X)
                return [new(Facing.Xp, newRange)];
            else if (newRange.EndPosition.X < oldRange.StartPosition.X)
                return [new(Facing.Xm, newRange)];
            else if (newRange.StartPosition.Y > oldRange.EndPosition.Y)
                return [new(Facing.Yp, newRange)];
            else if (newRange.EndPosition.Y < oldRange.StartPosition.Y)
                return [new(Facing.Ym, newRange)];
            else if (newRange.StartPosition.Z > oldRange.EndPosition.Z)
                return [new(Facing.Zp, newRange)];
            else if (newRange.EndPosition.Z < oldRange.StartPosition.Z)
                return [new(Facing.Zm, newRange)];

            if (newRange.StartPosition.X < oldRange.StartPosition.X)
            {
                result.Add(new(Facing.Xm, new CubeRange(
                    newRange.StartPosition.X, newRange.StartPosition.Y, newRange.StartPosition.Z,
                    oldRange.StartPosition.X - 1, newRange.EndPosition.Y, newRange.EndPosition.Z
                )));
            }

            if (newRange.EndPosition.X > oldRange.EndPosition.X)
            {
                result.Add(new(Facing.Xp, new CubeRange(
                    oldRange.EndPosition.X + 1, newRange.StartPosition.Y, newRange.StartPosition.Z,
                    newRange.EndPosition.X, newRange.EndPosition.Y, newRange.EndPosition.Z
                )));
            }

            if (newRange.StartPosition.Y < oldRange.StartPosition.Y)
            {
                result.Add(new(Facing.Ym, new CubeRange(
                    newRange.StartPosition.X, newRange.StartPosition.Y, newRange.StartPosition.Z,
                    newRange.EndPosition.X, oldRange.StartPosition.Y - 1, newRange.EndPosition.Z
                )));
            }

            if (newRange.EndPosition.Y > oldRange.EndPosition.Y)
            {
                result.Add(new(Facing.Yp, new CubeRange(
                    newRange.StartPosition.X, oldRange.EndPosition.Y + 1, newRange.StartPosition.Z,
                    newRange.EndPosition.X, newRange.EndPosition.Y, newRange.EndPosition.Z
                )));
            }

            if (newRange.StartPosition.Z < oldRange.StartPosition.Z)
            {
                result.Add(new(Facing.Zm, new CubeRange(
                    newRange.StartPosition.X, newRange.StartPosition.Y, newRange.StartPosition.Z,
                    newRange.EndPosition.X, newRange.EndPosition.Y, oldRange.StartPosition.Z - 1
                )));
            }

            if (newRange.EndPosition.Z > oldRange.EndPosition.Z)
            {
                result.Add(new(Facing.Zp, new CubeRange(
                    newRange.StartPosition.X, newRange.StartPosition.Y, oldRange.EndPosition.Z + 1,
                    newRange.EndPosition.X, newRange.EndPosition.Y, newRange.EndPosition.Z
                )));
            }

            return result.ToArray();
        }

        public readonly record struct FacingRange(Facing Facing, CubeRange Range);
    }
}
