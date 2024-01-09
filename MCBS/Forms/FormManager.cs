using MCBS.Application;
using MCBS.Events;
using MCBS.UI;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Forms
{
    public partial class FormManager : ITickable
    {
        public FormManager()
        {
            Items = new(this);

            AddedForm += OnAddedForm;
            RemovedForm += OnRemovedForm;
        }

        public FormCollection Items { get; }

        public event EventHandler<FormManager, FormContextEventArgs> AddedForm;

        public event EventHandler<FormManager, FormContextEventArgs> RemovedForm;

        protected virtual void OnAddedForm(FormManager sender, FormContextEventArgs e) { }

        protected virtual void OnRemovedForm(FormManager sender, FormContextEventArgs e) { }

        public void OnTick()
        {
            foreach (var item in Items)
            {
                item.Value.OnTick();
                if (item.Value.FormState == FormState.Closed)
                    Items.TryRemove(item.Key, out _);
            }
        }

        public FormContext LoadForm(IProgram program, IForm form)
        {
            ArgumentNullException.ThrowIfNull(program, nameof(program));
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            FormContext formContext = new(program, form);
            if (!Items.TryAdd(formContext.GUID, formContext))
                throw new InvalidOperationException();

            formContext.LoadForm();
            return formContext;
        }
    }
}
