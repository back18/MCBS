using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Minecraft.Blocks;
using QuanLib.Core.Events;
using SixLabors.ImageSharp.PixelFormats;
using QuanLib.Game;

namespace MCBS.BlockForms
{
    public abstract partial class WindowForm : Form
    {
        protected WindowForm()
        {
            TitleBar_Control = new(this);
            WindowPanel_Control = new(this);
            Home_PagePanel = new("Home");
            Menu_PagePanel = new("Menu");
            WindowPanel_Control.PagePanels.Add(Home_PagePanel.PageKey, Home_PagePanel);
            WindowPanel_Control.PagePanels.Add(Menu_PagePanel.PageKey, Menu_PagePanel);
            WindowPanel_Control.ActivePageKey = Home_PagePanel.PageKey;
            ShowTitleBar_Button = new();
        }

        public readonly TitleBar TitleBar_Control;

        public readonly WindowPanel WindowPanel_Control;

        public readonly PagePanel Home_PagePanel;

        public readonly PagePanel Menu_PagePanel;

        public readonly Button ShowTitleBar_Button;

        public bool ShowTitleBar
        {
            get => ChildControls.Contains(TitleBar_Control);
            set
            {
                if (value)
                {
                    if (!ShowTitleBar)
                    {
                        ChildControls.TryAdd(TitleBar_Control);
                        ChildControls.Remove(ShowTitleBar_Button);
                        WindowPanel_Control?.LayoutSyncer?.Sync();
                    }
                }
                else
                {
                    if (ShowTitleBar)
                    {
                        ChildControls.Remove(TitleBar_Control);
                        ChildControls.TryAdd(ShowTitleBar_Button);
                        WindowPanel_Control?.LayoutSyncer?.Sync();
                    }
                }
            }
        }

        public override string Text
        {
            get => TitleBar_Control?.Text ?? string.Empty;
            set
            {
                if (TitleBar_Control is null)
                    return;

                if (TitleBar_Control.Text != value)
                {
                    string temp = TitleBar_Control.Text;
                    TitleBar_Control.Text = value;
                    HandleTextChanged(new(temp, TitleBar_Control.Text));
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(TitleBar_Control);

            ChildControls.Add(WindowPanel_Control);
            WindowPanel_Control.BorderWidth = 0;
            WindowPanel_Control.LayoutSyncer = new(this,
            (sender, e) => { },
            (sender, e) =>
            {
                if (ShowTitleBar)
                {
                    WindowPanel_Control.ClientSize = new(ClientSize.Width, ClientSize.Height - TitleBar_Control.Height);
                    WindowPanel_Control.ClientLocation = new(0, TitleBar_Control.Height);
                }
                else
                {
                    WindowPanel_Control.ClientLocation = new(0, 0);
                    WindowPanel_Control.ClientSize = ClientSize;
                }
            });

            ShowTitleBar_Button.Visible = false;
            ShowTitleBar_Button.InvokeExternalCursorMove = true;
            ShowTitleBar_Button.ClientSize = new(16, 16);
            ShowTitleBar_Button.LayoutSyncer = new(this, (sender, e) => { }, (sender, e) =>
            ShowTitleBar_Button.LayoutLeft(this, 0, 0));
            ShowTitleBar_Button.Anchor = Direction.Top | Direction.Right;
            ShowTitleBar_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Shrink"]);
            ShowTitleBar_Button.CursorEnter += ShowTitleBar_Button_CursorEnter;
            ShowTitleBar_Button.CursorLeave += ShowTitleBar_Button_CursorLeave;
            ShowTitleBar_Button.RightClick += ShowTitleBar_Button_RightClick;
        }

        protected override void OnInitializeCompleted(Control sender, EventArgs e)
        {
            if (Home_PagePanel.PageSize != Size.Empty)
            {
                RestoreSize = new(Home_PagePanel.PageSize.Width, Home_PagePanel.PageSize.Height + TitleBar_Control.Height);
                RestoreLocation = new(Width / 2 - RestoreSize.Width / 2, Height / 2 - RestoreSize.Height / 2);
            }
            else
            {
                base.OnInitializeCompleted(sender, e);
            }
        }

        protected override void OnMove(Control sender, PositionChangedEventArgs e)
        {
            base.OnMove(sender, e);

            TitleBar_Control.UpdateMaximizeOrRestore();
        }

        protected override void OnResize(Control sender, SizeChangedEventArgs e)
        {
            base.OnResize(sender, e);

            TitleBar_Control.UpdateMaximizeOrRestore();
        }

        protected override void OnControlSelected(Control sender, EventArgs e)
        {
            base.OnControlSelected(sender, e);

            TitleBar_Control.SetTitleColor(BlockManager.Concrete.Black);
        }

        protected override void OnControlDeselected(Control sender, EventArgs e)
        {
            base.OnControlDeselected(sender, e);

            TitleBar_Control.SetTitleColor(BlockManager.Concrete.LightGray);
        }

        private void ShowTitleBar_Button_CursorEnter(Control sender, CursorEventArgs e)
        {
            ShowTitleBar_Button.Visible = true;
        }

        private void ShowTitleBar_Button_CursorLeave(Control sender, CursorEventArgs e)
        {
            ShowTitleBar_Button.Visible = false;
        }

        private void ShowTitleBar_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ShowTitleBar = true;
        }
    }
}
