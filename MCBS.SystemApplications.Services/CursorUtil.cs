using MCBS.Cursor;
using MCBS.Events;
using MCBS.Forms;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Services
{
    public static class CursorUtil
    {
        public static bool IsDragForming(CursorEventArgs e)
        {
            foreach (HoverControl hoverControl in e.CursorContext.HoverControls.Values)
            {
                if (hoverControl.Control is IForm hoverForm)
                {
                    if (MCOS.Instance.FormContextOf(hoverForm) is FormContext hoverFormContext && hoverFormContext.FormState == FormState.Dragging)
                        return true;
                }
            }

            return false;
        }

        public static bool IsStretchForming(CursorEventArgs e, IForm? form)
        {
            if (form is null)
                return false;

            if (MCOS.Instance.FormContextOf(form) is FormContext formContext &&
                formContext.FormState == FormState.Stretching &&
                formContext.StretchingContext?.CursorContext == e.CursorContext)
                return true;

            return false;
        }
    }
}
