﻿using MCBS.BlockForms.Utility;
using MCBS.UI;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.DialogBox
{
    public abstract class DialogBoxForm<R> : WindowForm
    {
        protected DialogBoxForm(IForm initiator, string title)
        {
            ArgumentNullException.ThrowIfNull(initiator, nameof(initiator));
            ArgumentNullException.ThrowIfNull(title, nameof(title));

            _initiator = initiator;
            _title = title;

            AllowDeselected = false;
            AllowStretch = false;
            TitleBar_Control.ButtonsToShow = FormButtons.Close;
        }

        protected readonly IForm _initiator;

        protected readonly string _title;

        public abstract R DefaultResult { get; }

        public abstract R DialogResult { get; protected set; }

        public override object? ReturnValue => DialogResult;

        public override void AfterInitialize()
        {
            base.AfterInitialize();

            Text = _title;
        }

        public void CenterOnInitiatorForm()
        {
            ClientLocation = new(
                _initiator.ClientLocation.X + (_initiator.ClientSize.Width + _initiator.BorderWidth - Width) / 2,
                _initiator.ClientLocation.Y + (_initiator.ClientSize.Height + _initiator.BorderWidth - Height) / 2);
        }
    }
}
