using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLib.Core;
using MCBS.UI;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Core.Events;
using MCBS.Cursor;
using QuanLib.Game;
using MCBS.UI.Extensions;
using MCBS.Drawing;
using System.Diagnostics;

namespace MCBS.BlockForms
{
    /// <summary>
    /// 控件基类
    /// </summary>
    public abstract partial class Control : UnmanagedBase, IControl
    {
        protected Control()
        {
            FirstHandleCursorMove = true;
            FirstHandleRightClick = false;
            FirstHandleLeftClick = false;
            FirstHandleCursorSlotChanged = false;
            FirstHandleCursorItemChanged = false;
            FirstHandleTextEditorUpdate = false;
            InvokeExternalCursorMove = false;
            KeepWhenClear = false;
            RequestDrawTransparencyTexture = true;
            IsInitCompleted = false;
            IsReadOnly = false;
            IsRequestRedraw = false;
            _DisplayPriority = 0;
            _MaxDisplayPriority = 512;
            _Text = string.Empty;
            _Visible = true;
            _ClientLocation = new(0, 0);
            _ClientSize = new(SR.DefaultFont.HalfWidth * 4, SR.DefaultFont.Height);
            _OffsetPosition = new(0, 0);
            MinSize = Size.Empty;
            MaxSize = new(int.MaxValue, int.MaxValue);
            _AutoSize = false;
            _BorderWidth = 1;
            Skin = new(this);
            Anchor = Direction.Top | Direction.Left;
            Stretch = Direction.None;
            _ContentAnchor = AnchorPosition.UpperLeft;
            _ControlState = ControlState.None;
            _LayoutSyncer = null;

            _drawCache = null;
            _hoverCursors = new();

            CursorMove += OnCursorMove;
            CursorEnter += OnCursorEnter;
            CursorLeave += OnCursorLeave;
            RightClick += OnRightClick;
            LeftClick += OnLeftClick;
            DoubleRightClick += OnDoubleRightClick;
            DoubleLeftClick += OnDoubleLeftClick;
            CursorSlotChanged += OnCursorSlotChanged;
            CursorItemChanged += OnCursorItemChanged;
            TextEditorUpdate += OnTextEditorUpdate;
            BeforeFrame += OnBeforeFrame;
            AfterFrame += OnAfterFrame;
            InitializeCompleted += OnInitializeCompleted;
            ControlSelected += OnControlSelected;
            ControlDeselected += OnControlDeselected;
            Move += OnMove;
            Resize += OnResize;
            OffsetPositionChanged += OnOffsetPositionChanged;
            TextChanged += OnTextChanged;
            Layout += OnLayout;
        }

        private BlockFrame? _drawCache;

        private readonly List<CursorContext> _hoverCursors;

        public bool FirstHandleCursorMove { get; set; }

        public bool FirstHandleRightClick { get; set; }

        public bool FirstHandleLeftClick { get; set; }

        public bool FirstHandleCursorSlotChanged { get; set; }

        public bool FirstHandleCursorItemChanged { get; set; }

        public bool FirstHandleTextEditorUpdate { get; set; }

        public bool InvokeExternalCursorMove { get; set; }

        public bool KeepWhenClear { get; set; }

        public bool RequestDrawTransparencyTexture { get; set; }

        public bool IsInitCompleted { get; private set; }

        public bool IsReadOnly { get; set; }

        public bool IsRequestRedraw { get; private set; }

        public IContainerControl? ParentContainer { get; private set; }

        public int Index => GetParentContainer()?.GetChildControls().IndexOf(this) ?? -1;

        public virtual string Text
        {
            get => _Text;
            set
            {
                if (_Text != value)
                {
                    string temp = _Text;
                    _Text = value;
                    HandleTextChanged(new(temp, _Text));
                    RequestRedraw();
                }
            }
        }
        private string _Text;

        #region 位置与尺寸

        public Point ClientLocation
        {
            get => _ClientLocation;
            set
            {
                if (_ClientLocation != value)
                {
                    Point temp = _ClientLocation;
                    _ClientLocation = value;
                    Move.Invoke(this, new(temp, _ClientLocation));
                    RequestRedraw();
                }
            }
        }
        private Point _ClientLocation;

