using MCBS.BlockForms;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.Screens;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop.DesktopIcons
{
    public abstract class DesktopIcon : ContainerControl<Control>
    {
        public DesktopIcon()
        {
            Icon_PictureBox = new();
            Name_Label = new();

            BorderWidth = 0;
            ClientSize = new(24, 24);
            Skin.SetBackgroundColor(string.Empty, ControlState.None);
            Skin.SetBackgroundColor(BlockManager.Concrete.LightBlue, ControlState.Hover);
            Skin.SetBackgroundColor(BlockManager.Concrete.Blue, ControlState.Selected, ControlState.Hover | ControlState.Selected);
        }

        private readonly PictureBox<Rgba32> Icon_PictureBox;

        private readonly Label Name_Label;

        public abstract IconIdentifier GetIconIdentifier();

        internal abstract string GetDisplayName();

        internal abstract Image<Rgba32> GetIncnImage();

        internal abstract void OpenIcon();

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(Icon_PictureBox);
            Icon_PictureBox.ClientLocation = new(3, 3);
            Icon_PictureBox.ClientSize = new(16, 16);
            Icon_PictureBox.DefaultResizeOptions.Size = Icon_PictureBox.ClientSize;
            Icon_PictureBox.SetImage(GetIncnImage());

            Name_Label.BorderWidth = 1;
            Name_Label.Text = GetDisplayName();
            Name_Label.Skin.SetAllBackgroundColor(BlockManager.Concrete.White);
        }

        protected override void OnCursorMove(Control sender, CursorEventArgs e)
        {
            base.OnCursorMove(sender, e);

            if (!e.CursorContext.HoverControls.TryGetValue(Name_Label, out var hoverControl))
                return;

            Screen? screen = e.CursorContext.ScreenContextOf?.Screen;
            if (screen is null)
                return;

            Point position = e.CursorContext.NewInputData.CursorPosition;
            Point offset = new(-Name_Label.BorderWidth, -Name_Label.BorderWidth);
            offset.Y -= 4;
            if (position.Y - offset.Y + Name_Label.Height - 1 > screen.Height)
            {
                offset.Y += Name_Label.Height - 1;
                offset.Y += 8;
            }
            if (position.X - offset.X + Name_Label.Width - Name_Label.BorderWidth * 2 > screen.Width)
            {
                offset.X = position.X - offset.X + Name_Label.Width - Name_Label.BorderWidth * 2 - screen.Width;
            }
            hoverControl.OffsetPosition = offset;
        }

        protected override void OnCursorEnter(Control sender, CursorEventArgs e)
        {
            base.OnCursorEnter(sender, e);

            e.CursorContext.HoverControls.TryAdd(Name_Label, out _);
        }

        protected override void OnCursorLeave(Control sender, CursorEventArgs e)
        {
            base.OnCursorLeave(sender, e);

            e.CursorContext.HoverControls.TryRemove(Name_Label, out _);
        }

        protected override void OnRightClick(Control sender, CursorEventArgs e)
        {
            base.OnRightClick(sender, e);

            IsSelected = !IsSelected;
        }
    }
}
