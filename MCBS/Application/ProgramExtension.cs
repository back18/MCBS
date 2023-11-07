using MCBS.Forms;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public static class ProgramExtension
    {
        public static object? RunForm(this IProgram source, IForm form)
        {
            if (form is null)
                throw new ArgumentNullException(nameof(form));

            FormContext context = MCOS.Instance.FormManager.Items.Add(source, form).LoadForm();
            context.WaitForClose();
            return form.ReturnValue;
        }

        public static async Task<object?> RunFormAsync(this IProgram source, IForm form)
        {
            if (form is null)
                throw new ArgumentNullException(nameof(form));

            FormContext context = MCOS.Instance.FormManager.Items.Add(source, form).LoadForm();
            await context.WaitForCloseAsync();
            return form.ReturnValue;
        }

        public static FormContext[] GetForms(this IProgram source)
        {
            List<FormContext> result = new();
            foreach (var context in MCOS.Instance.FormManager.Items.Values)
                if (context.Program == source)
                    result.Add(context);

            return result.ToArray();
        }

        public static ApplicationManifest? GetApplicationManifest(this IProgram source)
        {
            return MCOS.Instance.ProcessContextOf(source)?.Application;
        }

        public static string? GetApplicationDirectory(this IProgram source)
        {
            ApplicationManifest? applicationManifest = source.GetApplicationManifest();
            if (applicationManifest is null)
                return null;

            return SR.McbsDirectory.ApplicationsDir.GetApplicationDirectory(applicationManifest.ID);
        }
    }
}