        public Size ClientSize
        {
            get => _ClientSize;
            set
            {
                if (_ClientSize != value)
                {
                    Size temp = _ClientSize;
                    _ClientSize = value;
                    Resize.Invoke(this, new(temp, _ClientSize));
                    RequestRedraw();
                }
            }
        }
        private Size _ClientSize;

        public Point OffsetPosition
        {
            get => _OffsetPosition;
            set
            {
                if (_OffsetPosition != value)
                {
                    Point temp = _OffsetPosition;
                    _OffsetPosition = value;
                    OffsetPositionChanged.Invoke(this, new(temp, _OffsetPosition));
                    RequestRedraw();
                }
            }
        }
        private Point _OffsetPosition;

        public Size MinSize { get; set; }

        public Size MaxSize { get; set; }

        public Point Location
        {
            get => new(ClientLocation.X + ParentBorderWidth, ClientLocation.Y + ParentBorderWidth);
            set
            {
                ClientLocation = new(value.X - ParentBorderWidth, value.Y - ParentBorderWidth);
            }
        }

        public Size Size
        {
            get => new(ClientSize.Width + BorderWidth * 2, ClientSize.Height + BorderWidth * 2);
            set
            {
                ClientSize = new(value.Width - BorderWidth * 2, value.Height - BorderWidth * 2);
            }
        }

        public int X
        {
            get => ClientLocation.X + ParentBorderWidth;
            set
            {
                ClientLocation = new(value - ParentBorderWidth, ClientLocation.Y);
            }
        }

        public int Y
        {
            get => ClientLocation.Y + ParentBorderWidth;
            set
            {
                ClientLocation = new(ClientLocation.X, value - ParentBorderWidth);
            }
        }

        public int Width
        {
            get => ClientSize.Width + BorderWidth * 2;
            set
            {
                ClientSize = new(value - BorderWidth * 2, ClientSize.Height);
            }
        }

        public int Height
        {
            get => ClientSize.Height + BorderWidth * 2;
            set
            {
                ClientSize = new(ClientSize.Width, value - BorderWidth * 2);
            }
        }

        public int BorderWidth
        {
            get => _BorderWidth;
            set
            {
                if (value < 0)
                    value = 0;

                if (_BorderWidth != value)
                {
                    _BorderWidth = value;
                    RequestRedraw();
                }
            }
        }
        private int _BorderWidth;

        public int ParentBorderWidth => GetParentContainer()?.BorderWidth ?? 0;

        public int TopLocation
        {
            get => ClientLocation.Y;
            set
            {
                int offset = TopLocation - value;
                ClientSize = new(ClientSize.Width, ClientSize.Height + offset);
                Location = new(Location.X, Location.Y - offset);
            }
        }

        public int BottomLocation
        {
            get => ClientLocation.Y + Height - 1;
            set
            {
                int offset = value - BottomLocation;
                ClientSize = new(ClientSize.Width, ClientSize.Height + offset);
            }
        }

        public int LeftLocation
        {
            get => ClientLocation.X;
            set
            {
                int offset = LeftLocation - value;
                ClientSize = new(ClientSize.Width + offset, ClientSize.Height);
                Location = new(Location.X - offset, Location.Y);
            }
        }

        public int RightLocation
        {
            get => ClientLocation.X + Width - 1;
            set
            {
                int offset = value - RightLocation;
                ClientSize = new(ClientSize.Width + offset, ClientSize.Height);
            }
        }

        public int TopToBorder
        {
            get => Location.Y - ParentBorderWidth;
            set
            {
                int offset = TopToBorder - value;
                ClientSize = new(ClientSize.Width, ClientSize.Height + offset);
                Location = new(Location.X, Location.Y - offset);
            }
        }

        public int BottomToBorder
        {
            get => GetParentContainer() is not ContainerControl containerControl ? 0 : (containerControl.Height - containerControl.BorderWidth) - (Location.Y + Height);
            set
            {
                int offset = BottomToBorder - value;
                ClientSize = new(ClientSize.Width, ClientSize.Height + offset);
            }
        }

        public int LeftToBorder
        {
            get => Location.X - ParentBorderWidth;
            set
            {
                int offset = LeftToBorder - value;
                ClientSize = new(ClientSize.Width + offset, ClientSize.Height);
                Location = new(Location.X - offset, Location.Y);
            }
        }

