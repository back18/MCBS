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
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            FormContext formContext = MinecraftBlockScreen.Instance.FormManager.LoadForm(source, form);
            formContext.WaitForClose();
            return form.ReturnValue;
        }

        public static async Task<object?> RunFormAsync(this IProgram source, IForm form)
        {
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            FormContext formContext = MinecraftBlockScreen.Instance.FormManager.LoadForm(source, form);
            await formContext.WaitForCloseAsync();
            return form.ReturnValue;
        }

        public static FormContext[] GetForms(this IProgram source)
        {
            List<FormContext> result = new();
            foreach (var context in MinecraftBlockScreen.Instance.FormManager.Items.Values)
                if (context.Program == source)
                    result.Add(context);

            return result.ToArray();
        }

        public static ApplicationManifest? GetApplicationManifest(this IProgram source)
        {
            return MinecraftBlockScreen.Instance.ProcessContextOf(source)?.Application;
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
