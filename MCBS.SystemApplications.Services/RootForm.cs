using MCBS.BlockForms;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.UI;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Services
{
    public partial class RootForm : Form, IRootForm
    {
        public RootForm()
        {
            AllowDrag = false;
            IsSelected = true;
            DisplayPriority = int.MinValue;
            MaxDisplayPriority = int.MinValue + 1;
            BorderWidth = 0;
            Skin.SetAllBackgroundColor(BlockManager.Concrete.LightBlue);

            FormContainer_Control = new(this);
            TaskBar_Control = new(this);
            StartMenu_ListMenuBox = new();
            StartMenu_Label = new();
            Light_Switch = new();
            StartSleep_Button = new();
            CloseScreen_Button = new();
            RestartScreen_Button = new();
            ShowTaskBar_Button = new();
        }

        internal readonly FormContainer FormContainer_Control;

        internal readonly TaskBar TaskBar_Control;

        private readonly ListMenuBox<Control> StartMenu_ListMenuBox;

        private readonly Label StartMenu_Label;

        private readonly Switch Light_Switch;

        private readonly Button StartSleep_Button;

        private readonly Button CloseScreen_Button;

        private readonly Button RestartScreen_Button;

        private readonly Button ShowTaskBar_Button;

        public Size FormContainerSize => FormContainer_Control.ClientSize;

        public override bool IsMaximize => false;

        public bool ShowTaskBar
        {
            get => ChildControls.Contains(TaskBar_Control);
            set
            {
                if (value)
                {
                    if (!ChildControls.Contains(TaskBar_Control))
                    {
                        ChildControls.TryAdd(TaskBar_Control);
                        ChildControls.Remove(ShowTaskBar_Button);
                        FormContainer_Control.Size = new(Width, Height - TaskBar_Control.Height);
                    }
                }
                else
                {
                    if (ChildControls.Contains(TaskBar_Control))
                    {
                        ChildControls.Remove(TaskBar_Control);
                        ChildControls.TryAdd(ShowTaskBar_Button);
                        FormContainer_Control.Size = ClientSize;
                    }
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(TaskBar_Control);
            TaskBar_Control.Width = ClientSize.Width;
            TaskBar_Control.ClientLocation = new(0, ClientSize.Height - TaskBar_Control.Height);

            ChildControls.Add(FormContainer_Control);
            FormContainer_Control.Size = new(ClientSize.Width, ClientSize.Height - TaskBar_Control.Height);

            StartMenu_ListMenuBox.ClientSize = new(70, 20 * 5 + 2);
            StartMenu_ListMenuBox.MaxDisplayPriority = int.MaxValue;
            StartMenu_ListMenuBox.DisplayPriority = int.MaxValue - 1;
            StartMenu_ListMenuBox.Spacing = 2;
            StartMenu_ListMenuBox.Anchor = Direction.Bottom | Direction.Left;

            StartMenu_Label.Text = "==开始==";
            StartMenu_Label.ClientSize = new(64, 16);
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
            ShowTaskBar_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Shrink"]);
            ShowTaskBar_Button.CursorEnter += ShowTaskBar_Button_CursorEnter;
            ShowTaskBar_Button.CursorLeave += ShowTaskBar_Button_CursorLeave;
            ShowTaskBar_Button.RightClick += ShowTaskBar_Button_RightClick;
        }

        public override void HandleCursorMove(CursorEventArgs e)
        {
            UpdateHoverState(e);

            if (CursorIsBusy(e))
                return;

            base.HandleCursorMove(e);
        }

        public override bool HandleRightClick(CursorEventArgs e)
        {
            if (CursorIsBusy(e))
                return false;

            return base.HandleRightClick(e);
        }

        protected override void OnResize(Control sender, SizeChangedEventArgs e)
        {
            base.OnResize(sender, e);

            if (ShowTaskBar)
            {
                FormContainer_Control.Size = new(ClientSize.Width, ClientSize.Height - TaskBar_Control.Height);
            }
            else
            {
                FormContainer_Control.Size = ClientSize;
            }

            TaskBar_Control.Width = ClientSize.Width;
            TaskBar_Control.ClientLocation = new(0, ClientSize.Height - TaskBar_Control.Height);
        }

        private void Light_Switch_RightClick(Control sender, CursorEventArgs e)
        {
            if (Light_Switch.IsSelected)
                MCOS.Instance.ScreenContextOf(this)?.ScreenOutputHandler.FillAirBlock(1);
            else
                MCOS.Instance.ScreenContextOf(this)?.ScreenOutputHandler.FillLightBlock();
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
            FormContainer_Control.ChildControls.Add(form);
            TrySwitchSelectedForm(form);
        }

        public bool RemoveForm(IForm form)
        {
            if (!FormContainer_Control.ChildControls.Remove(form))
                return false;

            form.IsSelected = false;
            SelectedMaxDisplayPriority();
            return true;
        }

        public bool ContainsForm(IForm form)
        {
            return FormContainer_Control.ChildControls.Contains(form);
        }

        public IEnumerable<IForm> GetAllForm()
        {
            return FormContainer_Control.ChildControls;
        }

        public bool TrySwitchSelectedForm(IForm form)
        {
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            if (!FormContainer_Control.ChildControls.Contains(form))
                return false;
            if (!form.AllowSelected)
                return false;

            var selecteds = FormContainer_Control.ChildControls.GetSelecteds();
            foreach (var selected in selecteds)
            {
                if (!selected.AllowDeselected)
                    return false;
            }

            form.IsSelected = true;
            foreach (var selected in selecteds)
            {
                selected.IsSelected = false;
            }

            TaskBar_Control.SwitchSelectedForm(form);
            return true;
        }

        public void SelectedMaxDisplayPriority()
        {
            if (FormContainer_Control.ChildControls.Count > 0)
            {
                for (int i = FormContainer_Control.ChildControls.Count - 1; i >= 0; i--)
                {
                    if (FormContainer_Control.ChildControls[i].AllowSelected)
                    {
                        FormContainer_Control.ChildControls[i].IsSelected = true;
                        TaskBar_Control.SwitchSelectedForm(FormContainer_Control.ChildControls[i]);
                        break;
                    }
                }
            }
        }

        internal bool CursorIsBusy(CursorEventArgs e)
        {
            foreach (var control in GetChildControls().ToArray())
                control.UpdateHoverState(e.Clone(control.ParentPos2ChildPos));

            CursorEventArgs e2 = e.Clone(FormContainer_Control.ParentPos2ChildPos);
            if (Array.IndexOf(FormContainer_Control.GetHoverCursors(), e.CursorContext) == -1 &&
                (CursorUtil.IsDragForming(e2) || CursorUtil.IsStretchForming(e2, FormContainer_Control.ChildControls.FirstSelected)))
                return true;

            return false;
        }
    }
}
