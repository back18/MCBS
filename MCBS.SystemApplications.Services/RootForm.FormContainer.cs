﻿using MCBS.BlockForms;
using MCBS.Cursor;
using MCBS.Events;
using MCBS.Forms;
using MCBS.UI;
using MCBS.UI.Extensions;
using QuanLib.Core.Events;
using QuanLib.Game;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Services
{
    public partial class RootForm
    {
        public class FormContainer : GenericPanel<IForm>
        {
            public FormContainer(RootForm owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;

                BorderWidth = 0;
            }

            private readonly RootForm _owner;

            protected override void OnResize(Control sender, ValueChangedEventArgs<Size> e)
            {
                base.OnResize(sender, e);

                Size offset = e.NewValue - e.OldValue;
                foreach (var form in ChildControls)
                    form.ClientSize += offset;
            }

            public override bool HandleCursorMove(CursorEventArgs e)
            {
                foreach (var control in GetChildControls().ToArray())
                    control.UpdateHoverState(e.Clone(control.ParentPos2ChildPos));
                UpdateHoverState(e);

                if (CursorUtil.IsDragForming(e))
                    return false;

                IForm? firstSelectedForm = ChildControls.FirstSelected;
                if (firstSelectedForm is null)
                    return false;

                if (MinecraftBlockScreen.Instance.FormContextOf(firstSelectedForm) is FormContext formContext &&
                    formContext.FormState == FormState.Stretching &&
                    formContext.StretchingContext?.CursorContext != e.CursorContext)
                    return false;

                return firstSelectedForm.HandleCursorMove(e.Clone(firstSelectedForm.ParentPos2ChildPos));
            }

            public override bool HandleRightClick(CursorEventArgs e)
            {
                foreach (HoverControl hoverControl in e.CursorContext.HoverControls.Values)
                {
                    if (hoverControl.Control is IForm hoverForm)
                    {
                        FormContext? hoverFormContext = MinecraftBlockScreen.Instance.FormContextOf(hoverForm);
                        if (hoverFormContext is not null && hoverFormContext.FormState == FormState.Dragging)
                        {
                            hoverFormContext.DragDownForm(_owner);
                            return true;
                        }
                    }
                }

                IForm? firstSelectedForm = ChildControls.FirstSelected;
                IForm? firstHoverForm = ChildControls.FirstHover;

                if (firstSelectedForm is null)
                {
                    if (firstHoverForm is null)
                        return false;
                    return _owner.TrySwitchSelectedForm(firstHoverForm);
                }

                FormContext? formContext = MinecraftBlockScreen.Instance.FormContextOf(firstSelectedForm);
                if (formContext is null)
                    return firstSelectedForm.HandleRightClick(e.Clone(firstSelectedForm.ParentPos2ChildPos));

                Direction borders;
                if (formContext.FormState == FormState.Stretching && formContext.StretchingContext?.CursorContext == e.CursorContext)
                    borders = formContext.StretchingContext.Borders;
                else
                    borders = firstSelectedForm.GetStretchingBorders(e.Position);

                if (borders != Direction.None)
                {
                    if (formContext.FormState == FormState.Stretching && formContext.StretchingContext?.CursorContext == e.CursorContext)
                    {
                        formContext.StretchDownForm();
                        e.CursorContext.StyleType = GetCursorStyleType(firstSelectedForm.GetStretchingBorders(e.Position));
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
                else if (firstHoverForm is not null && firstHoverForm != firstSelectedForm)
                {
                    return _owner.TrySwitchSelectedForm(firstHoverForm);
                }
                else if (formContext.FormState == FormState.Active)
                {
                    return firstSelectedForm.HandleRightClick(e.Clone(firstSelectedForm.ParentPos2ChildPos));
                }
                else
                {
                    return false;
                }
            }

            public override bool HandleLeftClick(CursorEventArgs e)
            {
                IForm? form = ChildControls.FirstSelected;
                return form is not null && form.HandleLeftClick(e.Clone(form.ParentPos2ChildPos));
            }

            public override bool HandleTextEditorUpdate(CursorEventArgs e)
            {
                IForm? form = ChildControls.FirstSelected;
                return form is not null && form.HandleTextEditorUpdate(e.Clone(form.ParentPos2ChildPos));
            }

            public override bool HandleCursorSlotChanged(CursorEventArgs e)
            {
                IForm? form = ChildControls.FirstSelected;
                return form is not null && form.HandleCursorSlotChanged(e.Clone(form.ParentPos2ChildPos));
            }

            public override bool HandleCursorItemChanged(CursorEventArgs e)
            {
                IForm? form = ChildControls.FirstSelected;
                return form is not null && form.HandleCursorItemChanged(e.Clone(form.ParentPos2ChildPos));
            }
        }
    }
}
