﻿using MCBS.BlockForms.Utility;
using MCBS.Drawing;
using MCBS.Events;
using QuanLib.Core;
using QuanLib.Core.Events;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class ScrollablePanel : Panel<Control>
    {
        public ScrollablePanel()
        {
            FirstHandleCursorSlotChanged = true;

            ScrollDelta = 16;
            _PageSize = new(0, 0);
            ScrollBarShowTime = 20;
            ScrollBarHideTime = 0;
            EnableVerticalSliding = false;
            EnableHorizontalSliding = false;
            SlidingDelta = 2;

            VerticalScrollBar = new();
            HorizontalScrollBar = new();

            PageSizeChanged += OnPageSizeChanged;
        }

        private readonly VerticalScrollBar VerticalScrollBar;

        private readonly HorizontalScrollBar HorizontalScrollBar;

        public bool ShowVerticalScrollBar => ClientSize.Height < PageSize.Height;

        public bool ShowHorizontalScrollBar => ClientSize.Width < PageSize.Width;

        public int ScrollBarShowTime { get; set; }

        public int ScrollBarHideTime { get; private set; }

        public int ScrollDelta { get; set; }

        public bool EnableVerticalSliding { get; set; }

        public bool EnableHorizontalSliding { get; set; }

        public int SlidingDelta { get; set; }

        public Size PageSize
        {
            get => _PageSize;
            set
            {
                if (_PageSize != value)
                {
                    Size temp = _PageSize;
                    _PageSize = value;
                    PageSizeChanged.Invoke(this, new(temp, _PageSize));
                    RequestRedraw();
                }
            }
        }
        private Size _PageSize;

        public event EventHandler<ScrollablePanel, ValueChangedEventArgs<Size>> PageSizeChanged;

        protected virtual void OnPageSizeChanged(ScrollablePanel sender, ValueChangedEventArgs<Size> e)
        {
            if (e.OldValue.Height != e.NewValue.Height)
                RefreshVerticalScrollBar();
            if (e.OldValue.Width != e.NewValue.Width)
                RefreshHorizontalScrollBar();
        }

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(VerticalScrollBar);
            VerticalScrollBar.Visible = false;
            VerticalScrollBar.KeepWhenClear = true;
            VerticalScrollBar.Width = 8;
            VerticalScrollBar.LayoutSyncer = new(this, (sender, e) => { }, (sender, e) => { });
            VerticalScrollBar.RightClick += VerticalScrollBar_RightClick;

            ChildControls.Add(HorizontalScrollBar);
            HorizontalScrollBar.Visible = false;
            HorizontalScrollBar.KeepWhenClear = true;
            HorizontalScrollBar.Height = 8;
            HorizontalScrollBar.LayoutSyncer = new(this, (sender, e) => { }, (sender, e) => { });
            HorizontalScrollBar.RightClick += HorizontalScrollBar_RightClick;
        }

        protected override void OnInitializeCompleted(Control sender, EventArgs e)
        {
            base.OnInitializeCompleted(sender, e);

            RefreshVerticalScrollBar();
            RefreshHorizontalScrollBar();
        }

        protected override BlockFrame Drawing()
        {
            return this.DrawBackground(GetDrawingSize());
        }

        protected override void OnResize(Control sender, ValueChangedEventArgs<Size> e)
        {
            Size oldSize = e.OldValue;
            Size newSize = e.NewValue;

            int maxX = Math.Max(ClientSize.Width, PageSize.Width) - ClientSize.Width;
            int maxY = Math.Max(ClientSize.Height, PageSize.Height) - ClientSize.Height;
            OffsetPosition = new(Math.Clamp(OffsetPosition.X, 0, maxX), Math.Clamp(OffsetPosition.Y, 0, maxY));

            if (oldSize.Height < PageSize.Height)
                oldSize.Height = PageSize.Height;
            if (oldSize.Width < PageSize.Width)
                oldSize.Width = PageSize.Width;

            if (newSize.Height < PageSize.Height)
            {
                newSize.Height = PageSize.Height;
                RefreshVerticalScrollBar();
            }
            else
            {
                HideScrollBar();
            }

            if (newSize.Width < PageSize.Width)
            {
                newSize.Width = PageSize.Width;
                RefreshHorizontalScrollBar();
            }
            else
            {
                HideScrollBar();
            }

            base.OnResize(sender, new(oldSize, newSize));
        }

        protected override void OnCursorSlotChanged(Control sender, CursorEventArgs e)
        {
            base.OnCursorSlotChanged(sender, e);

            Point offset;
            int delte = e.InventorySlotDelta * ScrollDelta;
            if (ShowVerticalScrollBar)
            {
                offset = new(OffsetPosition.X, OffsetPosition.Y + delte);
                int max = Math.Max(ClientSize.Height, PageSize.Height) - ClientSize.Height;
                offset.Y = Math.Clamp(offset.Y, 0, max);
            }
            else if (ShowHorizontalScrollBar)
            {
                offset = new(OffsetPosition.X + delte, OffsetPosition.Y);
                int max = Math.Max(ClientSize.Width, PageSize.Width) - ClientSize.Width;
                offset.X = Math.Clamp(offset.X, 0, max);
            }
            else
            {
                return;
            }

            OffsetPosition = offset;
        }

        protected override void OnOffsetPositionChanged(Control sender, ValueChangedEventArgs<Point> e)
        {
            base.OnOffsetPositionChanged(sender, e);

            if (e.OldValue.Y != e.NewValue.Y)
                RefreshVerticalScrollBar();
            if (e.OldValue.X != e.NewValue.X)
                RefreshHorizontalScrollBar();
        }

        private void VerticalScrollBar_RightClick(Control sender, CursorEventArgs e)
        {
            if (ShowVerticalScrollBar)
            {
                double position = (double)e.Position.Y / VerticalScrollBar.ClientSize.Height - VerticalScrollBar.SliderSize / 2;
                int max = Math.Max(ClientSize.Height, PageSize.Height) - ClientSize.Height;
                Point offset = new(OffsetPosition.X, (int)Math.Round(PageSize.Height * position));
                offset.Y = Math.Clamp(offset.Y, 0, max);
                OffsetPosition = offset;
                RefreshVerticalScrollBar();
            }
        }

        private void HorizontalScrollBar_RightClick(Control sender, CursorEventArgs e)
        {
            if (ShowHorizontalScrollBar)
            {
                double position = (double)e.Position.X / HorizontalScrollBar.ClientSize.Width - HorizontalScrollBar.SliderSize / 2;
                int max = Math.Max(ClientSize.Width, PageSize.Width) - ClientSize.Width;
                Point offset = new((int)Math.Round(PageSize.Width * position), OffsetPosition.Y);
                offset.X = Math.Clamp(offset.X, 0, max);
                OffsetPosition = offset;
                RefreshHorizontalScrollBar();
            }
        }

        protected override void OnCursorMove(Control sender, CursorEventArgs e)
        {
            base.OnCursorMove(sender, e);

            ShowScrollBar();

            Point cursorOffset = new Point(e.NewData.CursorPosition.X - e.OldData.CursorPosition.X, e.NewData.CursorPosition.Y - e.OldData.CursorPosition.Y) * SlidingDelta;

            if (EnableVerticalSliding && cursorOffset.Y != 0)
            {
                int up = ClientSize.Height / 3;
                int down = ClientSize.Height - up;
                int y = e.Position.Y - OffsetPosition.Y;
                if (y <= up)
                {
                    if (cursorOffset.Y < 0)
                        OffsetPosition = new(OffsetPosition.X, Math.Max(cursorOffset.Y + cursorOffset.Y, 0));
                }
                else if (y >= down)
                {
                    if (cursorOffset.Y > 0)
                        OffsetPosition = new(OffsetPosition.X, Math.Min(cursorOffset.Y + cursorOffset.Y, PageSize.Height - ClientSize.Height));
                }
            }

            if (EnableHorizontalSliding && cursorOffset.X != 0)
            {
                int left = ClientSize.Width / 3;
                int right = ClientSize.Width - left;
                int x = e.Position.X - OffsetPosition.X;
                if (x <= left)
                {
                    if (cursorOffset.X < 0)
                        OffsetPosition = new(Math.Max(OffsetPosition.X + cursorOffset.X, 0), OffsetPosition.Y);
                }
                else if (x >= right)
                {
                    if (cursorOffset.X > 0)
                        OffsetPosition = new(Math.Min(OffsetPosition.X + cursorOffset.X, PageSize.Width - ClientSize.Width), OffsetPosition.Y);
                }
            }
        }

        protected override void OnBeforeFrame(Control sender, EventArgs e)
        {
            base.OnBeforeFrame(sender, e);

            if (VerticalScrollBar.Visible == true || HorizontalScrollBar.Visible == true)
            {
                if (ScrollBarHideTime <= 0 && !VerticalScrollBar.IsHover && !HorizontalScrollBar.IsHover)
                {
                    HideScrollBar();
                }
                ScrollBarHideTime--;
            }
        }

        public void ShowScrollBar()
        {
            if (ShowVerticalScrollBar)
                VerticalScrollBar.Visible = true;
            if (ShowHorizontalScrollBar)
                HorizontalScrollBar.Visible = true;

            ScrollBarHideTime = ScrollBarShowTime;
        }

        public void HideScrollBar()
        {
            VerticalScrollBar.Visible = false;
            HorizontalScrollBar.Visible = false;
        }

        public void RefreshVerticalScrollBar()
        {
            if (ShowVerticalScrollBar)
            {
                VerticalScrollBar.SliderSize = (double)ClientSize.Height / PageSize.Height;
                VerticalScrollBar.SliderPosition = (double)OffsetPosition.Y / PageSize.Height;
                VerticalScrollBar.Height = ClientSize.Height;
                if (ShowHorizontalScrollBar)
                    VerticalScrollBar.Height -= HorizontalScrollBar.Height - 2;

                VerticalScrollBar.ClientLocation = new Point(ClientSize.Width - VerticalScrollBar.Width + OffsetPosition.X + 1, OffsetPosition.Y);
                HorizontalScrollBar.ClientLocation = new Point(OffsetPosition.X, ClientSize.Height - HorizontalScrollBar.Height + OffsetPosition.Y + 1);

                ShowScrollBar();
            }
        }

        public void RefreshHorizontalScrollBar()
        {
            if (ShowHorizontalScrollBar)
            {
                HorizontalScrollBar.SliderSize = (double)ClientSize.Width / PageSize.Width;
                HorizontalScrollBar.SliderPosition = (double)OffsetPosition.X / PageSize.Width;
                HorizontalScrollBar.Width = ClientSize.Width;
                if (ShowVerticalScrollBar)
                    HorizontalScrollBar.Width -= VerticalScrollBar.Width - 2;

                VerticalScrollBar.ClientLocation = new Point(ClientSize.Width - VerticalScrollBar.Width + OffsetPosition.X + 1, OffsetPosition.Y);
                HorizontalScrollBar.ClientLocation = new Point(OffsetPosition.X, ClientSize.Height - HorizontalScrollBar.Height + OffsetPosition.Y + 1);

                ShowScrollBar();
            }
        }

        public Size GetDrawingSize()
        {
            return new(Math.Max(ClientSize.Width, PageSize.Width), Math.Max(ClientSize.Height, PageSize.Height));
        }
    }
}
