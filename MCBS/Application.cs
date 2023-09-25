using MCBS.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public abstract class Application
    {
        public abstract object? Main(string[] args);

        public object? RunForm(IForm form)
        {
            FormContext context = MCOS.Instance.FormManager.Items.Add(this, form);
            context.LoadForm();
            context.WaitForFormClose();
            return form.ReturnValue;
        }

        public FormContext[] GetForms()
        {
            List<FormContext> result = new();
            foreach (var context in MCOS.Instance.FormManager.Items.Values)
                if (context.Application == this)
                    result.Add(context);

            return result.ToArray();
        }

        public ApplicationInfo? GetInfo()
        {
            return MCOS.Instance.ProcessOf(this)?.ApplicationInfo;
        }

        public string? GetApplicationDirectory()
        {
            return GetInfo()?.ApplicationDirectory;
        }

        public static Application CreateApplication(Type appType)
        {
            if (appType is null)
                throw new ArgumentNullException(nameof(appType));
            if (!appType.IsSubclassOf(typeof(Application)))
                throw new ArgumentException("Type对象不是Application", nameof(appType));

            return Activator.CreateInstance(appType) as Application ?? throw new ArgumentException("无法构建Application对象", nameof(appType));
        }

        public static Application CreateApplication<T>() where T : Application
        {
            return CreateApplication(typeof(T));
        }
    }
}
