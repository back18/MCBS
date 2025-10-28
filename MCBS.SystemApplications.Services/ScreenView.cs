using MCBS.BlockForms;
using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using MCBS.Events;
using MCBS.Forms;
using MCBS.Screens;
using MCBS.UI;
using MCBS.UI.Extensions;
using QuanLib.Core.Events;
using QuanLib.Game;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Services
{
    public class ScreenView : ContainerControl<RootForm>, IScreenView
    {
        public ScreenView()
        {
            BorderWidth = 0;
            Skin.SetAllBackgroundColor("minecraft:air");

            RootForm_Control = new();

            _isBoundController = false;
            _failedFacings = [];
        }

        private bool _isBoundController;

        private Rectangle _oldRectangle;

        private readonly HashSet<Facing> _failedFacings;

        private readonly RootForm RootForm_Control;

        public IRootForm RootForm => RootForm_Control;

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(RootForm_Control);
            RootForm_Control.ClientSize = ClientSize - new Size(32);
            RootForm_Control.Location = new Point(16, 16);

            if (MinecraftBlockScreen.Instance.ScreenContextOf(RootForm) is ScreenContext screenContext)
            {
                screenContext.ScreenController.CheckRangeFailed += ScreenController_CheckRangeFailed;
                screenContext.ScreenController.Move += ScreenController_Move;
                screenContext.ScreenController.Resize += ScreenController_Resize;
                _isBoundController = true;
            }
        }

        public override bool HandleCursorMove(CursorEventArgs e)
        {
            foreach (var control in GetChildControls().ToArray())
                control.UpdateHoverState(e.Clone(control.ParentPos2ChildPos));
            UpdateHoverState(e);

            if (CursorUtil.IsDragForming(e))
                return false;

            if (MinecraftBlockScreen.Instance.FormContextOf(RootForm_Control) is FormContext formContext &&
                formContext.FormState == FormState.Stretching &&
                formContext.StretchingContext?.CursorContext != e.CursorContext)
                return false;

            return RootForm_Control.HandleCursorMove(e.Clone(RootForm_Control.ParentPos2ChildPos));
        }

        public override bool HandleRightClick(CursorEventArgs e)
        {
            FormContext? formContext = MinecraftBlockScreen.Instance.FormContextOf(RootForm_Control);
            if (formContext is null)
                return false;

            Direction borders;
            if (formContext.FormState == FormState.Stretching && formContext.StretchingContext?.CursorContext == e.CursorContext)
                borders = formContext.StretchingContext.Borders;
            else
                borders = RootForm_Control.GetStretchingBorders(e.Position);

            if (borders != Direction.None)
            {
                if (formContext.FormState == FormState.Stretching && formContext.StretchingContext?.CursorContext == e.CursorContext)
                {
                    formContext.StretchDownForm();
                    e.CursorContext.StyleType = Form.GetCursorStyleType(RootForm_Control.GetStretchingBorders(e.Position));
                    return true;
                }
                else if (formContext.FormState == FormState.Active)
                {
                    formContext.StretchUpForm(e.CursorContext, borders);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (ChildControls.FirstHover == RootForm_Control && formContext.FormState == FormState.Active)
            {
                return RootForm_Control.HandleRightClick(e.Clone(RootForm_Control.ParentPos2ChildPos));
            }
            else
            {
                return false;
            }
        }

        protected override void OnAfterFrame(Control sender, EventArgs e)
        {
            base.OnAfterFrame(sender, e);

            if (!IsInitCompleted)
                return;

            if (!_isBoundController)
                return;

            Rectangle rectangle = RootForm_Control.GetRectangle();
            Rectangle standardRectangle = GetStandardRectangle();
            if (rectangle == standardRectangle)
                return;

            if (rectangle == _oldRectangle)
                return;

            ScreenController? screenController = MinecraftBlockScreen.Instance.ScreenContextOf(RootForm_Control)?.ScreenController;
            if (screenController is null)
                return;

            Point posOffset = new(rectangle.X - standardRectangle.X, rectangle.Y - standardRectangle.Y);
            screenController.ApplyTranslate(posOffset);
            screenController.SetSize(rectangle.Width + 32, rectangle.Height + 32);
            _oldRectangle = rectangle;
        }

        protected override BlockFrame Drawing()
        {
            if (_failedFacings.Count == 0)
                return base.Drawing();

            BlockFrame background = base.Drawing();
            background.DrawHorizontalLine(0, BlockManager.Concrete.Red);
            background.DrawHorizontalLine(background.Height - 1, BlockManager.Concrete.Red);
            background.DrawVerticalLine(0, BlockManager.Concrete.Red);
            background.DrawVerticalLine(background.Width - 1, BlockManager.Concrete.Red);
            return background;
        }

        private void ScreenController_CheckRangeFailed(ScreenController sender, EventArgs<ScreenController.FacingRange> e)
        {
            _failedFacings.Add(e.Argument.Facing);
            RequestRedraw();
        }

        private void ScreenController_Move(ScreenController sender, ValueChangedEventArgs<Vector3<int>> e)
        {
            if (_failedFacings.Count > 0)
            {
                _failedFacings.Clear();
                RequestRedraw();
            }
        }

        private void ScreenController_Resize(ScreenController sender, ValueChangedEventArgs<Size> e)
        {
            if (_failedFacings.Count > 0)
            {
                _failedFacings.Clear();
                RequestRedraw();
            }
        }

        private Rectangle GetStandardRectangle()
        {
            return new(16, 16, Size.Width - 32, Size.Height - 32);
        }
    }
}
