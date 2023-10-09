using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.UI;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.Forms;
using QuanLib.Minecraft.Blocks;

namespace MCBS.BlockForms
{
    public abstract class RootForm : Form, IRootForm
    {
        public RootForm()
        {
            AllowMove = false;
            AllowResize = false;
            DisplayPriority = int.MinValue;
            MaxDisplayPriority = int.MinValue + 1;
            BorderWidth = 0;
            Skin.SetAllBackgroundBlockID(BlockManager.Concrete.LightBlue);

            FormContainer = new(this);
            TaskBar = new(this);
            StartMenu_ListMenuBox = new();
            StartMenu_Label = new();
            Light_Switch = new();
            StartSleep_Button = new();
            CloseScreen_Button = new();
            RestartScreen_Button = new();
            ShowTaskBar_Button = new();
        }

        private readonly RootFormFormContainer FormContainer;

        private readonly RootFormTaskBar TaskBar;

        private readonly ListMenuBox<Control> StartMenu_ListMenuBox;

        private readonly Label StartMenu_Label;

        private readonly Switch Light_Switch;

        private readonly Button StartSleep_Button;

        private readonly Button CloseScreen_Button;

        private readonly Button RestartScreen_Button;

        private readonly Button ShowTaskBar_Button;

        public Size FormContainerSize => FormContainer.ClientSize;

