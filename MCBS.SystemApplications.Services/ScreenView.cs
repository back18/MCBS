﻿using MCBS.BlockForms;
using MCBS.Cursor;
using MCBS.Events;
using MCBS.Forms;
using MCBS.UI;
using QuanLib.Minecraft.Blocks;
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
        }

        private readonly RootForm RootForm_Control;

        public IRootForm RootForm => RootForm_Control;

        public override void Initialize()
        {
            base.Initialize();

            ChildControls.Add(RootForm_Control);
            RootForm_Control.ClientSize = new(ClientSize.Width - 32, ClientSize.Height - 32);
            RootForm_Control.ClientLocation = new(16, 16);
        }

        public override void HandleCursorMove(CursorEventArgs e)
        {
            foreach (var control in GetChildControls().ToArray())
                control.UpdateHoverState(e.Clone(control.ParentPos2ChildPos));
            UpdateHoverState(e);

            if (CursorUtil.IsDragForming(e))
                return;

            if (MCOS.Instance.FormContextOf(RootForm_Control) is FormContext formContext &&
                formContext.FormState == FormState.Stretching &&
                formContext.StretchingContext?.CursorContext != e.CursorContext)
                return;

            RootForm_Control.HandleCursorMove(e.Clone(RootForm_Control.ParentPos2ChildPos));
        }

        public override bool HandleRightClick(CursorEventArgs e)
        {
            FormContext? formContext = MCOS.Instance.FormContextOf(RootForm_Control);
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
    }
}