        public int RightToBorder
        {
            get => GetParentContainer() is not ContainerControl containerControl ? 0 : (containerControl.Width - containerControl.BorderWidth) - (Location.X + Width);
            set
            {
                int offset = RightToBorder - value;
                ClientSize = new(ClientSize.Width + offset, ClientSize.Height);
            }
        }

        public bool AutoSize
        {
            get => _AutoSize;
            set
            {
                if (_AutoSize != value)
                {
                    if (value)
                        AutoSetSize();
                    _AutoSize = value;
                    RequestRedraw();
                }
            }
        }
        private bool _AutoSize;

        #endregion

        #region 外观与布局

        public bool Visible
        {
            get => _Visible;
            set
            {
                if (_Visible != value)
                {
                    _Visible = value;
                    RequestRedraw();
                }
            }
        }
        private bool _Visible;

        public int DisplayPriority
        {
            get
            {
                if (IsSelected)
                    return MaxDisplayPriority;
                else
                    return _DisplayPriority;
            }
            set
            {
                _DisplayPriority = value;
                if (!IsSelected)
                    ParentContainer?.GetChildControls().Sort();
            }
        }
        private int _DisplayPriority;

        public int MaxDisplayPriority
        {
            get => _MaxDisplayPriority;
            set
            {
                _MaxDisplayPriority = value;
                if (IsSelected)
                    ParentContainer?.GetChildControls().Sort();
            }
        }
        private int _MaxDisplayPriority;

        public ControlSkin Skin { get; }

        /// <summary>
        /// 锚定，大小不变，位置自适应父控件
        /// </summary>
        public Direction Anchor { get; set; }

        /// <summary>
        /// 拉伸，位置不变，大小自适应父控件
        /// </summary>
        public Direction Stretch { get; set; }

        public LayoutMode LayoutMode => LayoutSyncer is null ? LayoutMode.Auto : LayoutMode.Sync;

        public AnchorPosition ContentAnchor
        {
            get => _ContentAnchor;
            set
            {
                if (_ContentAnchor != value)
                {
                    _ContentAnchor = value;
                    RequestRedraw();
                }
            }
        }
        private AnchorPosition _ContentAnchor;

        public ControlState ControlState
        {
            get => _ControlState;
            set
            {
                if (_ControlState != value)
                {
                    if (!BlockPixel.Equals(Skin.GetForegroundColor(_ControlState), Skin.GetForegroundColor(value)) ||
                        !BlockPixel.Equals(Skin.GetBackgroundColor(_ControlState), Skin.GetBackgroundColor(value)) ||
                        !BlockPixel.Equals(Skin.GetBorderColor(_ControlState), Skin.GetBorderColor(value)) ||
                        !Texture.Equals(Skin.GetBackgroundTexture(_ControlState), Skin.GetBackgroundTexture(value)))
                    {
                        RequestRedraw();
                    }
                    _ControlState = value;
                }
            }
        }
        private ControlState _ControlState;

        public bool IsHover
        {
            get => ControlState.HasFlag(ControlState.Hover);
            private set
            {
                if (IsHover != value)
                {
                    if (value)
                    {
                        ControlState |= ControlState.Hover;
                    }
                    else
                    {
                        ControlState ^= ControlState.Hover;
                    }
                }
            }
        }

        public bool IsSelected
        {
            get => ControlState.HasFlag(ControlState.Selected);
            set
            {
                if (IsSelected != value)
                {
                    if (value)
                    {
                        ControlState |= ControlState.Selected;
                        ControlSelected.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        ControlState ^= ControlState.Selected;
                        ControlDeselected.Invoke(this, EventArgs.Empty);
                    }
                    ParentContainer?.GetChildControls().Sort();
                }
            }
        }

        public LayoutSyncer? LayoutSyncer
        {
            get => _LayoutSyncer;
            set
            {
                _LayoutSyncer?.Unbinding();
                _LayoutSyncer = value;
                _LayoutSyncer?.Binding();
                _LayoutSyncer?.Sync();
            }
        }
        private LayoutSyncer? _LayoutSyncer;

        #endregion

        #region 事件发布

        public event EventHandler<Control, CursorEventArgs> CursorMove;

        public event EventHandler<Control, CursorEventArgs> CursorEnter;

        public event EventHandler<Control, CursorEventArgs> CursorLeave;

        public event EventHandler<Control, CursorEventArgs> LeftClick;

        public event EventHandler<Control, CursorEventArgs> RightClick;

