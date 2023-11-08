using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Core.Events;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract partial class WindowForm
    {
        public class TitleBar : ContainerControl<Control>
        {
            public TitleBar(WindowForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));

                base.Text = _owner.Text;
                LayoutSyncer = new(_owner, (sender, e) => { }, (sender, e) => Width = e.NewSize.Width);
                _owner.TextChanged += Owner_TextChanged;
                _owner.InitializeCompleted += Owner_InitializeCompleted;

                MoveAnchorPoint = new(0, 0);
                _ButtonsToShow = FormButtons.Close | FormButtons.MaximizeOrRestore | FormButtons.Minimize | FormButtons.FullScreen;

                Title_IconTextBox = new();
                Close_Button = new();
                MaximizeOrRestore_Switch = new();
                Minimize_Button = new();
                FullScreen_Button = new();

                BorderWidth = 0;
                InvokeExternalCursorMove = true;
            }

            private readonly WindowForm _owner;

            private readonly IconTextBox<Rgba32> Title_IconTextBox;

            private readonly Button Close_Button;

            private readonly Switch MaximizeOrRestore_Switch;

            private readonly Button Minimize_Button;

            private readonly Button FullScreen_Button;

            public override string Text
            {
                get => Title_IconTextBox.Text_Label.Text;
                set
                {
                    if (Title_IconTextBox.Text_Label.Text != value)
                    {
                        string temp = Title_IconTextBox.Text_Label.Text;
                        Title_IconTextBox.Text_Label.Text = value;
                        HandleTextChanged(new(temp, Title_IconTextBox.Text_Label.Text));
                    }
                }
            }

            public FormButtons ButtonsToShow
            {
                get => _ButtonsToShow;
                set
                {
                    _ButtonsToShow = value;
                    ActiveLayoutAll();
                }
            }
            private FormButtons _ButtonsToShow;

            public Point MoveAnchorPoint { get; set; }

            public override void Initialize()
            {
                base.Initialize();

                if (_owner != ParentContainer)
                    throw new InvalidOperationException();

                ChildControls.Add(Title_IconTextBox);
                Title_IconTextBox.KeepWhenClear = true;
                Title_IconTextBox.AutoSize = true;
                Title_IconTextBox.Icon_PictureBox.SetImage(_owner.GetIcon());
                Title_IconTextBox.Text_Label.Text = _owner.Text;
                Title_IconTextBox.BorderWidth = 0;

                Close_Button.BorderWidth = 0;
                Close_Button.ClientSize = new(16, 16);
                Close_Button.Anchor = Direction.Top | Direction.Right;
                Close_Button.FirstHandleRightClick = true;
                Close_Button.IsRenderingTransparencyTexture = false;
                Close_Button.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                Close_Button.Skin.SetBackgroundColor(BlockManager.Concrete.Red, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                Close_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Close"]);
                Close_Button.RightClick += Close_Button_RightClick;

                MaximizeOrRestore_Switch.BorderWidth = 0;
                MaximizeOrRestore_Switch.ClientSize = new(16, 16);
                MaximizeOrRestore_Switch.Anchor = Direction.Top | Direction.Right;
                MaximizeOrRestore_Switch.FirstHandleRightClick = true;
                MaximizeOrRestore_Switch.IsRenderingTransparencyTexture = false;
                MaximizeOrRestore_Switch.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                MaximizeOrRestore_Switch.Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                MaximizeOrRestore_Switch.Skin.SetBackgroundTexture(TextureManager.Instance["Maximize"], new ControlState[] { ControlState.None, ControlState.Hover });
                MaximizeOrRestore_Switch.Skin.SetBackgroundTexture(TextureManager.Instance["Restore"], new ControlState[] { ControlState.Selected, ControlState.Hover | ControlState.Selected });

                Minimize_Button.BorderWidth = 0;
                Minimize_Button.ClientSize = new(16, 16);
                Minimize_Button.Anchor = Direction.Top | Direction.Right;
                Minimize_Button.FirstHandleRightClick = true;
                Minimize_Button.IsRenderingTransparencyTexture = false;
                Minimize_Button.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                Minimize_Button.Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                Minimize_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Minimize"]);
                Minimize_Button.RightClick += Minimize_Button_RightClick;

                FullScreen_Button.BorderWidth = 0;
                FullScreen_Button.ClientSize = new(16, 16);
                FullScreen_Button.Anchor = Direction.Top | Direction.Right;
                FullScreen_Button.FirstHandleRightClick = true;
                FullScreen_Button.IsRenderingTransparencyTexture = false;
                FullScreen_Button.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                FullScreen_Button.Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                FullScreen_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Expand"]);
                FullScreen_Button.RightClick += HideTitleBar_Button_RightClick;
            }

            public override void AfterInitialize()
            {
                base.AfterInitialize();

                ActiveLayoutAll();
            }

            private void Owner_TextChanged(Control sender, TextChangedEventArgs e)
            {
                base.Text = _owner.Text;
            }

            private void Owner_InitializeCompleted(Control sender, EventArgs e)
            {
                UpdateMaximizeOrRestore();
                MaximizeOrRestore_Switch.ControlSelected += MaximizeOrRestore_Switch_ControlSelected;
                MaximizeOrRestore_Switch.ControlDeselected += MaximizeOrRestore_Switch_ControlDeselected;
            }

            protected override void OnRightClick(Control sender, CursorEventArgs e)
            {
                base.OnRightClick(sender, e);

                if (_owner.IsSelected && _owner.AllowDrag)
                    MCOS.Instance.FormContextOf(_owner)?.DragUpForm(e.CursorContext, e.Position);
            }

            private void Close_Button_RightClick(Control sender, CursorEventArgs e)
            {
                _owner.CloseForm();
            }

            private void MaximizeOrRestore_Switch_ControlSelected(Control sender, EventArgs e)
            {
                _owner.MaximizeForm();
            }

            private void MaximizeOrRestore_Switch_ControlDeselected(Control sender, EventArgs e)
            {
                _owner.RestoreForm();
            }

            private void Minimize_Button_RightClick(Control sender, CursorEventArgs e)
            {
                _owner.MinimizeForm();
            }

            private void HideTitleBar_Button_RightClick(Control sender, CursorEventArgs e)
            {
                _owner.ShowTitleBar = false;
            }

            public void UpdateMaximizeOrRestore()
            {
                if (!_owner._onresize)
                {
                    if (_owner.IsMaximize)
                        MaximizeOrRestore_Switch.IsSelected = true;
                    else
                        MaximizeOrRestore_Switch.IsSelected = false;
                }
            }

            public override void ActiveLayoutAll()
            {
                ChildControls.Clear();
                if (ButtonsToShow.HasFlag(FormButtons.Close))
                {
                    Control? control = ChildControls.RecentlyAddedControl;
                    if (control is null)
                        Close_Button.LayoutLeft(this, 0, 0);
                    else
                        Close_Button.LayoutLeft(this, control, 0);
                    ChildControls.Add(Close_Button);
                }
                if (ButtonsToShow.HasFlag(FormButtons.MaximizeOrRestore))
                {
                    Control? control = ChildControls.RecentlyAddedControl;
                    if (control is null)
                        MaximizeOrRestore_Switch.LayoutLeft(this, 0, 0);
                    else
                        MaximizeOrRestore_Switch.LayoutLeft(this, control, 0);
                    ChildControls.Add(MaximizeOrRestore_Switch);
                }
                if (ButtonsToShow.HasFlag(FormButtons.Minimize))
                {
                    Control? control = ChildControls.RecentlyAddedControl;
                    if (control is null)
                        Minimize_Button.LayoutLeft(this, 0, 0);
                    else
                        Minimize_Button.LayoutLeft(this, control, 0);
                    ChildControls.Add(Minimize_Button);
                }
                if (ButtonsToShow.HasFlag(FormButtons.FullScreen))
                {
                    Control? control = ChildControls.RecentlyAddedControl;
                    if (control is null)
                        FullScreen_Button.LayoutLeft(this, 0, 0);
                    else
                        FullScreen_Button.LayoutLeft(this, control, 0);
                    ChildControls.Add(FullScreen_Button);
                }
            }
        }
    }
}
