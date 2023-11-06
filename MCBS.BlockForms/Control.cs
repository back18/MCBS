using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.PixelFormats;
using QuanLib.Core;
using QuanLib.Minecraft.ResourcePack.Block;
using QuanLib.Minecraft;
using MCBS.UI;
using MCBS.Screens;
using MCBS.BlockForms.Utility;
using MCBS.Processes;
using MCBS.Events;
using QuanLib.Core.Events;
using QuanLib.Minecraft.Blocks;
using MCBS.Cursor;
using System.Diagnostics.CodeAnalysis;
using MCBS.Rendering;

namespace MCBS.BlockForms
{
    /// <summary>
    /// 控件基类
    /// </summary>
    public abstract partial class Control : UnmanagedBase, IControl
    {
        protected Control()
        {
            FirstHandleRightClick = false;
            FirstHandleLeftClick = false;
            FirstHandleCursorSlotChanged = false;
            FirstHandleCursorItemChanged = false;
            FirstHandleTextEditorUpdate = false;
            InvokeExternalCursorMove = false;
            KeepWhenClear = false;
            IsRenderingTransparencyTexture = true;
            IsInitCompleted = false;
            _DisplayPriority = 0;
            _MaxDisplayPriority = 512;
            _Text = string.Empty;
            _Visible = true;
            _ClientLocation = new(0, 0);
            _ClientSize = new(SR.DefaultFont.HalfWidth * 4, SR.DefaultFont.Height);
            _OffsetPosition = new(0, 0);
            _AutoSize = false;
            _BorderWidth = 1;
            Skin = new(this);
            Anchor = Direction.Top | Direction.Left;
            Stretch = Direction.None;
            _ContentAnchor = AnchorPosition.UpperLeft;
            _ControlState = ControlState.None;
            _LayoutSyncer = null;

            _needRendering = true;
            _renderingCache = null;
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

        private bool _needRendering;

        private BlockFrame? _renderingCache;

        private readonly List<CursorContext> _hoverCursors;

        public bool FirstHandleRightClick { get; set; }

        public bool FirstHandleLeftClick { get; set; }

        public bool FirstHandleCursorSlotChanged { get; set; }

        public bool FirstHandleCursorItemChanged { get; set; }

        public bool FirstHandleTextEditorUpdate { get; set; }

        public bool InvokeExternalCursorMove { get; set; }

        public bool KeepWhenClear { get; set; }

        public bool IsRenderingTransparencyTexture { get; set; }

        public bool IsInitCompleted { get; private set; }

        public IContainerControl? GenericParentContainer { get; private set; }

        public ContainerControl? ParentContainer { get; private set; }

        public int Index => ParentContainer?.GetChildControls().IndexOf(this) ?? -1;

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
                    RequestRendering();
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
                    RequestRendering();
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
                    RequestRendering();
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
                    RequestRendering();
                }
            }
        }
        private Point _OffsetPosition;

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
            get => new(ClientSize.Width + BorderWidth * 2, ClientSize.Height * BorderWidth * 2);
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
                    RequestRendering();
                }
            }
        }
        private int _BorderWidth;

        public int ParentBorderWidth => ParentContainer?.BorderWidth ?? 0;

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
            get => (ParentContainer?.Height - ParentBorderWidth ?? GetScreenPlane().Height) - (Location.Y + Height);
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
            get => (ParentContainer?.Width - ParentBorderWidth ?? GetScreenPlane().Width) - (Location.X + Width);
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
                    RequestRendering();
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
                    RequestRendering();
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
                    GenericParentContainer?.GetChildControls().Sort();
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
                    GenericParentContainer?.GetChildControls().Sort();
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
                    RequestRendering();
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
                        RequestRendering();
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
                    GenericParentContainer?.GetChildControls().Sort();
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

        public event EventHandler<Control, PositionChangedEventArgs> Move;

        public event EventHandler<Control, SizeChangedEventArgs> Resize;

        public event EventHandler<Control, PositionChangedEventArgs> OffsetPositionChanged;

        public event EventHandler<Control, TextChangedEventArgs> TextChanged;

        public event EventHandler<Control, SizeChangedEventArgs> Layout;

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

        protected virtual void OnMove(Control sender, PositionChangedEventArgs e) { }

        protected virtual void OnResize(Control sender, SizeChangedEventArgs e) { }

        protected virtual void OnOffsetPositionChanged(Control sender, PositionChangedEventArgs e) { }

        protected virtual void OnTextChanged(Control sender, TextChangedEventArgs e) { }

        public virtual void OnLayout(Control sender, SizeChangedEventArgs e)
        {
            Size offset = e.NewSize - e.OldSize;
            if (offset.Height != 0)
            {
                if (Anchor.HasFlag(Direction.Top) && Anchor.HasFlag(Direction.Bottom))
                {
                    double proportion = (ClientLocation.Y + Height / 2.0) / e.OldSize.Height;
                    ClientLocation = new(ClientLocation.X, (int)Math.Round(e.NewSize.Height * proportion - Height / 2.0));
                }
                else
                {
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
                    double proportion = (ClientLocation.X + Width / 2.0) / e.OldSize.Width;
                    ClientLocation = new((int)Math.Round(e.NewSize.Width * proportion - Width / 2.0), ClientLocation.Y);
                }
                else
                {
                    if (Anchor.HasFlag(Direction.Right))
                        ClientLocation = new(ClientLocation.X + offset.Width, ClientLocation.Y);
                    if (Stretch.HasFlag(Direction.Left) || Stretch.HasFlag(Direction.Right))
                        RightToBorder -= offset.Width;
                }
            }
        }

        #endregion

        #region 事件处理

        public virtual void HandleCursorMove(CursorEventArgs e)
        {
            UpdateHoverState(e);

            if (this.IncludedOnControl(e.Position) || InvokeExternalCursorMove)
            {
                CursorMove.Invoke(this, e);
            }
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

        protected virtual void HandleTextChanged(TextChangedEventArgs e)
        {
            TextChanged.Invoke(this, e);
        }

        public virtual void HandleLayout(SizeChangedEventArgs e)
        {
            Layout.Invoke(this, e);
        }

        #region TryInvoke

        protected bool TryInvokeLeftClick(CursorEventArgs e)
        {
            if (Visible && this.IncludedOnControl(e.Position))
            {
                LeftClick.Invoke(this, e);
                if ((e.NewData.LeftClickTime - e.OldData.LeftClickTime).TotalMilliseconds <= 500)
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
                if ((e.NewData.RightClickTime - e.OldData.RightClickTime).TotalMilliseconds <= 500)
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

        public virtual void OnInitCompleted1()
        {

        }

        public virtual void OnInitCompleted2()
        {

        }

        public virtual void OnInitCompleted3()
        {
            IsInitCompleted = true;
        }

        public virtual void HandleInitialize()
        {
            Initialize();
            InitializeCompleted.Invoke(this, EventArgs.Empty);
        }

        public virtual void HandleInitCompleted1()
        {
            OnInitCompleted1();
        }

        public virtual void HandleInitCompleted2()
        {
            OnInitCompleted2();
        }

        public virtual void HandleInitCompleted3()
        {
            OnInitCompleted3();
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

        #region 帧渲染处理

        protected void RequestRendering()
        {
            _needRendering = true;
            ParentContainer?.RequestRendering();
        }

        public virtual async Task<BlockFrame> GetRenderingResultAsync()
        {
            if (_needRendering || _renderingCache is null)
            {
                if (_renderingCache is IDisposable disposable)
                    disposable.Dispose();

                _renderingCache = await Task.Run(() => Rendering());
                _needRendering = false;
            }

            return _renderingCache;
        }

        protected virtual BlockFrame Rendering()
        {
            return this.RenderingBackground(ClientSize);
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

        #region 父级相关处理

        public Control GetRootControl()
        {
            Control result = this;
            while (true)
            {
                Control? parent = result.ParentContainer;
                if (parent is null)
                    return result;
                else
                    result = parent;
            }
        }

        public RootForm? GetRootForm()
        {
            IControl? result = this;
            while (true)
            {
                if (result is null)
                    return null;
                else if (result is RootForm form)
                    return form;
                else
                    result = result.GenericParentContainer;
            }
        }

        public Form? GetForm()
        {
            IControl? result = this;
            while (true)
            {
                if (result is null)
                    return null;
                else if (result is Form form)
                    return form;
                else
                    result = result.GenericParentContainer;
            }
        }

        public ProcessContext? GetProcess()
        {
            Form? form = GetForm();
            if (form is null)
                return null;

            return MCOS.Instance.ProcessOf(form);
        }

        public ScreenContext? GetScreenContext()
        {
            Form? form = GetForm();
            if (form is null)
                return null;

            return MCOS.Instance.ScreenContextOf(form);
        }

        public Size GetFormContainerSize()
        {
            IRootForm? rootForm1 = GetRootForm();
            if (rootForm1 is not null)
                return rootForm1.FormContainerSize;

            Form? form = GetForm();
            if (form is not null)
            {
                IForm? initiator = MCOS.Instance.ProcessOf(form)?.Initiator;
                if (initiator is not null)
                {
                    if (initiator is IRootForm rootForm2)
                        return rootForm2.FormContainerSize;

                    IRootForm? rootForm3 = initiator.GetRootForm();
                    if (rootForm3 is not null)
                        return rootForm3.FormContainerSize;
                }
            }

            return new Size(256, 126);
        }

        public IPlane GetScreenPlane()
        {
            Form? form = GetForm();
            if (form is not null)
            {
                Screen? screen = MCOS.Instance.ScreenContextOf(form)?.Screen;
                if (screen is not null)
                    return screen;

                IForm? initiator = MCOS.Instance.ProcessOf(form)?.Initiator;
                if (initiator is not null)
                {
                    screen = MCOS.Instance.ScreenContextOf(initiator)?.Screen;
                    if (screen is not null)
                        return screen;
                }
            }

            return new Plane(256, 144, Facing.Zp);
        }

        #endregion

        public void UpdateHoverState(CursorEventArgs e)
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

        public virtual void ClearAllLayoutSyncer()
        {
            LayoutSyncer = null;
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

        void IControl.SetGenericContainerControl(IContainerControl? container)
        {
            GenericParentContainer = container;
            if (container is null)
                ParentContainer = null;
            else if (container is ContainerControl containerControl)
                ParentContainer = containerControl;
        }
    }
}
