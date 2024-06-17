﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms;
using MCBS.BlockForms.Utility;
using QuanLib.Core.Events;
using MCBS.Events;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp.PixelFormats;
using QuanLib.Game;

namespace MCBS.SystemApplications.VideoPlayer
{
    public class VideoPlayerForm : WindowForm
    {
        public VideoPlayerForm(string? path = null)
        {
            _open = path;

            VideoPlayer = new();
            Setting_Switch = new();
            Path_TextBox = new();

            OverlayShowTime = 20;
            OverlayHideTime = 0;
        }

        private readonly string? _open;

        private readonly VideoPlayerBox<Bgr24> VideoPlayer;

        private readonly Switch Setting_Switch;

        private readonly TextBox Path_TextBox;

        public int OverlayShowTime { get; set; }

        public int OverlayHideTime { get; set; }

        public override void Initialize()
        {
            base.Initialize();

            Home_PagePanel.ChildControls.Add(VideoPlayer);
            VideoPlayer.BorderWidth = 0;
            VideoPlayer.Size = Home_PagePanel.ClientSize;
            VideoPlayer.Stretch = Direction.Bottom | Direction.Right;

            Home_PagePanel.ChildControls.Add(Setting_Switch);
            Setting_Switch.OffText = "设置";
            Setting_Switch.OnText = "应用";
            Setting_Switch.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            Setting_Switch.Skin.SetBackgroundColor(BlockManager.Concrete.Yellow, ControlState.Hover);
            Setting_Switch.Skin.SetBackgroundColor(BlockManager.Concrete.Orange, ControlState.Selected, ControlState.Hover | ControlState.Selected);
            Setting_Switch.Skin.SetAllForegroundColor(BlockManager.Concrete.Pink);
            Setting_Switch.ClientLocation = new(2, 2);

            Home_PagePanel.ChildControls.Add(Path_TextBox);
            Path_TextBox.LayoutRight(Home_PagePanel, Setting_Switch, 2);
            Path_TextBox.Width = Home_PagePanel.ClientSize.Width - Setting_Switch.Width - 6;
            Path_TextBox.Stretch = Direction.Right;
            Path_TextBox.Skin.SetAllForegroundColor(BlockManager.Concrete.Pink);
            Path_TextBox.Skin.SetBackgroundColor(string.Empty, ControlState.None);
            Path_TextBox.TextChanged += Path_TextBox_TextChanged;
        }

        public override void AfterInitialize()
        {
            base.AfterInitialize();

            if (_open is not null)
                Path_TextBox.Text = _open;
        }

        protected override void OnCursorMove(Control sender, CursorEventArgs e)
        {
            base.OnCursorMove(sender, e);

            ShowOverlay();
            OverlayHideTime = OverlayShowTime;
        }

        protected override void OnRightClick(Control sender, CursorEventArgs e)
        {
            if (Home_PagePanel.ChildControls.FirstHover is null or VideoPlayerBox<Bgr24>)
            {
                if (Setting_Switch.Visible)
                {
                    HideOverlay();
                    OverlayHideTime = 0;
                }
                else
                {
                    ShowOverlay();
                    OverlayHideTime = OverlayShowTime;
                }
            }
        }

        protected override void OnBeforeFrame(Control sender, EventArgs e)
        {
            base.OnBeforeFrame(sender, e);

            if (Home_PagePanel.ChildControls.FirstHover is null or VideoPlayerBox<Bgr24>)
            {
                if (OverlayHideTime <= 0)
                    HideOverlay();
                OverlayHideTime--;
            }
        }

        protected override void OnFormClose(Form sender, EventArgs e)
        {
            base.OnFormClose(sender, e);

            VideoPlayer.VideoBox.Dispose();
        }

        private void ShowOverlay()
        {
            Setting_Switch.Visible = true;
            Path_TextBox.Visible = true;
        }

        private void HideOverlay()
        {
            Setting_Switch.Visible = false;
            Path_TextBox.Visible = false;
        }

        private void Path_TextBox_TextChanged(Control sender, ValueChangedEventArgs<string> e)
        {
            if (SR.DefaultFont.GetTotalSize(e.NewValue).Width > Path_TextBox.ClientSize.Width)
                Path_TextBox.ContentAnchor = AnchorPosition.UpperRight;
            else
                Path_TextBox.ContentAnchor = AnchorPosition.UpperLeft;

            if (VideoPlayer.VideoBox.TryReadMediaFile(e.NewValue))
            {
                VideoPlayer.VideoBox.MediaFilePlayer?.Play();
            }
            else
            {
                _ = DialogBoxHelper.OpenMessageBoxAsync(this, "警告", $"无法打开视频文件：“{e.NewValue}”", MessageBoxButtons.OK);
            }
        }
    }
}