        public event EventHandler<Control, CursorEventArgs> DoubleLeftClick;

        public event EventHandler<Control, CursorEventArgs> DoubleRightClick;

        public event EventHandler<Control, CursorEventArgs> TextEditorUpdate;

        public event EventHandler<Control, CursorEventArgs> CursorSlotChanged;

        public event EventHandler<Control, CursorEventArgs> CursorItemChanged;

        public event EventHandler<Control, EventArgs> BeforeFrame;

        public event EventHandler<Control, EventArgs> AfterFrame;

        public event EventHandler<Control, EventArgs> InitializeCompleted;

        public event EventHandler<Control, EventArgs> ControlSelected;

        public event EventHandler<Control, EventArgs> ControlDeselected;

        public event EventHandler<Control, ValueChangedEventArgs<Point>> Move;

        public event EventHandler<Control, ValueChangedEventArgs<Size>> Resize;

        public event EventHandler<Control, ValueChangedEventArgs<Point>> OffsetPositionChanged;

        public event EventHandler<Control, ValueChangedEventArgs<string>> TextChanged;

        public event EventHandler<Control, ValueChangedEventArgs<Size>> Layout;

        #endregion

        #region 事件订阅

        protected virtual void OnCursorMove(Control sender, CursorEventArgs e) { }

        protected virtual void OnCursorEnter(Control sender, CursorEventArgs e) { }

        protected virtual void OnCursorLeave(Control sender, CursorEventArgs e) { }

        protected virtual void OnLeftClick(Control sender, CursorEventArgs e) { }

        protected virtual void OnRightClick(Control sender, CursorEventArgs e) { }

        protected virtual void OnDoubleLeftClick(Control sender, CursorEventArgs e) { }

        protected virtual void OnDoubleRightClick(Control sender, CursorEventArgs e) { }

        protected virtual void OnTextEditorUpdate(Control sender, CursorEventArgs e) { }

        protected virtual void OnCursorSlotChanged(Control sender, CursorEventArgs e) { }

        protected virtual void OnCursorItemChanged(Control sender, CursorEventArgs e) { }

        protected virtual void OnBeforeFrame(Control sender, EventArgs e) { }

        protected virtual void OnAfterFrame(Control sender, EventArgs e) { }

        protected virtual void OnInitializeCompleted(Control sender, EventArgs e) { }

        protected virtual void OnControlSelected(Control sender, EventArgs e) { }

        protected virtual void OnControlDeselected(Control sender, EventArgs e) { }

        protected virtual void OnMove(Control sender, ValueChangedEventArgs<Point> e) { }

        protected virtual void OnResize(Control sender, ValueChangedEventArgs<Size> e) { }

        protected virtual void OnOffsetPositionChanged(Control sender, ValueChangedEventArgs<Point> e)
        {
            IScreenView? screenView = this.GetScreenView();
            if (screenView is not null)
            {
                foreach (var cursorContext in _hoverCursors.ToArray())
                    screenView.HandleCursorMove(new(cursorContext.NewInputData.CursorPosition, cursorContext));
            }
        }

        protected virtual void OnTextChanged(Control sender, ValueChangedEventArgs<string> e) { }

        protected virtual void OnLayout(Control sender, ValueChangedEventArgs<Size> e)
        {
            Size offset = e.NewValue - e.OldValue;
            if (offset.Height != 0)
            {
                if (Anchor.HasFlag(Direction.Top) && Anchor.HasFlag(Direction.Bottom))
                {
                    double proportion = (ClientLocation.Y + Height / 2.0) / e.OldValue.Height;
                    ClientLocation = new(ClientLocation.X, (int)Math.Round(e.NewValue.Height * proportion - Height / 2.0));
                }
                else
                {
                    offset.Height = Math.Clamp(offset.Height, MinSize.Height - ClientSize.Height, MaxSize.Height - ClientSize.Height);
                    if (Anchor.HasFlag(Direction.Bottom))
                        ClientLocation = new(ClientLocation.X, ClientLocation.Y + offset.Height);
                    if (Stretch.HasFlag(Direction.Top) || Stretch.HasFlag(Direction.Bottom))
                        BottomToBorder -= offset.Height;
                }
            }

            if (offset.Width != 0)
            {
                if (Anchor.HasFlag(Direction.Left) && Anchor.HasFlag(Direction.Right))
                {
                    double proportion = (ClientLocation.X + Width / 2.0) / e.OldValue.Width;
                    ClientLocation = new((int)Math.Round(e.NewValue.Width * proportion - Width / 2.0), ClientLocation.Y);
                }
                else
                {
                    offset.Width = Math.Clamp(offset.Width, MinSize.Width - ClientSize.Width, MaxSize.Width - ClientSize.Width);
                    if (Anchor.HasFlag(Direction.Right))
                        ClientLocation = new(ClientLocation.X + offset.Width, ClientLocation.Y);
                    if (Stretch.HasFlag(Direction.Left) || Stretch.HasFlag(Direction.Right))
                        RightToBorder -= offset.Width;
                }
            }
        }

