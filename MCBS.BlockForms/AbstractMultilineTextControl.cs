using MCBS.Events;
using QuanLib.Core.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class AbstractMultilineTextControl : ScrollablePanel
    {
        protected AbstractMultilineTextControl()
        {
            Lines = new(Array.Empty<string>());
            _WordWrap = true;
            _AutoScroll = false;
        }

        public ReadOnlyCollection<string> Lines { get; protected set; }

        public bool WordWrap
        {
            get => _WordWrap;
            set
            {
                if (_WordWrap != value)
                {
                    _WordWrap = value;
                    UpdatePageSize();
                }
            }
        }
        private bool _WordWrap;

        public bool AutoScroll
        {
            get => _AutoScroll;
            set
            {
                if (_AutoScroll != value)
                {
                    _AutoScroll = value;
                    UpdatePageSize();
                }
            }
        }
        private bool _AutoScroll;

        protected override void OnResize(Control sender, SizeChangedEventArgs e)
        {
            base.OnResize(sender, e);

            if (AutoSize || WordWrap)
                UpdatePageSize();
        }

        protected override void OnTextChanged(Control sender, TextChangedEventArgs e)
        {
            base.OnTextChanged(sender, e);

            UpdatePageSize();

            if (AutoScroll)
            {
                int y = PageSize.Height - ClientSize.Height;
                if (y > 0)
                    OffsetPosition = new(OffsetPosition.X, y);
            }
        }

        public override void AutoSetSize()
        {
            UpdatePageSize();
        }

        protected abstract void UpdatePageSize();
    }
}
