﻿using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Core;
using QuanLib.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class NumberBox : TextControl
    {
        public NumberBox()
        {
            FirstHandleCursorSlotChanged = true;
            ContentAnchor = AnchorPosition.Centered;

            _NumberValue = 0;
            MinNumberValue = int.MinValue;
            MaxNumberValue = int.MaxValue;
            ScrollDelta = 1;

            NumberValueChanged += OnNumberValueChanged;
        }

        public int NumberValue
        {
            get => _NumberValue;
            set
            {
                if (value < MinNumberValue)
                    value = MinNumberValue;
                else if (value > MaxNumberValue)
                    value = MaxNumberValue;

                if (_NumberValue != value)
                {
                    int temp = _NumberValue;
                    _NumberValue = value;
                    NumberValueChanged.Invoke(this, new(temp, _NumberValue));
                }
            }
        }
        private int _NumberValue;

        public int MinNumberValue { get; set; }

        public int MaxNumberValue { get; set; }

        public int ScrollDelta { get; set; }

        public event EventHandler<NumberBox, ValueChangedEventArgs<int>> NumberValueChanged;

        protected virtual void OnNumberValueChanged(NumberBox sender, ValueChangedEventArgs<int> e)
        {
            Text = _NumberValue.ToString();
        }

        protected override void OnInitializeCompleted(Control sender, EventArgs e)
        {
            base.OnInitializeCompleted(sender, e);

            Text = NumberValue.ToString();
        }

        protected override void OnCursorSlotChanged(Control sender, CursorEventArgs e)
        {
            base.OnCursorSlotChanged(sender, e);

            NumberValue += e.InventorySlotDelta * ScrollDelta;
        }
    }
}
