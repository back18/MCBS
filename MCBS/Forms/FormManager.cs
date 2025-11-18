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
            Items = new(this);

            AddedForm += OnAddedForm;
            RemovedForm += OnRemovedForm;
        }

        public FormCollection Items { get; }

        public event EventHandler<FormManager, EventArgs<FormContext>> AddedForm;

        public event EventHandler<FormManager, EventArgs<FormContext>> RemovedForm;

        protected virtual void OnAddedForm(FormManager sender, EventArgs<FormContext> e) { }

        protected virtual void OnRemovedForm(FormManager sender, EventArgs<FormContext> e) { }

        public void OnTickUpdate(int tick)
        {
            foreach (var item in Items)
            {
                item.Value.OnTickUpdate(tick);
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

        protected override void DisposeUnmanaged()
        {
            Guid[] guids = Items.Keys.ToArray();
            for (int i = 0; i < guids.Length; i++)
            {
                Guid guid = guids[i];
                if (Items.TryGetValue(guid, out var formContext))
                {
                    formContext.Dispose();
                    Items.TryRemove(guid, out _);
                }
            }
        }
    }
}