        public bool ShowTaskBar
        {
            get => ChildControls.Contains(TaskBar);
            set
            {
                if (value)
                {
                    if (!ShowTaskBar)
                    {
                        ChildControls.TryAdd(TaskBar);
                        ChildControls.Remove(ShowTaskBar_Button);
                        FormContainer?.LayoutSyncer?.Sync();
                    }
                }
                else
                {
                    if (ShowTaskBar)
                    {
                        ChildControls.Remove(TaskBar);
                        ChildControls.TryAdd(ShowTaskBar_Button);
                        FormContainer?.LayoutSyncer?.Sync();
                    }
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(TaskBar);

            ChildControls.Add(FormContainer);
            FormContainer.LayoutSyncer?.Sync();

            StartMenu_ListMenuBox.ClientSize = new(70, 20 * 5 + 2);
            StartMenu_ListMenuBox.MaxDisplayPriority = int.MaxValue;
            StartMenu_ListMenuBox.DisplayPriority = int.MaxValue - 1;
            StartMenu_ListMenuBox.Spacing = 2;
            StartMenu_ListMenuBox.Anchor = Direction.Bottom | Direction.Left;

            StartMenu_Label.Text = "==开始==";
            StartMenu_Label.ClientSize = new(64, 16);
            StartMenu_Label.Skin.SetAllBackgroundBlockID(string.Empty);
            StartMenu_ListMenuBox.AddedChildControlAndLayout(StartMenu_Label);

            Light_Switch.OnText = "点亮屏幕";
            Light_Switch.OffText = "熄灭屏幕";
            Light_Switch.ClientSize = new(64, 16);
            Light_Switch.RightClick += Light_Switch_RightClick;
            StartMenu_ListMenuBox.AddedChildControlAndLayout(Light_Switch);

            StartSleep_Button.Text = "进入休眠";
            StartSleep_Button.ClientSize = new(64, 16);
            StartMenu_ListMenuBox.AddedChildControlAndLayout(StartSleep_Button);

            CloseScreen_Button.Text = "关闭屏幕";
            CloseScreen_Button.ClientSize = new(64, 16);
            CloseScreen_Button.RightClick += CloseScreen_Button_RightClick;
            StartMenu_ListMenuBox.AddedChildControlAndLayout(CloseScreen_Button);

            RestartScreen_Button.Text = "重启屏幕";
            RestartScreen_Button.ClientSize = new(64, 16);
            RestartScreen_Button.RightClick += RestartScreen_Button_RightClick;
            StartMenu_ListMenuBox.AddedChildControlAndLayout(RestartScreen_Button);

            ShowTaskBar_Button.Visible = false;
            ShowTaskBar_Button.InvokeExternalCursorMove = true;
            ShowTaskBar_Button.ClientSize = new(16, 16);
            ShowTaskBar_Button.LayoutSyncer = new(this, (sender, e) => { }, (sender, e) =>
            ShowTaskBar_Button.LayoutLeft(this, e.NewSize.Height - ShowTaskBar_Button.Height, 0));
            ShowTaskBar_Button.Anchor = Direction.Bottom | Direction.Right;
            ShowTaskBar_Button.Skin.SetAllBackgroundImage(TextureManager.Instance["Shrink"]);
            ShowTaskBar_Button.CursorEnter += ShowTaskBar_Button_CursorEnter;
            ShowTaskBar_Button.CursorLeave += ShowTaskBar_Button_CursorLeave;
            ShowTaskBar_Button.RightClick += ShowTaskBar_Button_RightClick;
        }

        private void Light_Switch_RightClick(Control sender, CursorEventArgs e)
        {
            if (Light_Switch.IsSelected)
                MCOS.Instance.ScreenContextOf(this)?.Screen.CloseLight();
            else
                MCOS.Instance.ScreenContextOf(this)?.Screen.OpenLight();
        }

        private void CloseScreen_Button_RightClick(Control sender, CursorEventArgs e)
        {
            MCOS.Instance.ScreenContextOf(this)?.UnloadScreen();
        }

        private void RestartScreen_Button_RightClick(Control sender, CursorEventArgs e)
        {
            MCOS.Instance.ScreenContextOf(this)?.RestartScreen();
        }

        private void ShowTaskBar_Button_CursorEnter(Control sender, CursorEventArgs e)
        {
            ShowTaskBar_Button.Visible = true;
        }

        private void ShowTaskBar_Button_CursorLeave(Control sender, CursorEventArgs e)
        {
            ShowTaskBar_Button.Visible = false;
        }

        private void ShowTaskBar_Button_RightClick(Control sender, CursorEventArgs e)
        {
            ShowTaskBar = true;
        }

        public void AddForm(IForm form)
        {
            if (form == this)
                return;
            FormContainer.ChildControls.Add(form);
            TrySwitchSelectedForm(form);
        }

        public bool RemoveForm(IForm form)
        {
            if (!FormContainer.ChildControls.Remove(form))
                return false;

            form.IsSelected = false;
            SelectedMaxDisplayPriority();
            return true;
        }

        public bool ContainsForm(IForm form)
        {
            return FormContainer.ChildControls.Contains(form);
        }

        public IEnumerable<IForm> GetAllForm()
        {
            return FormContainer.ChildControls;
        }

        public bool TrySwitchSelectedForm(IForm form)
        {
            if (form is null)
                throw new ArgumentNullException(nameof(form));

            if (!FormContainer.ChildControls.Contains(form))
                return false;
            if (!form.AllowSelected)
                return false;

            var selecteds = FormContainer.ChildControls.GetSelecteds();
            foreach (var selected in selecteds)
            {
                if (!selected.AllowDeselected)
                    return false;
            }

            form.IsSelected = true;
            foreach (var electeds in selecteds)
            {
                electeds.IsSelected = false;
            }

            TaskBar.SwitchSelectedForm(form);
            return true;
        }

        public void SelectedMaxDisplayPriority()
        {
            if (FormContainer.ChildControls.Count > 0)
            {
                for (int i = FormContainer.ChildControls.Count - 1; i >= 0; i--)
                {
                    if (FormContainer.ChildControls[i].AllowSelected)
                    {
                        FormContainer.ChildControls[i].IsSelected = true;
                        TaskBar.SwitchSelectedForm(FormContainer.ChildControls[i]);
                        break;
                    }
                }
            }
        }

        public class RootFormFormContainer : GenericPanel<IForm>
        {
            public RootFormFormContainer(RootForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));

                BorderWidth = 0;
                LayoutSyncer = new(_owner,
                (sender, e) => { },
                (sender, e) =>
                {
                    if (_owner.ShowTaskBar)
                    {
                        ClientSize = new(e.NewSize.Width, e.NewSize.Height - _owner.TaskBar.Height);
                        foreach (var form in ChildControls)
                            form.ClientSize = new(form.ClientSize.Width, form.ClientSize.Height - _owner.TaskBar.Height);
                    }
                    else
                    {
                        ClientSize = new(e.NewSize.Width, e.NewSize.Height);
                        foreach (var form in ChildControls)
                            form.ClientSize = new(form.ClientSize.Width, form.ClientSize.Height + _owner.TaskBar.Height);
                    }
                });
            }

            private readonly RootForm _owner;

            public override void Initialize()
            {
                base.Initialize();

                if (_owner != ParentContainer)
                    throw new InvalidOperationException();
            }

            public override void HandleCursorMove(CursorEventArgs e)
            {
                foreach (var control in ChildControls.Reverse())
                {
                    if (control.IsSelected)
                    {
                        control.HandleCursorMove(e.Clone(control.ParentPos2ChildPos));
                    }
                }

                UpdateHoverState(e);
            }

            public override bool HandleRightClick(CursorEventArgs e)
            {
                bool result = false;
                foreach (var control in ChildControls.Reverse())
                {
                    Point child = control.ParentPos2ChildPos(e.Position);
                    if (control is Form form)
                    {
                        if (form.ResizeBorder != Direction.None)
                        {
                            form.Resizeing = !form.Resizeing;
                            result = true;
                            break;
                        }
                        else if (form.IncludedOnControl(child))
                        {
                            if (form.IsSelected)
                                form.HandleRightClick(e.Clone(control.ParentPos2ChildPos));
                            else
                                _owner.TrySwitchSelectedForm(form);
                            result = true;
                            break;
                        }
                    }
                }

                if (result)
                    return true;
                else if (ChildControls.FirstHover is null)
                    return false;
                else
                    return ChildControls.FirstHover.HandleRightClick(e.Clone(ChildControls.FirstHover.ParentPos2ChildPos));
            }

            public override bool HandleLeftClick(CursorEventArgs e)
            {
                bool result = false;
                foreach (var control in ChildControls.Reverse())
                {
                    if (control.IsSelected)
                    {
                        result = control.HandleLeftClick(e.Clone(control.ParentPos2ChildPos));
                    }
                }

                return result;
            }

            public override bool HandleTextEditorUpdate(CursorEventArgs e)
            {
                bool result = false;
                foreach (var control in ChildControls.Reverse())
                {
                    if (control.IsSelected)
                    {
                        result = control.HandleTextEditorUpdate(e.Clone(control.ParentPos2ChildPos));
                    }
                }

                return result;
            }

            public override bool HandleCursorSlotChanged(CursorEventArgs e)
            {
                bool result = false;
                foreach (var control in ChildControls.Reverse())
                {
                    if (control.IsSelected)
                    {
                        result = control.HandleCursorSlotChanged(e.Clone(control.ParentPos2ChildPos));
                    }
                }

                return result;
            }

            public override bool HandleCursorItemChanged(CursorEventArgs e)
            {
                bool result = false;
                foreach (var control in ChildControls.Reverse())
                {
                    if (control.IsSelected)
                    {
                        result = control.HandleCursorItemChanged(e.Clone(control.ParentPos2ChildPos));
                    }
                }

                return result;
            }
        }

