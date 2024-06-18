using MCBS.BlockForms.Utility;
using MCBS.Drawing;
using MCBS.Events;
using QuanLib.Core.Events;
using QuanLib.Game;
using QuanLib.Minecraft.Blocks;
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
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;

                base.Text = _owner.Text;
                LayoutSyncer = new(_owner, (sender, e) => { }, (sender, e) => Width = e.NewValue.Width);
                _owner.TextChanged += Owner_TextChanged;
                _owner.InitializeCompleted += Owner_InitializeCompleted;

                _ButtonsToShow = FormButtons.Close | FormButtons.MaximizeOrRestore | FormButtons.Minimize | FormButtons.FullScreen | FormButtons.Menu | FormButtons.Home | FormButtons.Back;

                Title_IconTextBox = new();
                Close_Button = new();
                MaximizeOrRestore_Switch = new();
                Minimize_Button = new();
                FullScreen_Button = new();
                Menu_Button = new();
                Home_Button = new();
                Back_Button = new();

                BorderWidth = 0;
                InvokeExternalCursorMove = true;
            }

            private readonly WindowForm _owner;

            private readonly IconTextBox<Rgba32> Title_IconTextBox;

            private readonly Button Close_Button;

            private readonly Switch MaximizeOrRestore_Switch;

            private readonly Button Minimize_Button;

            private readonly Button FullScreen_Button;

            private readonly Button Menu_Button;

            private readonly Button Home_Button;

            private readonly Button Back_Button;

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
                Close_Button.RequestDrawTransparencyTexture = false;
                Close_Button.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                Close_Button.Skin.SetBackgroundColor(BlockManager.Concrete.Red, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                Close_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Close"]);
                Close_Button.RightClick += Close_Button_RightClick;

                MaximizeOrRestore_Switch.BorderWidth = 0;
                MaximizeOrRestore_Switch.ClientSize = new(16, 16);
                MaximizeOrRestore_Switch.Anchor = Direction.Top | Direction.Right;
                MaximizeOrRestore_Switch.FirstHandleRightClick = true;
                MaximizeOrRestore_Switch.RequestDrawTransparencyTexture = false;
                MaximizeOrRestore_Switch.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                MaximizeOrRestore_Switch.Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                MaximizeOrRestore_Switch.Skin.SetBackgroundTexture(TextureManager.Instance["Maximize"], new ControlState[] { ControlState.None, ControlState.Hover });
                MaximizeOrRestore_Switch.Skin.SetBackgroundTexture(TextureManager.Instance["Restore"], [ControlState.Selected, ControlState.Hover | ControlState.Selected]);

                Minimize_Button.BorderWidth = 0;
                Minimize_Button.ClientSize = new(16, 16);
                Minimize_Button.Anchor = Direction.Top | Direction.Right;
                Minimize_Button.FirstHandleRightClick = true;
                Minimize_Button.RequestDrawTransparencyTexture = false;
                Minimize_Button.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                Minimize_Button.Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                Minimize_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Minimize"]);
                Minimize_Button.RightClick += Minimize_Button_RightClick;

                FullScreen_Button.BorderWidth = 0;
                FullScreen_Button.ClientSize = new(16, 16);
                FullScreen_Button.Anchor = Direction.Top | Direction.Right;
                FullScreen_Button.FirstHandleRightClick = true;
                FullScreen_Button.RequestDrawTransparencyTexture = false;
                FullScreen_Button.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                FullScreen_Button.Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                FullScreen_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Expand"]);
                FullScreen_Button.RightClick += HideTitleBar_Button_RightClick;

                Menu_Button.BorderWidth = 0;
                Menu_Button.ClientSize = new(16, 16);
                Menu_Button.Anchor = Direction.Top | Direction.Right;
                Menu_Button.FirstHandleRightClick = true;
                Menu_Button.RequestDrawTransparencyTexture = false;
                Menu_Button.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                Menu_Button.Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                Menu_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Menu"]);
                Menu_Button.RightClick += Menu_Button_RightClick;

                Home_Button.BorderWidth = 0;
                Home_Button.ClientSize = new(16, 16);
                Home_Button.Anchor = Direction.Top | Direction.Right;
                Home_Button.FirstHandleRightClick = true;
                Home_Button.RequestDrawTransparencyTexture = false;
                Home_Button.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                Home_Button.Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                Home_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Home"]);
                Home_Button.RightClick += Home_Button_RightClick;

                Back_Button.BorderWidth = 0;
                Back_Button.ClientSize = new(16, 16);
                Back_Button.Anchor = Direction.Top | Direction.Right;
                Back_Button.FirstHandleRightClick = true;
                Back_Button.RequestDrawTransparencyTexture = false;
                Back_Button.Skin.SetBackgroundColor(Skin.BackgroundColor, ControlState.None, ControlState.Selected);
                Back_Button.Skin.SetBackgroundColor(BlockManager.Concrete.LightGray, ControlState.Hover, ControlState.Hover | ControlState.Selected);
                Back_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Back"]);
            }

            public override void AfterInitialize()
            {
                base.AfterInitialize();

                ActiveLayoutAll();
            }

            private void Owner_TextChanged(Control sender, ValueChangedEventArgs<string> e)
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
                    MinecraftBlockScreen.Instance.FormContextOf(_owner)?.DragUpForm(e.CursorContext, e.Position);
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

            private void Menu_Button_RightClick(Control sender, CursorEventArgs e)
            {
                _owner.WindowPanel_Control.ActivePageKey = _owner.Menu_PagePanel.PageKey;
            }

            private void Home_Button_RightClick(Control sender, CursorEventArgs e)
            {
                _owner.WindowPanel_Control.ActivePageKey = _owner.Home_PagePanel.PageKey;
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

            public void SetTitleColor(BlockPixel color)
            {
                Title_IconTextBox.Text_Label.Skin.SetAllForegroundColor(color);
            }

            public void SetTitleColor(string color)
            {
                Title_IconTextBox.Text_Label.Skin.SetAllForegroundColor(color);
            }

            public override void ActiveLayoutAll()
            {
                ChildControls.Clear();

                AddButtonIfHas(FormButtons.Close);
                AddButtonIfHas(FormButtons.MaximizeOrRestore);
                AddButtonIfHas(FormButtons.Minimize);
                AddButtonIfHas(FormButtons.FullScreen);
                AddButtonIfHas(FormButtons.Menu);
                AddButtonIfHas(FormButtons.Home);
                AddButtonIfHas(FormButtons.Back);
            }

            private void AddButtonIfHas(FormButtons formButtons)
            {
                if (!ButtonsToShow.HasFlag(formButtons))
                    return;

                Control button = GetButton(formButtons);
                Control? control = ChildControls.RecentlyAddedControl;

                if (control is null)
                    button.LayoutLeft(this, 0, 0);
                else
                    button.LayoutLeft(this, control, 0);

                ChildControls.Add(button);
            }

            private Control GetButton(FormButtons formButtons)
            {
                return formButtons switch
                {
                    FormButtons.Close => Close_Button,
                    FormButtons.MaximizeOrRestore => MaximizeOrRestore_Switch,
                    FormButtons.Minimize => Minimize_Button,
                    FormButtons.FullScreen => FullScreen_Button,
                    FormButtons.Menu => Menu_Button,
                    FormButtons.Home => Home_Button,
                    FormButtons.Back => Back_Button,
                    _ => throw new InvalidOperationException(),
                };
            }
        }
    }
}
