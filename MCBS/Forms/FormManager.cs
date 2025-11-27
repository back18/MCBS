using MCBS.Application;
using MCBS.UI;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.TickLoop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Forms
{
    public partial class FormManager : UnmanagedBase, ITickUpdatable
    {
        public FormManager()
        {
            Collection = new(this);

            AddedForm += OnAddedForm;
            RemovedForm += OnRemovedForm;
        }

        private readonly Lock _lock = new();

        public FormCollection Collection { get; }

        public event EventHandler<FormManager, EventArgs<FormContext>> AddedForm;

        public event EventHandler<FormManager, EventArgs<FormContext>> RemovedForm;

        protected virtual void OnAddedForm(FormManager sender, EventArgs<FormContext> e) { }

        protected virtual void OnRemovedForm(FormManager sender, EventArgs<FormContext> e) { }

        public void OnTickUpdate(int tick)
        {
            foreach (FormContext formContext in Collection.GetForms())
            {
                formContext.OnTickUpdate(tick);
                if (formContext.FormState == FormState.Closed)
                {
                    lock (_lock)
                        Collection.RemoveForm(formContext);
                }
            }
        }

        public FormContext LoadForm(IProgram program, IForm form)
        {
            ArgumentNullException.ThrowIfNull(program, nameof(program));
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            lock (_lock)
            {
                Guid guid = Collection.PreGenerateGuid();
                FormContext formContext = new(program, form, guid);
                Collection.AddForm(formContext);
                formContext.LoadForm();
                return formContext;
            }
        }

        protected override void DisposeUnmanaged()
        {
            FormContext[] forms = Collection.GetForms();
            List<Exception>? exceptions = null;

            foreach (FormContext formContext in forms)
            {
                try
                {
                    formContext.Dispose();
                }
                catch (Exception ex)
                {
                    exceptions ??= [];
                    exceptions.Add(ex);
                }
            }

            try
            {
                Collection.ClearAllForm();
            }
            catch (AggregateException ex) when (exceptions is not null)
            {
                exceptions.Add(ex);
            }

            if (exceptions is not null && exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }
    }
}