        public class RootFormTaskBar : ContainerControl<Control>
        {
            public RootFormTaskBar(RootForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));

                BorderWidth = 0;
                Height = 18;
                LayoutSyncer = new(_owner, (sender, e) => { }, (sender, e) =>
                {
                    Width = e.NewSize.Width;
                    ClientLocation = new(0, e.NewSize.Height - Height);
                });
                Skin.SetAllBackgroundBlockID(BlockManager.Concrete.White);

                StartMenu_Switch = new();
                FormsMenu = new();
                FullScreen_Button = new();
            }

            private readonly RootForm _owner;

            private readonly Switch StartMenu_Switch;

            private readonly Button FullScreen_Button;

            private readonly TaskBarIconMenu FormsMenu;

            public override void Initialize()
            {
                base.Initialize();

                if (_owner != ParentContainer)
                    throw new InvalidOperationException();

                ChildControls.Add(StartMenu_Switch);
                StartMenu_Switch.BorderWidth = 0;
                StartMenu_Switch.ClientLocation = new(0, 1);
                StartMenu_Switch.ClientSize = new(16, 16);
                StartMenu_Switch.Anchor = Direction.Bottom | Direction.Left;
                StartMenu_Switch.Skin.IsRenderedImageBackground = true;
                StartMenu_Switch.Skin.BackgroundBlockID = Skin.BackgroundBlockID;
                StartMenu_Switch.Skin.BackgroundBlockID_Hover = Skin.BackgroundBlockID;
                StartMenu_Switch.Skin.BackgroundBlockID_Selected = BlockManager.Concrete.Orange;
                StartMenu_Switch.Skin.BackgroundBlockID_Hover_Selected = BlockManager.Concrete.Orange;
                StartMenu_Switch.Skin.SetAllBackgroundImage(TextureManager.Instance["Logo"]);
                StartMenu_Switch.ControlSelected += StartMenu_Switch_ControlSelected;
                StartMenu_Switch.ControlDeselected += StartMenu_Switch_ControlDeselected; ;

                ChildControls.Add(FullScreen_Button);
                FullScreen_Button.BorderWidth = 0;
                FullScreen_Button.ClientSize = new(16, 16);
                FullScreen_Button.LayoutLeft(this, 1, 0);
                FullScreen_Button.Anchor = Direction.Bottom | Direction.Right;
                FullScreen_Button.Skin.IsRenderedImageBackground = true;
                FullScreen_Button.Skin.BackgroundBlockID = Skin.BackgroundBlockID;
                FullScreen_Button.Skin.BackgroundBlockID_Hover = BlockManager.Concrete.LightGray;
                FullScreen_Button.Skin.BackgroundBlockID_Hover_Selected = BlockManager.Concrete.LightGray;
                FullScreen_Button.Skin.SetAllBackgroundImage(TextureManager.Instance["Expand"]);
                FullScreen_Button.RightClick += HideTitleBar_Button_RightClick;

                ChildControls.Add(FormsMenu);
                FormsMenu.Spacing = 0;
                FormsMenu.MinWidth = 18;
                FormsMenu.BorderWidth = 0;
                FormsMenu.ClientSize = new(ClientSize.Width - StartMenu_Switch.Width - FullScreen_Button.Width, ClientSize.Height);
                FormsMenu.ClientLocation = new(StartMenu_Switch.RightLocation + 1, 0);
                FormsMenu.Stretch = Direction.Right;

                _owner.FormContainer.AddedChildControl += FormContainer_AddedChildControl;
                _owner.FormContainer.RemovedChildControl += FormContainer_RemovedChildControl;
            }

