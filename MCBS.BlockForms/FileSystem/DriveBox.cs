using MCBS.BlockForms.Utility;
using MCBS.Drawing;
using QuanLib.Minecraft.Blocks;
using QuanLib.TextFormat;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem
{
    public class DriveBox : ContainerControl<Control>
    {
        public DriveBox(DriveInfo driveInfo)
        {
            ArgumentNullException.ThrowIfNull(driveInfo, nameof(driveInfo));

            DriveInfo = driveInfo;

            ClientSize = new(48 + 112 + 3, 48 + 2);
            Skin.SetBackgroundColor(BlockManager.Concrete.LightBlue, ControlState.Hover);
            Skin.SetBackgroundColor(BlockManager.Concrete.Blue, ControlState.Selected, ControlState.Hover | ControlState.Selected);

            Icon_PictureBox = new();
            DriveName_Label = new();
            Capacity_HorizontalProgressBar = new();
            Capacity_Label = new();
        }

        private static readonly BytesFormatter _bytesFormatter = new(AbbreviationBytesFormatText.Default);

        private readonly PictureBox<Rgba32> Icon_PictureBox;

        private readonly Label DriveName_Label;

        private readonly HorizontalProgressBar Capacity_HorizontalProgressBar;

        private readonly Label Capacity_Label;

        public DriveInfo DriveInfo { get; }

        public override void Initialize()
        {
            base.Initialize();

            string total, used;
            double ratio;
            if (DriveInfo.IsReady)
            {
                total = _bytesFormatter.Format(DriveInfo.TotalSize);
                used = _bytesFormatter.Format((DriveInfo.TotalSize - DriveInfo.TotalFreeSpace));
                ratio = DriveInfo.TotalSize > 0 ? 1 - ((double)DriveInfo.TotalFreeSpace / DriveInfo.TotalSize) : 0;
            }
            else
            {
                total = "-";
                used = "-";
                ratio = 0;
            }

            ChildControls.Add(Icon_PictureBox);
            Icon_PictureBox.BorderWidth = 0;
            Icon_PictureBox.AutoSize = false;
            Icon_PictureBox.ClientSize = new(48, 48);
            Icon_PictureBox.ClientLocation = new(1, 1);
            Icon_PictureBox.Skin.SetAllBackgroundColor(string.Empty);
            Icon_PictureBox.SetImage(TextureManager.Instance["HDD"]);
            Icon_PictureBox.Texture.ResizeOptions.Sampler = KnownResamplers.NearestNeighbor;

            ChildControls.Add(DriveName_Label);
            DriveName_Label.AutoSize = false;
            DriveName_Label.ClientSize = new(112, 16);
            DriveName_Label.LayoutRight(this, Icon_PictureBox, 1);
            DriveName_Label.Text = DriveInfo.IsReady ? $"{DriveInfo.VolumeLabel} ({DriveInfo.Name})" : "磁盘未准备就绪";

            ChildControls.Add(Capacity_HorizontalProgressBar);
            Capacity_HorizontalProgressBar.Size = new(112, 14);
            Capacity_HorizontalProgressBar.LayoutDown(this, DriveName_Label, 1);
            Capacity_HorizontalProgressBar.Progress = ratio;
            Capacity_HorizontalProgressBar.Skin.SetAllForegroundColor(ratio < 0.9 ? BlockManager.Concrete.LightBlue : BlockManager.Concrete.Red);

            ChildControls.Add(Capacity_Label);
            Capacity_Label.AutoSize = false;
            Capacity_Label.ClientSize = new(112, 16);
            Capacity_Label.LayoutDown(this, Capacity_HorizontalProgressBar, 1);
            Capacity_Label.Text = $"{used}/{total} {Math.Round(ratio * 100)}%";
        }
    }
}
