using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class MultilineLabel : MultilineTextControl
    {
        public MultilineLabel()
        {
            BorderWidth = 0;
            Skin.SetAllBackgroundColor(string.Empty);

            _text = new();
        }

        private readonly StringBuilder _text;

        public override string Text
        {
            get => _text.ToString();
            set
            {
                string temp = _text.ToString();
                if (temp != value)
                {
                    _text.Clear();
                    _text.Append(value);
                    HandleTextChanged(new(temp, value));
                    RequestRendering();
                }
            }
        }
    }
}
