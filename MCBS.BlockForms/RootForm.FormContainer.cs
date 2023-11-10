using MCBS.Cursor;
using MCBS.Events;
using MCBS.Forms;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract partial class RootForm
    {
        public class FormContainer : GenericPanel<IForm>
        {
            public FormContainer(RootForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));

                BorderWidth = 0;
                LayoutSyncer = new(_owner,
                (sender, e) => { },
                (sender, e) =>
                {
                    if (_owner.ShowTaskBar)
                    {
                        ClientSize = new(e.NewSize.Width, e.NewSize.Height - _owner.TaskBar_Control.Height);
                        foreach (var form in ChildControls)
                            form.ClientSize = new(form.ClientSize.Width, form.ClientSize.Height - _owner.TaskBar_Control.Height);
                    }
                    else
                    {
                        ClientSize = new(e.NewSize.Width, e.NewSize.Height);
                        foreach (var form in ChildControls)
                            form.ClientSize = new(form.ClientSize.Width, form.ClientSize.Height + _owner.TaskBar_Control.Height);
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
                foreach (var control in GetChildControls().ToArray())
                    control.UpdateHoverState(e.Clone(control.ParentPos2ChildPos));
                UpdateHoverState(e);

                foreach (HoverControl hoverControl in e.CursorContext.HoverControls.Values)
                {
                    if (hoverControl.Control is IForm hoverForm)
                    {
                        if (MCOS.Instance.FormContextOf(hoverForm) is FormContext hoverFormContext && hoverFormContext.FormState == FormState.Dragging)
                            return;
                    }
                }

                IForm? firstSelectedForm = ChildControls.FirstSelected;
                if (firstSelectedForm is null)
                    return;

                if (MCOS.Instance.FormContextOf(firstSelectedForm) is FormContext formContext &&
                    formContext.FormState == FormState.Stretching &&
                    formContext.StretchingContext is not null &&
                    formContext.StretchingContext.CursorContext != e.CursorContext)
                    return;

                firstSelectedForm.HandleCursorMove(e.Clone(firstSelectedForm.ParentPos2ChildPos));
            }

            public override bool HandleRightClick(CursorEventArgs e)
            {
                foreach (HoverControl hoverControl in e.CursorContext.HoverControls.Values)
                {
                    if (hoverControl.Control is IForm hoverForm)
                    {
                        FormContext? hoverFormContext = MCOS.Instance.FormContextOf(hoverForm);
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

                FormContext? formContext = MCOS.Instance.FormContextOf(firstSelectedForm);
                if (formContext is null)
                    return firstSelectedForm.HandleRightClick(e.Clone(firstSelectedForm.ParentPos2ChildPos));

                Direction borders = firstSelectedForm.GetStretchingBorders(e.Position);
                if (borders != Direction.None)
                {
                    if (formContext.FormState == FormState.Stretching &&
                        formContext.StretchingContext is not null &&
                        formContext.StretchingContext.CursorContext == e.CursorContext)
                    {
                        formContext.StretchDownForm();
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
