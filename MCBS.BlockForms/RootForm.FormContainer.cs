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
				UpdateHoverState(e);

				foreach (HoverControl hoverControl in e.CursorContext.HoverControls.Values)
				{
					if (hoverControl.Control is IForm hoverForm)
					{
						if (MCOS.Instance.FormContextOf(hoverForm) is FormContext hoverFormContext && hoverFormContext.FormState == FormState.Dragging)
							return;
					}
				}

				IForm? form = ChildControls.FirstSelected;
				if (form is null)
					return;

				if (MCOS.Instance.FormContextOf(form) is FormContext formContext &&
					formContext.FormState == FormState.Stretching &&
					formContext.StretchingContext is not null &&
					formContext.StretchingContext.CursorContext != e.CursorContext)
					return;

				form.HandleCursorMove(e.Clone(form.ParentPos2ChildPos));
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

				IForm? form = ChildControls.FirstSelected;
				if (form is null)
				{
					foreach (var control in ChildControls.Reverse())
					{
						if (control.IncludedOnControl(control.ParentPos2ChildPos(e.Position)))
						{
							_owner.TrySwitchSelectedForm(control);
							return true;
						}
					}
					return false;
				}

				FormContext? formContext = MCOS.Instance.FormContextOf(form);
				if (formContext is null)
					return form.HandleRightClick(e.Clone(form.ParentPos2ChildPos));

				Direction borders = form.GetStretchingBorders(e.Position);
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
				else if (formContext.FormState == FormState.Active)
				{
					return form.HandleRightClick(e.Clone(form.ParentPos2ChildPos));
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
