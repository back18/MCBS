using MCBS.BlockForms.Utility;
using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using MCBS.Events;
using QuanLib.Core;
using QuanLib.Game;
using QuanLib.Minecraft.Blocks;
using QuanLib.TextFormat;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem
{
    public class FileBox : ContainerControl<Control>
    {
        public FileBox(FileSystemInfo fileSystemInfo)
        {
            ArgumentNullException.ThrowIfNull(fileSystemInfo, nameof(fileSystemInfo));

            FileSystemInfo = fileSystemInfo;

            BorderWidth = 0;
            MinSize = new(80 + 64 + 40 + 40 + 3, 16);
            ClientSize = MinSize;
            Skin.SetBackgroundColor(BlockManager.Concrete.LightBlue, ControlState.Hover);
            Skin.SetBackgroundColor(BlockManager.Concrete.Blue, ControlState.Selected, ControlState.Hover | ControlState.Selected);

            FileName_IconSubtitleBox = new();
            WriteDate_Label = new();
            WriteTime_Label = new();
            Size_Label = new();
        }

        private static readonly BytesFormatter _bytesFormatter = new(AbbreviationBytesUnitText.Default);

        private readonly IconSubtitleBox<Rgba32> FileName_IconSubtitleBox;

        private readonly Label WriteDate_Label;

        private readonly Label WriteTime_Label;

        private readonly Label Size_Label;

        public FileSystemInfo FileSystemInfo { get; }

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(FileName_IconSubtitleBox);
            FileName_IconSubtitleBox.BorderWidth = 0;
            FileName_IconSubtitleBox.Stretch = Direction.Right;
            FileName_IconSubtitleBox.Icon_PictureBox.SetImage(PathIconManager.GetIcon(FileSystemInfo.FullName));
            FileName_IconSubtitleBox.Text_SubtitleBox.Text = FileSystemInfo.Name;
            FileName_IconSubtitleBox.Text_SubtitleBox.PlayingSpeed = 2;
            FileName_IconSubtitleBox.ClientSize = new(80 + Math.Max(0, ClientSize.Width - MinSize.Width), 16);
            FileName_IconSubtitleBox.Skin.SetAllBackgroundColor(string.Empty);

            ChildControls.Add(WriteDate_Label);
            WriteDate_Label.AutoSize = false;
            WriteDate_Label.Anchor = Direction.Right;
            WriteDate_Label.Text = FileSystemInfo.LastWriteTime.ToString("yy/MM/dd");
            WriteDate_Label.ClientSize = new(64, 16);
            WriteDate_Label.LayoutRight(this, FileName_IconSubtitleBox, 1);

            ChildControls.Add(WriteTime_Label);
            WriteTime_Label.AutoSize = false;
            WriteTime_Label.Anchor = Direction.Right;
            WriteTime_Label.Text = FileSystemInfo.LastWriteTime.ToString("HH:mm");
            WriteTime_Label.ClientSize = new(40, 16);
            WriteTime_Label.LayoutRight(this, WriteDate_Label, 1);

            ChildControls.Add(Size_Label);
            Size_Label.AutoSize = false;
            Size_Label.Anchor = Direction.Right;
            Size_Label.Text = FileSystemInfo is FileInfo fileInfo ? _bytesFormatter.Format(fileInfo.Length) : "-";
            Size_Label.ClientSize = new(40, 16);
            Size_Label.LayoutRight(this, WriteTime_Label, 1);
        }

        protected override BlockFrame Drawing()
        {
            BlockFrame blockFrame = base.Drawing();
            blockFrame.DrawVerticalLine(FileName_IconSubtitleBox.RightLocation + 1, BlockManager.Concrete.LightGray);
            blockFrame.DrawVerticalLine(WriteDate_Label.RightLocation + 1, BlockManager.Concrete.LightGray);
            blockFrame.DrawVerticalLine(WriteTime_Label.RightLocation + 1, BlockManager.Concrete.LightGray);
            return blockFrame;
        }
    }
}