        #endregion

        #region 事件处理

        public virtual bool HandleCursorMove(CursorEventArgs e)
        {
            UpdateHoverState(e);
            return TryInvokeCursorMove(e);
        }

        public virtual bool HandleLeftClick(CursorEventArgs e)
        {
            return TryInvokeLeftClick(e);
        }

        public virtual bool HandleRightClick(CursorEventArgs e)
        {
            return TryInvokeRightClick(e);
        }

        public virtual bool HandleTextEditorUpdate(CursorEventArgs e)
        {
            return TryInvokeTextEditorUpdate(e);
        }

        public virtual bool HandleCursorSlotChanged(CursorEventArgs e)
        {
            return TryInvokeCursorSlotChanged(e);
        }

        public virtual bool HandleCursorItemChanged(CursorEventArgs e)
        {
            return TryInvokeCursorItemChanged(e);
        }

        public virtual void HandleBeforeFrame(EventArgs e)
        {
            BeforeFrame.Invoke(this, e);
        }

        public virtual void HandleAfterFrame(EventArgs e)
        {
            AfterFrame.Invoke(this, e);
        }

        protected virtual void HandleTextChanged(ValueChangedEventArgs<string> e)
        {
            TextChanged.Invoke(this, e);
        }

        public virtual void HandleLayout(ValueChangedEventArgs<Size> e)
        {
            Layout.Invoke(this, e);
        }

        #region TryInvoke

        protected bool TryInvokeCursorMove(CursorEventArgs e)
        {
            if (this.IncludedOnControl(e.Position))
            {
                CursorMove.Invoke(this, e);
                return true;
            }
            else if (InvokeExternalCursorMove)
            {
                CursorMove.Invoke(this, e);
                return false;
            }
            else
            {
                return false;
            }
        }

        protected bool TryInvokeLeftClick(CursorEventArgs e)
        {
            if (Visible && this.IncludedOnControl(e.Position))
            {
                LeftClick.Invoke(this, e);
                if (e.NewData.LeftClickPosition == e.OldData.LeftClickPosition && (e.NewData.LeftClickTime - e.OldData.LeftClickTime).TotalMilliseconds <= 500)
                    DoubleLeftClick.Invoke(this, e);
                return true;
            }
            return false;
        }

        protected bool TryInvokeRightClick(CursorEventArgs e)
        {
            if (Visible && this.IncludedOnControl(e.Position))
            {
                RightClick.Invoke(this, e);
                if (e.NewData.RightClickPosition == e.OldData.RightClickPosition && (e.NewData.RightClickTime - e.OldData.RightClickTime).TotalMilliseconds <= 500)
                    DoubleRightClick.Invoke(this, e);
                return true;
            }
            return false;
        }

        protected bool TryInvokeTextEditorUpdate(CursorEventArgs e)
        {
            if (Visible && this.IncludedOnControl(e.Position))
            {
                TextEditorUpdate.Invoke(this, e);
                return true;
            }
            return false;
        }

        protected bool TryInvokeCursorSlotChanged(CursorEventArgs e)
        {
            if (Visible && this.IncludedOnControl(e.Position))
            {
                CursorSlotChanged.Invoke(this, e);
                return true;
            }
            return false;
        }

        protected bool TryInvokeCursorItemChanged(CursorEventArgs e)
        {
            if (Visible && this.IncludedOnControl(e.Position))
            {
                CursorItemChanged.Invoke(this, e);
                return true;
            }
            return false;
        }

        #endregion

        #endregion

        #region 初始化

        public virtual void Initialize()
        {
            if (AutoSize)
                AutoSetSize();
        }

        public virtual void BeforeInitialize()
        {

        }