            public void SwitchSelectedForm(IForm form)
            {
                FormsMenu.SwitchSelectedForm(form);
            }

            private void StartMenu_Switch_ControlSelected(Control sender, EventArgs e)
            {
                _owner.ChildControls.TryAdd(_owner.StartMenu_ListMenuBox);

                _owner.StartMenu_ListMenuBox.ClientLocation = new(0, Math.Max(_owner.ClientSize.Height - _owner.TaskBar.Height - _owner.StartMenu_ListMenuBox.Height, 0));
                if (_owner.StartMenu_ListMenuBox.BottomToBorder < _owner.TaskBar.Height)
                    _owner.StartMenu_ListMenuBox.BottomToBorder = _owner.TaskBar.Height;

                if (MCOS.Instance.ScreenContextOf(_owner)?.Screen.TestLight() ?? false)
                    _owner.Light_Switch.IsSelected = false;
                else
                    _owner.Light_Switch.IsSelected = true;
            }

            private void StartMenu_Switch_ControlDeselected(Control sender, EventArgs e)
            {
                _owner.ChildControls.Remove(_owner.StartMenu_ListMenuBox);
            }

            private void FormContainer_AddedChildControl(AbstractContainer<IControl> sender, ControlEventArgs<IControl> e)
            {
                if (e.Control is IForm form && !FormsMenu.ContainsForm(form) && (MCOS.Instance.ProcessOf(form)?.ApplicationInfo.AppendToDesktop ?? false))
                {
                    FormsMenu.AddedChildControlAndLayout(new TaskBarIcon(form));
                }
            }

            private void FormContainer_RemovedChildControl(AbstractContainer<IControl> sender, ControlEventArgs<IControl> e)
            {
                if (e.Control is IForm form)
                {
                    var context = MCOS.Instance.FormContextOf(form);
                    if (context is null || context.StateManager.NextState != FormState.Closed)
                        return;

                    var icon = FormsMenu.TaskBarIconOf(form);
                    if (icon is null)
                        return;

                    FormsMenu.RemoveChildControlAndLayout(icon);
                }
            }

            private void HideTitleBar_Button_RightClick(Control sender, CursorEventArgs e)
            {
                _owner.ShowTaskBar = false;
            }
        }
    }
}
