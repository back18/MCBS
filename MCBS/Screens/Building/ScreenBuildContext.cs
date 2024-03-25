using static MCBS.Config.ConfigManager;
using QuanLib.Game;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.TickLoop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens.Building
{
    public class ScreenBuildContext : ITickUpdatable
    {
        public ScreenBuildContext(string playerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(playerName, nameof(playerName));

            PlayerName = playerName;
        }

        public string PlayerName { get; }

        public ScreenBuildState BuildState { get; private set; }

        public Screen Screen => _Screen ?? throw new InvalidOperationException("屏幕未被初始化");
        private Screen? _Screen;

        public void OnTickUpdate(int tick)
        {
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;

            if (BuildState != ScreenBuildState.Active)
            {
                return;
            }

            if (!sender.TryGetEntityPosition(PlayerName, out var position) ||
                !sender.TryGetEntityRotation(PlayerName, out var rotation) ||
                !sender.TryGetPlayerSelectedItem(PlayerName, out var item) ||
                item.ID != ScreenConfig.RightClickItemID ||
                item.GetItemName() != ScreenConfig.ScreenBuilderItemName)
            {
                BuildState = ScreenBuildState.Canceled;
                return;
            }

            position.Y += 1.625;
            Facing playerFacing = (rotation.Pitch <= -80 || rotation.Pitch >= 80) ? rotation.PitchFacing : rotation.YawFacing;
            Vector3<int> anchorPosition = position.ToIntVector3().Offset(playerFacing, 1);
            if (playerFacing == Facing.Ym)
                anchorPosition.Y -= 1;

            Facing xFacing, yFacing;
            switch (playerFacing)
            {
                case Facing.Xp:
                case Facing.Xm:
                case Facing.Zp:
                case Facing.Zm:
                    yFacing = Facing.Ym;
                    xFacing = playerFacing.LeftRotate(Facing.Yp);
                    break;
                case Facing.Yp:
                    yFacing = rotation.YawFacing;
                    xFacing = yFacing.LeftRotate(Facing.Yp);
                    break;
                case Facing.Ym:
                    yFacing = rotation.YawFacing.Reverse();
                    xFacing = yFacing.RightRotate(Facing.Yp);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            Vector3<int> startPosition = anchorPosition.Offset(yFacing, -ScreenConfig.InitialHeight + 1);
            Screen screen = new(startPosition, ScreenConfig.InitialWidth, ScreenConfig.InitialHeight, xFacing, yFacing);

            if (!screen.InAltitudeRange(ScreenConfig.MinAltitude, ScreenConfig.MaxAltitude))
            {
                sender.ShowTitle(PlayerName, 0, 10, 10, $"屏幕位置超出了高度范围", TextColor.Red);
                sender.ShowSubTitle(PlayerName, 0, 10, 10, $"最小高度:{ScreenConfig.MinAltitude} 最大高度:{ScreenConfig.MaxAltitude}");
                return;
            }

            _Screen = screen;
            sender.ShowTitle(PlayerName, 0, 10, 10, "屏幕构建器");
            sender.ShowSubTitle(PlayerName, 0, 10, 10, $"方向:{playerFacing.ToChineseString()} 位置:{anchorPosition} 尺寸:[{ScreenConfig.InitialWidth}, {ScreenConfig.InitialHeight}]");

            if (MinecraftBlockScreen.Instance.CursorManager.GetOrCreate(PlayerName).ClickReader.ReadClick().IsRightClick)
            {
                BuildState = ScreenBuildState.Completed;
                return;
            }
        }
    }
}