        public virtual void AfterInitialize()
        {
            IsInitCompleted = true;
        }

        public virtual void HandleInitialize()
        {
            Initialize();
            InitializeCompleted.Invoke(this, EventArgs.Empty);
        }

        public virtual void HandleBeforeInitialize()
        {
            BeforeInitialize();
        }

        public virtual void HandleAfterInitialize()
        {
            AfterInitialize();
        }

        #endregion

        #region 位置移动

        public void ToTopMove(int offset)
        {
            Location = new(Location.X, Location.Y - offset);
        }

        public void ToBottomMove(int offset)
        {
            Location = new(Location.X, Location.Y + offset);
        }

        public void ToLeftMove(int offset)
        {
            Location = new(Location.X - offset, Location.Y);
        }

        public void ToRightMove(int offset)
        {
            Location = new(Location.X + offset, Location.Y);
        }

        public void MoveToTop(int distance)
        {
            int offset = TopToBorder - distance;
            ToTopMove(offset);
        }

        public void MoveToBottom(int distance)
        {
            int offset = BottomToBorder - distance;
            ToBottomMove(offset);
        }

        public void MoveToLeft(int distance)
        {
            int offset = LeftToBorder - distance;
            ToLeftMove(offset);
        }

        public void MoveToRight(int distance)
        {
            int offset = RightToBorder - distance;
            ToRightMove(offset);
        }

        #endregion

        #region 帧绘制处理

        public void RequestRedraw()
        {
            IsRequestRedraw = true;
            ParentContainer?.RequestRedraw();
        }

        public virtual DrawResult GetDrawResult()
        {
            if (IsRequestRedraw || _drawCache is null)
            {
                if (_drawCache is IDisposable disposable)
                    disposable.Dispose();

                Stopwatch stopwatch = Stopwatch.StartNew();
                _drawCache = Drawing();
                stopwatch.Stop();

                IsRequestRedraw = false;
                return new(this, _drawCache, true, stopwatch.Elapsed);
            }

            return new(this, _drawCache, false, TimeSpan.Zero);
        }

        protected virtual BlockFrame Drawing()
        {
            return this.DrawBackground(ClientSize);
        }

        public BlockPixel GetForegroundColor()
        {
            return Skin.GetForegroundColor(ControlState);
        }

        public BlockPixel GetBackgroundColor()
        {
            return Skin.GetBackgroundColor(ControlState);
        }

        public BlockPixel GetBorderColor()
        {
            return Skin.GetBorderColor(ControlState);
        }

        public Texture? GetBackgroundTexture()
        {
            return Skin.GetBackgroundTexture(ControlState);
        }

        #endregion

        public virtual void UpdateHoverState(CursorEventArgs e)
        {
            bool included = this.IncludedOnControl(e.Position);
            if (_hoverCursors.Contains(e.CursorContext))
            {
                if (!included)
                {
                    _hoverCursors.Remove(e.CursorContext);
                    if (_hoverCursors.Count == 0)
                        IsHover = false;
                    CursorLeave.Invoke(this, e);
                }
            }
            else
            {
                if (included)
                {
                    _hoverCursors.Add(e.CursorContext);
                    IsHover = true;
                    CursorEnter.Invoke(this, e);
                }
            }
        }

        public CursorContext[] GetHoverCursors()
        {
            return _hoverCursors.ToArray();
        }

        public bool UpdateParentContainer(IContainerControl? parentContainer)
        {
            if (parentContainer is null)
            {
                if (ParentContainer is null || !ParentContainer.GetChildControls().Contains(this))
                {
                    ParentContainer = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (parentContainer.GetChildControls().Contains(this))
            {
                ParentContainer = parentContainer;
                return true;
            }
            else
            {
                return false;
            }
        }

        public ContainerControl? GetParentContainer()
        {
            return ParentContainer as ContainerControl;
        }

        public virtual void AutoSetSize()
        {

        }

        protected override void DisposeUnmanaged()
        {
            LayoutSyncer = null;
        }

        public override string ToString()
        {
            return $"Type:{GetType().Name}|Text:{Text}|Pos:{ClientLocation.X},{ClientLocation.Y}|Size:{ClientSize.Width},{ClientSize.Height}";
        }

        public int CompareTo(IControl? other)
        {
            return DisplayPriority.CompareTo(other?.DisplayPriority);
        }
    }
}
