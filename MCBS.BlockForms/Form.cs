using MCBS.Application;
using MCBS.Cursor;
using MCBS.Cursor.Style;
using MCBS.Events;
using MCBS.Forms;
using MCBS.Screens;
using MCBS.UI;
using QuanLib.Core;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class Form : ContainerControl<Control>, IForm
    {
        protected Form()
        {
            AllowSelected = true;
            AllowDeselected = true;
            AllowDrag = true;
            AllowStretch = true;
            ResizeBorder = Direction.None;
            IsMinimize = false;
            Stretch = Direction.Bottom | Direction.Right;

            ReturnValue = null;
            Text = string.Empty;

            _onresize = false;

            FormLoad += OnFormLoad;
            FormClose += OnFormClose;
            FormMinimize += OnFormMinimize;
            FormUnminimize += OnFormUnminimize;
        }

        protected bool _onresize;

        public virtual bool AllowSelected { get; set; }

        public virtual bool AllowDeselected { get; set; }

        public virtual bool AllowDrag { get; set; }

        public virtual bool AllowStretch { get; set; }

        public virtual Direction ResizeBorder { get; protected set; }

        public virtual object? ReturnValue { get; protected set; }

        public virtual bool IsMinimize { get; protected set; }

        public virtual bool IsMaximize
        {
            get
            {
                Size maximizeSize = MaximizeSize;
                return Location == MaximizeLocation && Width == maximizeSize.Width && Height == maximizeSize.Height;
            }
        }

        public virtual Point MaximizeLocation => new(0, 0);

        public virtual Size MaximizeSize => GetFormContainerSize();

        public virtual Point RestoreLocation { get; protected set; }

        public virtual Size RestoreSize { get; protected set; }

        public event EventHandler<Form, EventArgs> FormLoad;

        public event EventHandler<Form, EventArgs> FormClose;

        public event EventHandler<Form, EventArgs> FormMinimize;

        public event EventHandler<Form, EventArgs> FormUnminimize;

        protected virtual void OnFormLoad(Form sender, EventArgs e) { }

        protected virtual void OnFormClose(Form sender, EventArgs e)
        {
            ClearAllLayoutSyncer();
        }

        protected virtual void OnFormMinimize(Form sender, EventArgs e) { }

        protected virtual void OnFormUnminimize(Form sender, EventArgs e) { }

        public void HandleFormLoad(EventArgs e)
        {
            FormLoad.Invoke(this, e);
        }

        public void HandleFormClose(EventArgs e)
        {
            FormClose.Invoke(this, e);
        }

        public void HandleFormMinimize(EventArgs e)
        {
            FormMinimize.Invoke(this, e);
        }

        public void HandleFormUnminimize(EventArgs e)
        {
            FormUnminimize.Invoke(this, e);
        }

        public override void Initialize()
        {
            base.Initialize();

            Size maximizeSize = MaximizeSize;
            Width = maximizeSize.Width;
            Height = maximizeSize.Height;
            InvokeExternalCursorMove = true;

            ApplicationInfo? appInfo = MCOS.Instance.ProcessOf(this)?.ApplicationInfo;
            if (appInfo is not null)
                Text = appInfo.Name;
        }

        protected override void OnInitializeCompleted(Control sender, EventArgs e)
        {
            base.OnInitializeCompleted(sender, e);

            if (IsMaximize)
            {
                RestoreSize = ClientSize * 2 / 3;
                RestoreLocation = new(Width / 2 - RestoreSize.Width / 2, Height / 2 - RestoreSize.Height / 2);
            }
            else
            {
                RestoreSize = ClientSize;
                RestoreLocation = ClientLocation;
            }
        }

        protected override void OnCursorMove(Control sender, CursorEventArgs e)
        {
            base.OnCursorMove(sender, e);

            if (!IsSelected)
                return;

            Point parentPos = this.ChildPos2ParentPos(e.Position);
            if (parentPos.Y < TopLocation - 32 ||
                parentPos.X < LeftLocation - 32 ||
                parentPos.Y > BottomLocation + 32 ||
                parentPos.X > RightLocation + 32)
                return;

            FormContext? formContext = GetFormContext();
            if (formContext is not null)
            {
                if (formContext.FormState == FormState.Stretching &&
                    formContext.StretchingContext is not null &&
                    formContext.StretchingContext.CursorContext == e.CursorContext)
                {
                    Direction borders = formContext.StretchingContext.Borders;
                    e.CursorContext.StyleType = GetCursorStyleType(borders);

                    if (borders.HasFlag(Direction.Top))
                        TopLocation = parentPos.Y;
                    if (borders.HasFlag(Direction.Bottom))
                        BottomLocation = parentPos.Y;
                    if (borders.HasFlag(Direction.Left))
                        LeftLocation = parentPos.X;
                    if (borders.HasFlag(Direction.Right))
                        RightLocation = parentPos.X;
                }
                else if (formContext.FormState == FormState.Active)
                {
                    Direction borders = GetStretchingBorders(parentPos);
                    e.CursorContext.StyleType = GetCursorStyleType(borders);
                }
            }
        }

        protected override void OnMove(Control sender, PositionChangedEventArgs e)
        {
            base.OnMove(sender, e);

            if (!_onresize && !IsMaximize)
            {
                RestoreLocation = ClientLocation;
                RestoreSize = ClientSize;
            }
        }

        protected override void OnResize(Control sender, SizeChangedEventArgs e)
        {
            base.OnResize(sender, e);

            if (!_onresize && !IsMaximize)
            {
                RestoreLocation = ClientLocation;
                RestoreSize = ClientSize;
            }
        }

        protected override void OnControlDeselected(Control sender, EventArgs e)
        {
            base.OnControlDeselected(sender, e);

            CursorContext[] hoverContexts = GetHoverCursors();
            foreach (var hoverContext in hoverContexts)
                HandleCursorMove(new(new(-1024, -1024), hoverContext));
        }

        public virtual Image<Rgba32> GetIcon()
        {
            ApplicationInfo? appInfo = MCOS.Instance.ProcessOf(this)?.ApplicationInfo;
            if (appInfo is not null)
                return appInfo.GetIcon();
            else
                return new(16, 16, GetBlockColor(BlockManager.Concrete.White));
        }

        public Direction GetStretchingBorders(Point position)
        {
            Direction result = Direction.None;
            if (!AllowStretch || !IsSelected || IsMaximize)
                return result;

            if (position.Y >= TopLocation - 2 &&
                position.X >= LeftLocation - 2 &&
                position.Y <= BottomLocation + 2 &&
                position.X <= RightLocation + 2)
            {
                if (position.Y <= TopLocation + 2)
                    result |= Direction.Top;
                if (position.X <= LeftLocation + 2)
                    result |= Direction.Left;
                if (position.Y >= BottomLocation - 2)
                    result |= Direction.Bottom;
                if (position.X >= RightLocation - 2)
                    result |= Direction.Right;
            }

            return result;
        }

        public virtual void MaximizeForm()
        {
            _onresize = true;
            Size = MaximizeSize;
            Location = new(0, 0);
            _onresize = false;
        }

        public virtual void RestoreForm()
        {
            _onresize = true;
            ClientLocation = RestoreLocation;
            ClientSize = RestoreSize;
            _onresize = false;
        }

        public virtual void MinimizeForm()
        {
            GetFormContext()?.MinimizeForm();
        }

        public virtual void UnminimizeForm()
        {
            GetFormContext()?.UnminimizeForm();
        }

        public virtual void CloseForm()
        {
            GetFormContext()?.CloseForm();
        }

        public FormContext? GetFormContext()
        {
            return MCOS.Instance.FormContextOf(this);
        }

        public static string GetCursorStyleType(Direction borders)
        {
            return borders switch
            {
                Direction.Top or Direction.Bottom => CursorStyleType.VerticalResize,
                Direction.Left or Direction.Right => CursorStyleType.HorizontalResize,
                Direction.Left | Direction.Top or Direction.Right | Direction.Bottom => CursorStyleType.LeftObliqueResize,
                Direction.Right | Direction.Top or Direction.Left | Direction.Bottom => CursorStyleType.RightObliqueResize,
                _ => CursorStyleType.Default,
            };
        }
    }
}
